#include "pch.h"
#include <GraphicsComponent/TexturesManager.h>
#include "GraphicsComponent/DefferedRenderer.h"
#include "GraphicsComponent/GraphicsComponent.h"
#include <wincodec.h>

using namespace Teeditor_TeeWorlds_Direct3DInterop;

GraphicsComponent::GraphicsComponent(ICanvasResourceCreator^ resourceCreator, float viewportWidth, float viewportHeight)
{
    m_rResourceCreator = resourceCreator;

    // Look up the Direct3D device and context corresponding to the specified Win2D resource creator.
    __abi_ThrowIfFailed(GetDXGIInterface(resourceCreator->Device, m_pDevice.GetAddressOf()));

    m_pDevice->GetImmediateContext(&m_pImmediateContext);

    m_currentRendererCount = 0;

    for (int i = 0; i < MAX_DEFFERED_RENDERER; i++)
    {
        m_aDefferedRenderer[i] = ref new DefferedRenderer(resourceCreator);
    }

    SetViewportSize(viewportWidth, viewportHeight);
}

void GraphicsComponent::SetTextureManager(TexturesManager^ texturesManager)
{
    for (int i = 0; i < MAX_DEFFERED_RENDERER; i++)
    {
        m_aDefferedRenderer[i]->SetTextureManager(texturesManager);
    }
}

void GraphicsComponent::SetViewportSize(float width, float height)
{
    for (int i = 0; i < MAX_DEFFERED_RENDERER; i++)
    {
        m_aDefferedRenderer[i]->SetViewportSize(width, height);
    }
}

void GraphicsComponent::SetRenderTarget(CanvasDrawingSession^ drawingSession)
{
    // Look up the Direct2D device context corresponding to the specified CanvasDrawingSession.
    ComPtr<ID2D1DeviceContext> d2dContext = GetWrappedResource<ID2D1DeviceContext>(drawingSession);

    // Flush any in-progress Direct2D rendering.
    d2dContext->Flush();

    // Tell Direct3D to render into the same surface as Direct2D.
    // Look up what target image this Direct2D device context is drawing onto.
    ComPtr<ID2D1Image> d2dTarget;
    d2dContext->GetTarget(&d2dTarget);

    // The target must be a bitmap.
    ComPtr<ID2D1Bitmap1> d2dBitmap;
    __abi_ThrowIfFailed(d2dTarget.As(&d2dBitmap));

    // Direct2D bitmap -> DXGI surface.
    ComPtr<IDXGISurface> dxgiSurface;
    __abi_ThrowIfFailed(d2dBitmap->GetSurface(&dxgiSurface));

    // DXGI surface -> Direct3D resource.
    ComPtr<ID3D11Resource> d3dResource;
    __abi_ThrowIfFailed(dxgiSurface.As(&d3dResource));

    // Create a rendertarget view.
    ComPtr<ID3D11RenderTargetView> renderTargetView;
    __abi_ThrowIfFailed(m_pDevice->CreateRenderTargetView(d3dResource.Get(), nullptr, renderTargetView.GetAddressOf()));

    //Set the rendertarget and depth buffer.
    for (UINT i = 0; i < MAX_DEFFERED_RENDERER; i++)
    {
        m_aDefferedRenderer[i]->SetRenderTarget(renderTargetView.Get());
    }

    renderTargetView.Reset();
}

DefferedRenderer^ GraphicsComponent::GetDefferedRenderer()
{
    if (m_currentRendererCount >= MAX_DEFFERED_RENDERER)
        return nullptr;

    UINT freeRendererId = m_currentRendererCount;
    m_currentRendererCount++;

    return m_aDefferedRenderer[freeRendererId];
}

void GraphicsComponent::ExecuteCommandLists()
{
    for (UINT i = 0; i < m_currentRendererCount; i++)
    {
        auto commandList = m_aDefferedRenderer[i]->GetCommandList();

        if (!commandList)
            continue;
        
        m_pImmediateContext->ExecuteCommandList(commandList, false);
    }

    for (UINT i = 0; i < MAX_DEFFERED_RENDERER; i++)
    {
        m_aDefferedRenderer[i]->Reset();
    }

    m_currentRendererCount = 0;
}
