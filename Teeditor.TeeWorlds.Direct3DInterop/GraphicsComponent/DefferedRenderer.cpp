#include "pch.h"
#include <GraphicsComponent/TexturesManager.h>
#include "GraphicsComponent/DefferedRenderer.h"
#include "Shaders/Compiled/TilesEffect_TilesPixelShader.inc"
#include "Shaders/Compiled/TilesEffect_TilesVertexShader.inc"
#include "Shaders/Compiled/QuadsEffect_QuadsPixelShader.inc"
#include "Shaders/Compiled/QuadsEffect_QuadsVertexShader.inc"
#include <wincodec.h>

using namespace Teeditor_TeeWorlds_Direct3DInterop;

const D3D11_INPUT_ELEMENT_DESC VertexPositionColorTexture2DArray::InputElements[] =
{
    { "SV_Position", 0, DXGI_FORMAT_R32G32B32_FLOAT,    0, D3D11_APPEND_ALIGNED_ELEMENT, D3D11_INPUT_PER_VERTEX_DATA, 0 },
    { "COLOR",       0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT, D3D11_INPUT_PER_VERTEX_DATA, 0 },
    { "TEXCOORD",    0, DXGI_FORMAT_R32G32B32_FLOAT,    0, D3D11_APPEND_ALIGNED_ELEMENT, D3D11_INPUT_PER_VERTEX_DATA, 0 },
};

DefferedRenderer::DefferedRenderer(ICanvasResourceCreator^ resourceCreator)
{
    __abi_ThrowIfFailed(GetDXGIInterface(resourceCreator->Device, m_pDevice.GetAddressOf()));

    m_pDevice->CreateDeferredContext(0, m_pDefferedContext.GetAddressOf());

    InitStates();
    InitShaders();

    m_pTilesBatch = std::make_unique<PrimitiveBatch<VertexPositionColorTexture2DArray>>(m_pDefferedContext.Get(), MAX_INDICES, MAX_VERTICES);
    m_pQuadsBatch = std::make_unique<PrimitiveBatch<VertexPositionColorTexture>>(m_pDefferedContext.Get(), MAX_INDICES, MAX_VERTICES);

    FillIndices();
}

void DefferedRenderer::InitStates()
{
    CommonStates states(m_pDevice.Get());
    m_pBlendState = states.AlphaBlend();
    m_pDepthStencilState = states.DepthNone();
    m_pSampleState = states.AnisotropicClamp();
    m_pRasterizerState = states.CullNone();

    D3D11_RASTERIZER_DESC rasterizerDesc;
    rasterizerDesc.CullMode = D3D11_CULL_NONE;
    rasterizerDesc.FillMode = D3D11_FILL_SOLID;
    rasterizerDesc.FrontCounterClockwise = false;
    rasterizerDesc.ScissorEnable = true;
    rasterizerDesc.MultisampleEnable = true;
    rasterizerDesc.DepthBias = D3D11_DEFAULT_DEPTH_BIAS;
    rasterizerDesc.DepthBiasClamp = D3D11_DEFAULT_DEPTH_BIAS_CLAMP;
    rasterizerDesc.SlopeScaledDepthBias = D3D11_DEFAULT_SLOPE_SCALED_DEPTH_BIAS;
    rasterizerDesc.DepthClipEnable = true;
    rasterizerDesc.AntialiasedLineEnable = false;

    m_pDevice->CreateRasterizerState(&rasterizerDesc, m_pRasterizerStateScissor.GetAddressOf());
}

