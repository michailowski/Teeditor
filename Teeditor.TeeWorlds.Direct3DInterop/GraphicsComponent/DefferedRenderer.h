#pragma once

namespace Teeditor_TeeWorlds_Direct3DInterop
{
    using namespace ::DirectX;
    using namespace Microsoft::Graphics::Canvas;
    using namespace Microsoft::WRL;
    using namespace Windows::Foundation::Numerics;
    using namespace Windows::Graphics::DirectX::Direct3D11;

    // Vertex struct holding position, color, and texturearray mapping information.
    struct VertexPositionColorTexture2DArray
    {
        VertexPositionColorTexture2DArray() = default;

        VertexPositionColorTexture2DArray(const VertexPositionColorTexture2DArray&) = default;
        VertexPositionColorTexture2DArray& operator=(const VertexPositionColorTexture2DArray&) = default;

        VertexPositionColorTexture2DArray(VertexPositionColorTexture2DArray&&) = default;
        VertexPositionColorTexture2DArray& operator=(VertexPositionColorTexture2DArray&&) = default;

        VertexPositionColorTexture2DArray(XMFLOAT3 const& position, XMFLOAT4 const& color, XMFLOAT3 const& textureCoordinate)
            : position(position),
            color(color),
            textureCoordinate(textureCoordinate)
        { }

        VertexPositionColorTexture2DArray(FXMVECTOR position, FXMVECTOR color, FXMVECTOR textureCoordinate)
        {
            XMStoreFloat3(&this->position, position);
            XMStoreFloat4(&this->color, color);
            XMStoreFloat3(&this->textureCoordinate, textureCoordinate);
        }

        XMFLOAT3 position;
        XMFLOAT4 color;
        XMFLOAT3 textureCoordinate;

        static const int InputElementCount = 3;
        static const D3D11_INPUT_ELEMENT_DESC InputElements[InputElementCount];
    };

    public enum class DrawType : int { None, Tiles, Quads };

    public ref class DefferedRenderer sealed
    {

    private:
        void InitStates();
        void InitShaders();
        void SetWrapMode(bool repeatU, bool repeatV, D3D11_FILTER filter);
        void DrawTilesBegin();
        void DrawQuadsBegin();
        void DrawTiles();
        void DrawQuads();

        enum
        {
            MAX_VERTICES = 21845,
            MAX_INDICES = MAX_VERTICES * 3
        };

        ComPtr<ID3D11Device> m_pDevice;
        ComPtr<ID3D11DeviceContext> m_pDefferedContext;

        TexturesManager^ m_rTexturesManager;
        TextureHandle^ m_rTextureHandle;

        XMFLOAT4 m_aColor[4];
        XMFLOAT3 m_aTexCoords[4];
        XMFLOAT3 m_aPosition[4];

        std::unique_ptr<DirectX::PrimitiveBatch<VertexPositionColorTexture2DArray>> m_pTilesBatch;
        std::unique_ptr<DirectX::PrimitiveBatch<VertexPositionColorTexture>> m_pQuadsBatch;

        uint16_t m_verticesNumber;
        VertexPositionColorTexture2DArray m_aTilesVertices[MAX_VERTICES];
        VertexPositionColorTexture m_aQuadsVertices[MAX_VERTICES];

        uint16_t m_indicesNumber;
        uint16_t m_aIndices[MAX_INDICES];

        ComPtr<ID3D11RenderTargetView> m_pRenderTargetView;

        ComPtr<ID3D11BlendState> m_pBlendState;
        ComPtr<ID3D11DepthStencilState> m_pDepthStencilState;
        ComPtr<ID3D11RasterizerState> m_pRasterizerState;
        ComPtr<ID3D11RasterizerState> m_pRasterizerStateScissor;
        ComPtr<ID3D11SamplerState> m_pSampleState;

        DrawType m_drawType;
        bool m_repeatU = false;
        bool m_repeatV = false;

        bool m_useClipping;
        D3D11_RECT m_clippingRect;

        ComPtr<ID3D11InputLayout> m_pTilesInputLayout;
        ComPtr<ID3D11InputLayout> m_pQuadsInputLayout;

        ComPtr<ID3D11VertexShader> m_pTilesVertexShader;
        ComPtr<ID3D11PixelShader> m_pTilesPixelShader;
        ComPtr<ID3D11VertexShader> m_pQuadsVertexShader;
        ComPtr<ID3D11PixelShader> m_pQuadsPixelShader;

        ComPtr<ID3D11CommandList> m_pCommandList;

        float m_viewportWidth;
        float m_viewportHeight;

    public:
        DefferedRenderer(ICanvasResourceCreator^ resourceCreator);

        void FillIndices();
        void AddVertices(uint16_t verticesCount);
        void FlushVertices();

        void SetView(float4x4 value);
        void SetClipping(bool isEnabled, float2 topLeft, float2 bottomRight);
        void SetPosition(float2 p0, float2 p1, float2 p2, float2 p3);

        void SetTexture(TextureHandle^ textureHandle);
        void SetTextureCoordinates(float2 t0, float2 t1, float2 t2, float2 t3, float index);

        void SetColor(float r, float g, float b, float a);
        void SetColor4(const Platform::Array<float4>^ colors);

        void DrawBegin(DrawType drawType);
        void Draw(float2 p0, float2 p1, float2 p2, float2 p3);
        void DrawEnd();

    internal:
        void SetRenderTarget(ID3D11RenderTargetView* renderTargetView);
        void SetTextureManager(TexturesManager^ manager);
        void SetViewportSize(float width, float height);

        ID3D11CommandList* GetCommandList()
        {
            return m_pCommandList.Get();
        }

        void Reset()
        {
            m_pCommandList.Reset();
            m_pRenderTargetView.Reset();
        }
    };

}