#include "pch.h"
#include "GraphicsComponent/TexturesManager.h"

using namespace Teeditor_TeeWorlds_Direct3DInterop;

UINT CountOfDivide(UINT num, UINT seed)
{
    if (num % 2 == 0)
        return CountOfDivide(num / 2, seed + 1);
    else
        return seed;
}

bool IsDegreeOfTwo(UINT num)
{
    return num && !(num & (num - 1));
}

UINT GetMipLevels(UINT size)
{
    UINT mipLevels = 1;

    if (IsDegreeOfTwo(size))
        mipLevels = CountOfDivide(size, 1);

    return mipLevels;
}

TexturesManager::TexturesManager(ICanvasResourceCreator^ resourceCreator)
{
    __abi_ThrowIfFailed(GetDXGIInterface(resourceCreator->Device, m_pDevice.GetAddressOf()));
    m_pDevice->GetImmediateContext(&m_pDeviceContext);

    InitMipSizesArray();

    m_firstFreeTextureId = 0;

    for (int i = 0; i < MAX_TEXTURES - 1; i++)
        m_aTextureIndices[i] = i + 1;

    m_aTextureIndices[MAX_TEXTURES - 1] = -1;

    m_rInvalidTexture = InitInvalidTexture();
}

TextureHandle^ TexturesManager::AddTexture(const Platform::Array<unsigned char>^ rawData, UINT width, UINT height)
{
    int id = GrabFreeTextureId();

    TextureHandle^ textureHandle = ref new TextureHandle(id);

    m_aTextures[id].m_pData = (uint8_t*)rawData->Data;
    m_aTextures[id].m_dataSize = rawData->Length * sizeof(uint8_t);

    m_aTextures[id].m_width = width;
    m_aTextures[id].m_height = height;

    return textureHandle;
}

void TexturesManager::LoadTexture(TextureHandle^ textureHandle)
{
    if (!textureHandle->IsValid)
        return;

    if (m_aTextures[textureHandle->Id].m_pTexture)
        return;

    bool needMipsGeneration = false;
    UINT minSideSize = 0;

    if (IsDegreeOfTwo(m_aTextures[textureHandle->Id].m_width) && IsDegreeOfTwo(m_aTextures[textureHandle->Id].m_height))
    {
        needMipsGeneration = true;
        minSideSize = std::min(m_aTextures[textureHandle->Id].m_width, m_aTextures[textureHandle->Id].m_height);
    }

    unsigned char* pTexels = m_aTextures[textureHandle->Id].m_pData;

    D3D11_TEXTURE2D_DESC textureDesc;
    textureDesc.Width = m_aTextures[textureHandle->Id].m_width;
    textureDesc.Height = m_aTextures[textureHandle->Id].m_height;
    textureDesc.MipLevels = needMipsGeneration ? 0 : 1;
    textureDesc.ArraySize = 1;
    textureDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    textureDesc.SampleDesc.Count = 1;
    textureDesc.SampleDesc.Quality = 0;
    textureDesc.Usage = D3D11_USAGE_DEFAULT;
    textureDesc.BindFlags = needMipsGeneration ? D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_RENDER_TARGET : D3D11_BIND_SHADER_RESOURCE;
    textureDesc.CPUAccessFlags = 0;
    textureDesc.MiscFlags = needMipsGeneration ? D3D11_RESOURCE_MISC_GENERATE_MIPS : 0;

    ComPtr<ID3D11Texture2D> texture;
    __abi_ThrowIfFailed(m_pDevice->CreateTexture2D(&textureDesc, NULL, texture.GetAddressOf()));

    auto rowPitch = textureDesc.Width * PIXEL_SIZE;
    auto depthPitch = textureDesc.Width * textureDesc.Height * PIXEL_SIZE;

    m_pDeviceContext->UpdateSubresource(texture.Get(), 0, NULL, pTexels, rowPitch, depthPitch);

    D3D11_SHADER_RESOURCE_VIEW_DESC textureViewDesc;
    textureViewDesc.Format = textureDesc.Format;
    textureViewDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D;
    textureViewDesc.Texture2D.MostDetailedMip = 0;
    textureViewDesc.Texture2D.MipLevels = GetMipLevels(minSideSize);

    // Create final texture shader resource view
    __abi_ThrowIfFailed(m_pDevice->CreateShaderResourceView(texture.Get(), &textureViewDesc,
        m_aTextures[textureHandle->Id].m_pTexture.GetAddressOf()));

    if (needMipsGeneration)
    {
        m_pDeviceContext->GenerateMips(m_aTextures[textureHandle->Id].m_pTexture.Get());
    }

    // Count memory usage
    for (UINT i = 0; i < textureViewDesc.Texture2D.MipLevels; ++i)
    {
        UINT subresourceWidth = textureDesc.Width / m_aMipSizes[i];
        UINT subresourceHeight = textureDesc.Height / m_aMipSizes[i];
        m_aTextures[textureHandle->Id].m_memSize += subresourceWidth * subresourceHeight * PIXEL_SIZE;
    }

    m_textureMemoryUsage += m_aTextures[textureHandle->Id].m_memSize;
}