void DefferedRenderer::InitShaders()
{
    void const* TilesPSByteCode = TilesEffect_TilesPixelShader;
    size_t TilesPSByteCodeLength = sizeof(TilesEffect_TilesPixelShader);

    void const* TilesVSByteCode = TilesEffect_TilesVertexShader;
    size_t TilesVSCodeLength = sizeof(TilesEffect_TilesVertexShader);

    __abi_ThrowIfFailed(m_pDevice->CreatePixelShader(TilesPSByteCode, TilesPSByteCodeLength, NULL, m_pTilesPixelShader.GetAddressOf()));
    __abi_ThrowIfFailed(m_pDevice->CreateVertexShader(TilesVSByteCode, TilesVSCodeLength, NULL, m_pTilesVertexShader.GetAddressOf()));

    void const* QuadsPSByteCode = QuadsEffect_QuadsPixelShader;
    size_t QuadsPSByteCodeLength = sizeof(QuadsEffect_QuadsPixelShader);

    void const* QuadsVSByteCode = QuadsEffect_QuadsVertexShader;
    size_t QuadsVSCodeLength = sizeof(QuadsEffect_QuadsVertexShader);

    __abi_ThrowIfFailed(m_pDevice->CreatePixelShader(QuadsPSByteCode, QuadsPSByteCodeLength, NULL, m_pQuadsPixelShader.GetAddressOf()));
    __abi_ThrowIfFailed(m_pDevice->CreateVertexShader(QuadsVSByteCode, QuadsVSCodeLength, NULL, m_pQuadsVertexShader.GetAddressOf()));

    m_pDevice->CreateInputLayout(VertexPositionColorTexture2DArray::InputElements,
        VertexPositionColorTexture2DArray::InputElementCount,
        TilesVSByteCode, TilesVSCodeLength,
        m_pTilesInputLayout.GetAddressOf());

    m_pDevice->CreateInputLayout(VertexPositionColorTexture::InputElements,
        VertexPositionColorTexture::InputElementCount,
        QuadsVSByteCode, QuadsVSCodeLength,
        m_pQuadsInputLayout.GetAddressOf());
}

void DefferedRenderer::FillIndices()
{
    uint16_t index = 0;

    for (uint16_t i = 0; i < MAX_VERTICES; i += 4)
    {
        m_aIndices[index] = i;
        m_aIndices[index + 1] = i + 1;
        m_aIndices[index + 2] = i + 2;
        m_aIndices[index + 3] = i + 1;
        m_aIndices[index + 4] = i + 2;
        m_aIndices[index + 5] = i + 3;

        index += 6;
    }
}

void DefferedRenderer::AddVertices(uint16_t verticesCount)
{
    m_verticesNumber += verticesCount;
    m_indicesNumber += 6;

    if (m_verticesNumber + verticesCount < MAX_VERTICES)
        return;

    FlushVertices();
}

void DefferedRenderer::FlushVertices()
{
    if (m_verticesNumber == 0)
        return;

    if (m_drawType == DrawType::Tiles)
    {
        m_pTilesBatch->DrawIndexed(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST, m_aIndices, m_indicesNumber, m_aTilesVertices, m_verticesNumber);
    }
    else if (m_drawType == DrawType::Quads)
    {
        m_pQuadsBatch->DrawIndexed(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST, m_aIndices, m_indicesNumber, m_aQuadsVertices, m_verticesNumber);
    }

    m_indicesNumber = 0;
    m_verticesNumber = 0;
}

void DefferedRenderer::SetRenderTarget(ID3D11RenderTargetView* renderTargetView)
{
    m_pRenderTargetView = renderTargetView;
}

void DefferedRenderer::SetWrapMode(bool repeatU, bool repeatV, D3D11_FILTER filter)
{
    FlushVertices();

    m_repeatU = repeatU;
    m_repeatV = repeatV;

    UINT maxAnisotropy = (m_pDevice->GetFeatureLevel() > D3D_FEATURE_LEVEL_9_1) ? 16 : 2;

    D3D11_SAMPLER_DESC samplerDesc;
    samplerDesc.Filter = filter;
    samplerDesc.AddressU = m_repeatU ? D3D11_TEXTURE_ADDRESS_WRAP : D3D11_TEXTURE_ADDRESS_CLAMP;
    samplerDesc.AddressV = m_repeatV ? D3D11_TEXTURE_ADDRESS_WRAP : D3D11_TEXTURE_ADDRESS_CLAMP;
    samplerDesc.AddressW = D3D11_TEXTURE_ADDRESS_WRAP;
    samplerDesc.MipLODBias = 0.0f;
    samplerDesc.MaxAnisotropy = maxAnisotropy;
    samplerDesc.ComparisonFunc = D3D11_COMPARISON_NEVER;
    samplerDesc.BorderColor[0] = 0;
    samplerDesc.BorderColor[1] = 0;
    samplerDesc.BorderColor[2] = 0;
    samplerDesc.BorderColor[3] = 0;
    samplerDesc.MinLOD = 0;
    samplerDesc.MaxLOD = D3D11_FLOAT32_MAX;

    m_pDevice->CreateSamplerState(&samplerDesc, m_pSampleState.GetAddressOf());
    m_pDefferedContext->PSSetSamplers(0, 1, m_pSampleState.GetAddressOf());
}

