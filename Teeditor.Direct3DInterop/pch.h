
#pragma once

#define NOMINMAX
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <memory>
#include <vector>
#include <wrl.h>
#include <wrl/client.h>
#include <WindowsNumerics.h>
#include <algorithm>
#include <exception>
#include <cmath>
#include <collection.h>

// DirectX headers.
#include <d2d1_3.h>
#include <d2d1helper.h>
#include <DirectXMath.h>
#include <DirectXHelpers.h>
#include <d3d11_1.h>
#include <DirectXColors.h>
#include <Windows.Graphics.DirectX.Direct3D11.interop.h>

// DirectX Tool Kit headers.
#include <GeometricPrimitive.h>
#include <Effects.h>
#include "CommonStates.h"
#include "DDSTextureLoader.h"
#include "DirectXHelpers.h"
#include "GamePad.h"
#include "GraphicsMemory.h"
#include "Keyboard.h"
#include "Model.h"
#include "Mouse.h"
#include "PostProcess.h"
#include "PrimitiveBatch.h"
#include "ScreenGrab.h"
#include "SimpleMath.h"
#include "SpriteBatch.h"
#include "SpriteFont.h"
#include "VertexTypes.h"
#include "WICTextureLoader.h"

// Win2D headers.
#include <Microsoft.Graphics.Canvas.native.h>


#include <exception>

namespace DX
{
    inline void ThrowIfFailed(HRESULT hr)
    {
        if (FAILED(hr))
        {
            // Set a breakpoint on this line to catch DirectX API errors
            throw std::exception();
        }
    }
}