void TexturesManager::LoadTextureArray(TextureHandle^ textureHandle)
{
    if (!textureHandle->IsValid)
        return;

    if (m_aTextures[textureHandle->Id].m_pTextureArray)
        return;

    if (m_aTextures[textureHandle->Id].m_width != m_aTextures[textureHandle->Id].m_height)
    {
        textureHandle->Invalidate();
        return;
    }

    unsigned char* pTexels = m_aTextures[textureHandle->Id].m_pData;

    D3D11_TEXTURE2D_DESC initTextureDesc;
    initTextureDesc.Width = m_aTextures[textureHandle->Id].m_width;
    initTextureDesc.Height = m_aTextures[textureHandle->Id].m_height;
    initTextureDesc.MipLevels = 0;
    initTextureDesc.ArraySize = 1;
    initTextureDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    initTextureDesc.SampleDesc.Count = 1;
    initTextureDesc.SampleDesc.Quality = 0;
    initTextureDesc.Usage = D3D11_USAGE_DEFAULT;
    initTextureDesc.BindFlags = D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_RENDER_TARGET;
    initTextureDesc.CPUAccessFlags = 0;
    initTextureDesc.MiscFlags = D3D11_RESOURCE_MISC_GENERATE_MIPS;

    ComPtr<ID3D11Texture2D> initTexture;
    __abi_ThrowIfFailed(m_pDevice->CreateTexture2D(&initTextureDesc, NULL, initTexture.GetAddressOf()));

    auto rowPitch = initTextureDesc.Width * PIXEL_SIZE;
    auto depthPitch = initTextureDesc.Width * initTextureDesc.Height * PIXEL_SIZE;

    m_pDeviceContext->UpdateSubresource(initTexture.Get(), 0, NULL, pTexels, rowPitch, depthPitch);

    // Mip generate setup
    D3D11_SHADER_RESOURCE_VIEW_DESC initTextureViewDesc;
    initTextureViewDesc.Format = initTextureDesc.Format;
    initTextureViewDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D;
    initTextureViewDesc.Texture2D.MostDetailedMip = 0;
    initTextureViewDesc.Texture2D.MipLevels = GetMipLevels(initTextureDesc.Width);

    ComPtr<ID3D11ShaderResourceView> initTextureResource;

    __abi_ThrowIfFailed(m_pDevice->CreateShaderResourceView(initTexture.Get(), &initTextureViewDesc, initTextureResource.GetAddressOf()));

    m_pDeviceContext->GenerateMips(initTextureResource.Get());
    initTexture->GetDesc(&initTextureDesc);

    // Create texture array
    UINT arrTextureSize = initTextureDesc.Width / NUMTILES_DIMENSION;

    D3D11_TEXTURE2D_DESC arrTextureDesc;
    arrTextureDesc.Width = arrTextureDesc.Height = arrTextureSize;
    arrTextureDesc.MipLevels = GetMipLevels(arrTextureSize);
    arrTextureDesc.ArraySize = NUMTILES_DIMENSION * NUMTILES_DIMENSION;
    arrTextureDesc.Format = initTextureDesc.Format;
    arrTextureDesc.SampleDesc.Count = 1;
    arrTextureDesc.SampleDesc.Quality = 0;
    arrTextureDesc.Usage = D3D11_USAGE_DEFAULT;
    arrTextureDesc.BindFlags = D3D11_BIND_SHADER_RESOURCE;
    arrTextureDesc.CPUAccessFlags = 0;
    arrTextureDesc.MiscFlags = 0;

    ComPtr<ID3D11Texture2D> arrTexture;
    __abi_ThrowIfFailed(m_pDevice->CreateTexture2D(&arrTextureDesc, 0, arrTexture.GetAddressOf()));

    // Copy subresources from mipmapped initialization texture to texture array
    UINT dstSubresource;
    UINT srcSubresource;
    UINT subresourceSize;

    D3D11_BOX srcBox;
    srcBox.front = 0;
    srcBox.back = 1;

    for (UINT i = 0; i < arrTextureDesc.ArraySize; ++i)
    {
        for (UINT j = 0; j < arrTextureDesc.MipLevels; ++j)
        {
            srcSubresource = D3D11CalcSubresource(j, 0, initTextureDesc.MipLevels);
            dstSubresource = D3D11CalcSubresource(j, i, arrTextureDesc.MipLevels);

            subresourceSize = arrTextureSize / m_aMipSizes[j];

            UINT x = i % NUMTILES_DIMENSION;
            UINT y = i / NUMTILES_DIMENSION;

            srcBox.left = x * subresourceSize;
            srcBox.right = srcBox.left + subresourceSize;
            srcBox.top = y * subresourceSize;
            srcBox.bottom = srcBox.top + subresourceSize;

            m_pDeviceContext->CopySubresourceRegion(arrTexture.Get(), dstSubresource, 0, 0, 0, initTexture.Get(), srcSubresource, &srcBox);

            m_aTextures[textureHandle->Id].m_memSize += subresourceSize * subresourceSize * PIXEL_SIZE;
        }
    }

    // Create texture array shader resource view
    D3D11_SHADER_RESOURCE_VIEW_DESC texArrayViewDesc;
    texArrayViewDesc.Format = arrTextureDesc.Format;
    texArrayViewDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2DARRAY;
    texArrayViewDesc.Texture2DArray.MostDetailedMip = 0;
    texArrayViewDesc.Texture2DArray.MipLevels = arrTextureDesc.MipLevels;
    texArrayViewDesc.Texture2DArray.FirstArraySlice = 0;
    texArrayViewDesc.Texture2DArray.ArraySize = arrTextureDesc.ArraySize;

    __abi_ThrowIfFailed(m_pDevice->CreateShaderResourceView(arrTexture.Get(), &texArrayViewDesc,
        m_aTextures[textureHandle->Id].m_pTextureArray.GetAddressOf()));

    m_textureMemoryUsage += m_aTextures[textureHandle->Id].m_memSize;
}

