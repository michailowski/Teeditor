#pragma once

namespace Teeditor_TeeWorlds_Direct3DInterop
{
    using namespace Platform;
    using namespace Platform::Collections;
    using namespace ::DirectX;
    using namespace Microsoft::Graphics::Canvas;
    using namespace Microsoft::WRL;
    using namespace Windows::Foundation::Numerics;
    using namespace Windows::Foundation::Collections;
    using namespace Windows::Graphics::DirectX::Direct3D11;

    public ref class GraphicsComponent sealed
    {
    public:
        GraphicsComponent(ICanvasResourceCreator^ resourceCreator, float viewportWidth, float viewportHeight);

        void SetTextureManager(TexturesManager^ texturesManager);
        void SetViewportSize(float width, float height);
        void SetRenderTarget(CanvasDrawingSession^ drawingSession);

        void ExecuteCommandLists();

        DefferedRenderer^ GetDefferedRenderer();

    private:
        enum
        {
            MAX_DEFFERED_RENDERER = 100
        };

        ICanvasResourceCreator^ m_rResourceCreator;

        ComPtr<ID3D11Device> m_pDevice;
        ComPtr<ID3D11DeviceContext> m_pImmediateContext;

        DefferedRenderer^ m_aDefferedRenderer[MAX_DEFFERED_RENDERER];
        
        UINT m_currentRendererCount;
    };
}
