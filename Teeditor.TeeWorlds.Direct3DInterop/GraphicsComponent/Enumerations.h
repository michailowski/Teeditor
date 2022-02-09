#include "pch.h"
#pragma once

namespace Teeditor_TeeWorlds_Direct3DInterop
{
    namespace Enumerations
    {
        using namespace Platform::Metadata;

        [Flags] public enum class TextureLoadType : unsigned int { None = 0, Texture = 1, Texture2DArray = 2 };
    }
}