bool TexturesManager::IsTextureLoaded(TextureHandle^ textureHandle)
{
    return textureHandle != nullptr && textureHandle->IsValid && m_aTextures[textureHandle->Id].m_pTexture;
}

bool TexturesManager::IsTextureArrayLoaded(TextureHandle^ textureHandle)
{

    return textureHandle != nullptr && textureHandle->IsValid && m_aTextures[textureHandle->Id].m_pTextureArray;
}

void TexturesManager::UnloadTexture(TextureHandle^ textureHandle)
{
    if (textureHandle->Id == m_rInvalidTexture->Id)
        return;

    if (!textureHandle->IsValid)
        return;

    m_aTextureIndices[textureHandle->Id] = m_firstFreeTextureId;
    m_firstFreeTextureId = textureHandle->Id;

    textureHandle->Invalidate();
}

TextureHandle^ TexturesManager::InitInvalidTexture()
{
    int id = GrabFreeTextureId();

    LoadInvalidTexture(id);
    LoadInvalidTextureArray(id);

    return ref new TextureHandle(id);
}

void TexturesManager::LoadInvalidTexture(int id)
{
    D3D11_SUBRESOURCE_DATA initTextureData = { &m_cInvalidPixel, PIXEL_SIZE, PIXEL_SIZE };

    D3D11_TEXTURE2D_DESC textureDesc = {};
    textureDesc.Width = textureDesc.Height = 1;
    textureDesc.MipLevels = textureDesc.ArraySize = 1;
    textureDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    textureDesc.SampleDesc.Count = 1;
    textureDesc.Usage = D3D11_USAGE_IMMUTABLE;
    textureDesc.BindFlags = D3D11_BIND_SHADER_RESOURCE;

    ComPtr<ID3D11Texture2D> texture;
    __abi_ThrowIfFailed(m_pDevice->CreateTexture2D(&textureDesc, &initTextureData, texture.GetAddressOf()));

    m_aTextures[id].m_memSize += PIXEL_SIZE;

    D3D11_SHADER_RESOURCE_VIEW_DESC textureViewDesc = {};
    textureViewDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    textureViewDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D;
    textureViewDesc.Texture2D.MipLevels = 1;

    __abi_ThrowIfFailed(m_pDevice->CreateShaderResourceView(texture.Get(),
        &textureViewDesc, m_aTextures[id].m_pTexture.GetAddressOf()));

    m_textureMemoryUsage += m_aTextures[id].m_memSize;
}

