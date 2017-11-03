
#define RENDERPLUGIN_EXPORTS extern "C" __declspec(dllexport)

#include "PlatformBase.h"

// --------------------------------------------------------------------------
// Include headers for the graphics APIs we support

#if SUPPORT_D3D9
#include <d3d9.h>
#endif
#if SUPPORT_D3D11
#include <d3d11.h>
#endif
#if SUPPORT_OPENGL
#if UNITY_WIN || UNITY_LINUX
#include <GL/gl.h>
#else
#include <OpenGL/OpenGL.h>
#include <OpenGL/gl.h>
#endif
#endif


#include "Unity\IUnityGraphics.h"
#include "Unity\IUnityGraphicsD3D9.h"
#include "Unity\IUnityGraphicsD3D11.h"