void DefferedRenderer::SetClipping(bool isEnabled, float2 topLeft, float2 bottomRight)
{
    m_useClipping = isEnabled;

    if (m_useClipping == false)
        return;

    m_clippingRect.left = static_cast<LONG>(topLeft.x);
    m_clippingRect.top = static_cast<LONG>(topLeft.y);
    m_clippingRect.right = static_cast<LONG>(bottomRight.x);
    m_clippingRect.bottom = static_cast<LONG>(bottomRight.y);
}

void DefferedRenderer::SetView(float4x4 value)
{
    ComPtr<ID3D11Buffer> pConstantBuffer;

    // Define the constant data used to communicate with shaders
    struct Parameters
    {
        XMFLOAT4X4 MatrixTransform;
    };

    // Supply the vertex shader constant data
    Parameters vsConstData;

    XMStoreFloat4x4(&vsConstData.MatrixTransform, XMLoadFloat4x4(&value));

    // Fill in a buffer description
    D3D11_BUFFER_DESC bufferDesc;
    bufferDesc.ByteWidth = sizeof(Parameters);
    bufferDesc.Usage = D3D11_USAGE_DYNAMIC;
    bufferDesc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
    bufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
    bufferDesc.MiscFlags = 0;
    bufferDesc.StructureByteStride = 0;

    // Fill in the subresource data
    D3D11_SUBRESOURCE_DATA initData;
    initData.pSysMem = &vsConstData;
    initData.SysMemPitch = 0;
    initData.SysMemSlicePitch = 0;

    // Create the buffer
    __abi_ThrowIfFailed(m_pDevice->CreateBuffer(&bufferDesc, &initData, pConstantBuffer.GetAddressOf()));

    // Set the buffer
    m_pDefferedContext->VSSetConstantBuffers(0, 1, pConstantBuffer.GetAddressOf());
}

void DefferedRenderer::SetPosition(float2 p0, float2 p1, float2 p2, float2 p3)
{
    m_aPosition[0].x = p0.x / m_viewportWidth;
    m_aPosition[0].y = -p0.y / m_viewportHeight;

    m_aPosition[1].x = p1.x / m_viewportWidth;
    m_aPosition[1].y = -p1.y / m_viewportHeight;

    m_aPosition[2].x = p2.x / m_viewportWidth;
    m_aPosition[2].y = -p2.y / m_viewportHeight;

    m_aPosition[3].x = p3.x / m_viewportWidth;
    m_aPosition[3].y = -p3.y / m_viewportHeight;

    m_aPosition[0].z = m_aPosition[1].z = m_aPosition[2].z = m_aPosition[3].z = 0.f;
}

void DefferedRenderer::SetTextureManager(TexturesManager^ texturesManager)
{
    m_rTexturesManager = texturesManager;
}

void DefferedRenderer::SetViewportSize(float width, float height)
{
    m_viewportWidth = width;
    m_viewportHeight = height;
}

void DefferedRenderer::SetTexture(TextureHandle^ textureHandle)
{
    m_rTextureHandle = textureHandle == nullptr ? m_rTexturesManager->m_rInvalidTexture : textureHandle;
}

void DefferedRenderer::SetTextureCoordinates(float2 t0, float2 t1, float2 t2, float2 t3, float textureArrayIndex)
{
    m_aTexCoords[0].x = t0.x; m_aTexCoords[0].y = t0.y;
    m_aTexCoords[1].x = t1.x; m_aTexCoords[1].y = t1.y;
    m_aTexCoords[2].x = t2.x; m_aTexCoords[2].y = t2.y;
    m_aTexCoords[3].x = t3.x; m_aTexCoords[3].y = t3.y;

    m_aTexCoords[0].z = m_aTexCoords[1].z = m_aTexCoords[2].z = m_aTexCoords[3].z = textureArrayIndex;
}

