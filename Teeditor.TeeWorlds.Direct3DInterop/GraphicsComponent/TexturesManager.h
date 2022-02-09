#include <GraphicsComponent/Enumerations.h>
#pragma once

using namespace ::DirectX;
using namespace Microsoft::Graphics::Canvas;
using namespace Microsoft::WRL;
using namespace Windows::Foundation::Numerics;
using namespace Windows::Graphics::DirectX::Direct3D11;
using namespace Teeditor_TeeWorlds_Direct3DInterop::Enumerations;

namespace Teeditor_TeeWorlds_Direct3DInterop
{
    enum
    {
        NUMTILES_DIMENSION = 16, // Number of tiles in each dimension within a texture
        MAX_TEXTURES = 1024 * 4,
        PIXEL_SIZE = 4
    };

    enum WIC_LOADER_FLAGS : uint32_t
    {
        WIC_LOADER_DEFAULT = 0,
        WIC_LOADER_FORCE_SRGB = 0x1,
        WIC_LOADER_IGNORE_SRGB = 0x2,
        WIC_LOADER_SRGB_DEFAULT = 0x4,
        WIC_LOADER_FIT_POW2 = 0x20,
        WIC_LOADER_MAKE_SQUARE = 0x40,
        WIC_LOADER_FORCE_RGBA32 = 0x80,
    };

    class CTexture
    {
    public:

        ComPtr<ID3D11ShaderResourceView> m_pTextureArray;
        ComPtr<ID3D11ShaderResourceView> m_pTexture;

        uint8_t* m_pData;
        unsigned int m_dataSize;

        unsigned int m_width;
        unsigned int m_height;

        int m_memSize;

        ~CTexture()
        {
            // Check for bugs
            //delete[] m_pData;
            //m_pData = 0x0;
            //m_Used = 0;
            //m_Size = 0;
        }
    };

    public ref class TextureHandle sealed
    {
    public:

        TextureHandle() {
            m_Id = -1;
        }

        TextureHandle(int id) {
            m_Id = id;
        }

        property bool IsValid {
            bool get() {
                return m_Id >= 0;
            }
        }

        property int Id {
            int get() {
                return m_Id;
            }
        }

        void Invalidate() { m_Id = -1; }

    private:
        int m_Id;
    };

    public ref class TexturesManager sealed
    {
    public:
        TexturesManager(ICanvasResourceCreator^ resourceCreator);

        TextureHandle^ AddTexture(const Platform::Array<unsigned char>^ rawData, UINT width, UINT height);

        void LoadTexture(TextureHandle^ textureHandle);
        void LoadTextureArray(TextureHandle^ textureHandle);

        bool IsTextureLoaded(TextureHandle^ textureHandle);
        bool IsTextureArrayLoaded(TextureHandle^ textureHandle);

        void UnloadTexture(TextureHandle^ textureHandle);


        TextureHandle^ InitInvalidTexture();

    private:
        int GrabFreeTextureId()
        {
            int id = m_firstFreeTextureId;
            m_firstFreeTextureId = m_aTextureIndices[id];
            m_aTextureIndices[id] = -1;

            return id;
        }

        void InitMipSizesArray()
        {
            UINT textureMaxDimension = D3D11_REQ_TEXTURE2D_U_OR_V_DIMENSION;
            UINT iDimension = 1;

            while (iDimension * 2 <= textureMaxDimension)
            {
                m_aMipSizes.push_back(iDimension);
                iDimension *= 2;
            }
        }

        void LoadInvalidTexture(int id);
        void LoadInvalidTextureArray(int id);

        ComPtr<ID3D11Device> m_pDevice;
        ComPtr<ID3D11DeviceContext> m_pDeviceContext;

        UINT m_textureMemoryUsage;
        std::vector<UINT> m_aMipSizes;
        int m_aTextureIndices[MAX_TEXTURES];
        int m_firstFreeTextureId;

        const uint32_t m_cInvalidPixel = 0xFFFFFFFF;

    internal:
        CTexture m_aTextures[MAX_TEXTURES];
        TextureHandle^ m_rInvalidTexture;
    };
}