void TexturesManager::LoadInvalidTextureArray(int id)
{
    D3D11_TEXTURE2D_DESC textureArrayDesc = {};
    textureArrayDesc.Width = textureArrayDesc.Height = 1;
    textureArrayDesc.MipLevels = 1;
    textureArrayDesc.ArraySize = NUMTILES_DIMENSION * NUMTILES_DIMENSION;
    textureArrayDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    textureArrayDesc.SampleDesc.Count = 1;
    textureArrayDesc.Usage = D3D11_USAGE_DEFAULT;
    textureArrayDesc.BindFlags = D3D11_BIND_SHADER_RESOURCE;

    ComPtr<ID3D11Texture2D> textureArray;
    __abi_ThrowIfFailed(m_pDevice->CreateTexture2D(&textureArrayDesc, NULL, textureArray.GetAddressOf()));

    for (UINT i = 0; i < textureArrayDesc.ArraySize; ++i)
    {
        UINT dstSubresource = D3D11CalcSubresource(0, i, textureArrayDesc.MipLevels);

        m_pDeviceContext->UpdateSubresource(textureArray.Get(), dstSubresource, NULL, &m_cInvalidPixel, PIXEL_SIZE, PIXEL_SIZE);

        m_aTextures[id].m_memSize += PIXEL_SIZE;
    }

    D3D11_SHADER_RESOURCE_VIEW_DESC textureArrayViewDesc = {};
    textureArrayViewDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    textureArrayViewDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2DARRAY;
    textureArrayViewDesc.Texture2D.MipLevels = 1;
    textureArrayViewDesc.Texture2DArray.FirstArraySlice = 0;
    textureArrayViewDesc.Texture2DArray.ArraySize = textureArrayDesc.ArraySize;

    __abi_ThrowIfFailed(m_pDevice->CreateShaderResourceView(textureArray.Get(),
        &textureArrayViewDesc, m_aTextures[id].m_pTextureArray.GetAddressOf()));

    m_textureMemoryUsage += m_aTextures[id].m_memSize;
}