void DefferedRenderer::SetColor(float r, float g, float b, float a)
{
    m_aColor[0] = XMFLOAT4(r, g, b, a);
    m_aColor[1] = XMFLOAT4(r, g, b, a);
    m_aColor[2] = XMFLOAT4(r, g, b, a);
    m_aColor[3] = XMFLOAT4(r, g, b, a);
}

void DefferedRenderer::SetColor4(const Platform::Array<float4>^ colors)
{
    m_aColor[0] = XMFLOAT4(colors[0].x, colors[0].y, colors[0].z, colors[0].w);
    m_aColor[1] = XMFLOAT4(colors[1].x, colors[1].y, colors[1].z, colors[1].w);
    m_aColor[2] = XMFLOAT4(colors[2].x, colors[2].y, colors[2].z, colors[2].w);
    m_aColor[3] = XMFLOAT4(colors[3].x, colors[3].y, colors[3].z, colors[3].w);
}

void DefferedRenderer::DrawBegin(DrawType drawType)
{
    m_drawType = drawType;

    if (m_drawType == DrawType::None)
        return;

    D3D11_VIEWPORT viewport = { 0, 0, m_viewportWidth, m_viewportHeight, 0, 0 };
    m_pDefferedContext->RSSetViewports(1, &viewport);

    m_pDefferedContext->OMSetBlendState(m_pBlendState.Get(), nullptr, 0xFFFFFFFF);
    m_pDefferedContext->OMSetDepthStencilState(m_pDepthStencilState.Get(), 0);
    
    if (m_useClipping == true)
    {
        m_pDefferedContext->RSSetState(m_pRasterizerStateScissor.Get());
        m_pDefferedContext->RSSetScissorRects(1, &m_clippingRect);
    }
    else
    {
        m_pDefferedContext->RSSetState(m_pRasterizerState.Get());
        m_pDefferedContext->RSSetScissorRects(0, nullptr);
    }

    if (m_drawType == DrawType::Tiles)
    {
        DrawTilesBegin();
    }
    else if (m_drawType == DrawType::Quads)
    {
        DrawQuadsBegin();
    }
}

void DefferedRenderer::DrawTilesBegin()
{
    m_pDefferedContext->IASetInputLayout(m_pTilesInputLayout.Get());
    m_pDefferedContext->VSSetShader(m_pTilesVertexShader.Get(), NULL, 0);
    m_pDefferedContext->PSSetShader(m_pTilesPixelShader.Get(), NULL, 0);

    auto textureHandleId = m_rTextureHandle->Id == -1 ? m_rTexturesManager->m_rInvalidTexture->Id : m_rTextureHandle->Id;
    auto textureSRV = m_rTexturesManager->m_aTextures[textureHandleId].m_pTextureArray;
    m_pDefferedContext->PSSetShaderResources(0, 1, textureSRV.GetAddressOf());

    SetWrapMode(false, false, D3D11_FILTER_MIN_MAG_MIP_LINEAR);

    m_pDefferedContext->OMSetRenderTargets(1, m_pRenderTargetView.GetAddressOf(), nullptr);

    m_pTilesBatch->Begin();
}

void DefferedRenderer::DrawQuadsBegin()
{
    m_pDefferedContext->IASetInputLayout(m_pQuadsInputLayout.Get());
    m_pDefferedContext->VSSetShader(m_pQuadsVertexShader.Get(), NULL, 0);
    m_pDefferedContext->PSSetShader(m_pQuadsPixelShader.Get(), NULL, 0);

    auto textureHandleId = m_rTextureHandle->Id == -1 ? m_rTexturesManager->m_rInvalidTexture->Id : m_rTextureHandle->Id;
    auto textureSRV = m_rTexturesManager->m_aTextures[textureHandleId].m_pTexture;
    m_pDefferedContext->PSSetShaderResources(0, 1, textureSRV.GetAddressOf());

    SetWrapMode(false, false, D3D11_FILTER_MIN_MAG_MIP_LINEAR);

    m_pDefferedContext->OMSetRenderTargets(1, m_pRenderTargetView.GetAddressOf(), nullptr);

    m_pQuadsBatch->Begin();
}

void DefferedRenderer::DrawEnd()
{
    if (m_drawType == DrawType::Tiles)
    {
        FlushVertices();
        m_pTilesBatch->End();
    }
    else if (m_drawType == DrawType::Quads)
    {
        FlushVertices();
        m_pQuadsBatch->End();
    }

    m_pDefferedContext->FinishCommandList(false, m_pCommandList.GetAddressOf());

    m_drawType = DrawType::None;
}

void DefferedRenderer::Draw(float2 p0, float2 p1, float2 p2, float2 p3)
{
    if (m_drawType == DrawType::None)
        return;

    SetPosition(p0, p1, p2, p3);
    
    if (m_drawType == DrawType::Tiles)
    {
        DrawTiles();
    }
    else if (m_drawType == DrawType::Quads)
    {
        DrawQuads();
    }
}

void DefferedRenderer::DrawTiles()
{
    m_aTilesVertices[m_verticesNumber].position = m_aPosition[0];
    m_aTilesVertices[m_verticesNumber].textureCoordinate = m_aTexCoords[0];
    m_aTilesVertices[m_verticesNumber].color = m_aColor[0];

    m_aTilesVertices[m_verticesNumber + 1].position = m_aPosition[1];
    m_aTilesVertices[m_verticesNumber + 1].textureCoordinate = m_aTexCoords[1];
    m_aTilesVertices[m_verticesNumber + 1].color = m_aColor[1];

    m_aTilesVertices[m_verticesNumber + 2].position = m_aPosition[2];
    m_aTilesVertices[m_verticesNumber + 2].textureCoordinate = m_aTexCoords[2];
    m_aTilesVertices[m_verticesNumber + 2].color = m_aColor[2];

    m_aTilesVertices[m_verticesNumber + 3].position = m_aPosition[3];
    m_aTilesVertices[m_verticesNumber + 3].textureCoordinate = m_aTexCoords[3];
    m_aTilesVertices[m_verticesNumber + 3].color = m_aColor[3];

    AddVertices(4);
}

void DefferedRenderer::DrawQuads()
{
    bool repeatU = false, repeatV = false;

    for (int p = 0; p < 4; p++)
    {
        if (m_aTexCoords[p].x < 0.0f || m_aTexCoords[p].x > 1.0f)
            repeatU = true;

        if (m_aTexCoords[p].y < 0.0f || m_aTexCoords[p].y > 1.0f)
            repeatV = true;
    }

    if (repeatU != m_repeatU || repeatV != m_repeatV)
    {
        SetWrapMode(repeatU, repeatV, D3D11_FILTER_MIN_MAG_MIP_LINEAR);
    }

    m_aQuadsVertices[m_verticesNumber].position = m_aPosition[0];
    m_aQuadsVertices[m_verticesNumber].textureCoordinate = XMFLOAT2(m_aTexCoords[0].x, m_aTexCoords[0].y);
    m_aQuadsVertices[m_verticesNumber].color = m_aColor[0];

    m_aQuadsVertices[m_verticesNumber + 1].position = m_aPosition[1];
    m_aQuadsVertices[m_verticesNumber + 1].textureCoordinate = XMFLOAT2(m_aTexCoords[1].x, m_aTexCoords[1].y);
    m_aQuadsVertices[m_verticesNumber + 1].color = m_aColor[1];

    m_aQuadsVertices[m_verticesNumber + 2].position = m_aPosition[2];
    m_aQuadsVertices[m_verticesNumber + 2].textureCoordinate = XMFLOAT2(m_aTexCoords[2].x, m_aTexCoords[2].y);
    m_aQuadsVertices[m_verticesNumber + 2].color = m_aColor[2];

    m_aQuadsVertices[m_verticesNumber + 3].position = m_aPosition[3];
    m_aQuadsVertices[m_verticesNumber + 3].textureCoordinate = XMFLOAT2(m_aTexCoords[3].x, m_aTexCoords[3].y);
    m_aQuadsVertices[m_verticesNumber + 3].color = m_aColor[3];

    AddVertices(4);
}