// Example low level rendering Unity plugin


#include "RenderPlugin.h"
#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>

#include <math.h>
#include <stdio.h>
#include <vector>
#include <string>
#include <vector>
#include <sstream>
#ifdef WIN32
#include <unordered_map>
using namespace std;
#else
#include <tr1/unordered_map>
using namespace std;
using namespace std::tr1;
#endif


typedef void(*FuncPtr)(const char *);
FuncPtr Debug;

void DebugLog(string log) {
	if (Debug) {
		Debug(log.c_str());
	}
}

// --------------------------------------------------------------------------
// UnitySetGraphicsDevice

static int g_DeviceType = -1;

// COM-like Release macro
#ifndef SAFE_RELEASE
#define SAFE_RELEASE(a) if (a) { a->Release(); a = NULL; }
#endif

typedef struct UPRenderer {
	HANDLE render_mutex;	
	void *last_frame;
	void *texture;
	int frame_height;
	int frame_width;
	int texture_width, texture_height;
} UPRenderer;

static unordered_map<int, UPRenderer *> g_renderer_by_id;
static int g_renderer_id;

static HANDLE create_mutex = CreateMutex(NULL, FALSE, NULL);

extern "C" int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API CreateRenderer() {
	WaitForSingleObject(create_mutex, INFINITE);	

	int renderer_id = ++g_renderer_id;

	UPRenderer *renderer = (UPRenderer *)calloc(sizeof(UPRenderer), 1);
	renderer->render_mutex = CreateMutex(NULL, FALSE, NULL);	

	// allocate slot
	g_renderer_by_id[renderer_id] = renderer;

	ReleaseMutex(create_mutex);
	
	return renderer_id;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetRendererFrame(int renderer_id, void *frame, int width, int height) {
	if (g_renderer_by_id.find(renderer_id) == g_renderer_by_id.end()) {
		return;
	}
	
	UPRenderer *renderer = g_renderer_by_id[renderer_id];
	WaitForSingleObject(renderer->render_mutex, INFINITE);
	renderer->last_frame = frame;
	renderer->frame_width = width;
	renderer->frame_height = height;

	ReleaseMutex(renderer->render_mutex);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API DestroyRenderer(int renderer_id) {
	WaitForSingleObject(create_mutex, INFINITE);	

	if (g_renderer_by_id.find(renderer_id) != g_renderer_by_id.end()) {
		UPRenderer *renderer = g_renderer_by_id[renderer_id];
		if (renderer) {
			CloseHandle(renderer->render_mutex);			

			g_renderer_by_id.erase(renderer_id);

			free(renderer);
		}
	}
	ReleaseMutex(create_mutex);	
}


extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetRendererTexture(int renderer_id, void *texturePtr, int width, int height)
{
	if (g_renderer_by_id.find(renderer_id) == g_renderer_by_id.end()) {
		return;
	}
	UPRenderer *renderer = g_renderer_by_id[renderer_id];

	WaitForSingleObject(renderer->render_mutex, INFINITE);
	renderer->texture = texturePtr;
	renderer->texture_width = width;
	renderer->texture_height = height;
	
	ReleaseMutex(renderer->render_mutex);
}

extern "C" int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ShouldCreateNewTexture(int renderer_id, int *width, int *height) {
	if (g_renderer_by_id.find(renderer_id) == g_renderer_by_id.end()) {
		return 0;
	}
	int ret = 0;

	UPRenderer *renderer = g_renderer_by_id[renderer_id];
	WaitForSingleObject(renderer->render_mutex, INFINITE);

	if (renderer->last_frame) {
		if (renderer->texture == NULL ||
			renderer->frame_width != renderer->texture_width ||
			renderer->frame_height != renderer->texture_height) {
			*width = renderer->frame_width;
			*height = renderer->frame_height;
			ret = 1;
		}
	}
	ReleaseMutex(renderer->render_mutex);	

	return ret;
}


extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetLogFunction(FuncPtr fp) {
	Debug = fp;
}


// UNITY plugin interface ---------------------------------------------------
// --------------------------------------------------------------------------
static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

static IUnityInterfaces* s_UnityInterfaces = NULL;
static IUnityGraphics* s_Graphics = NULL;

extern "C" void	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
	s_UnityInterfaces = unityInterfaces;
	s_Graphics = s_UnityInterfaces->Get<IUnityGraphics>();
	s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);

	// Run OnGraphicsDeviceEvent(initialize) manually on plugin load
	OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
	s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

static UnityGfxRenderer s_DeviceType = kUnityGfxRendererNull;
#if SUPPORT_D3D9
static IDirect3DDevice9* g_D3D9Device;
#endif
#if SUPPORT_D3D11
static ID3D11Device* g_D3D11Device;
#endif

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{	
	// Create graphics API implementation upon initialization
	if (eventType == kUnityGfxDeviceEventInitialize)
	{
		s_DeviceType = s_Graphics->GetRenderer();

#	if SUPPORT_D3D11
		if (s_DeviceType == kUnityGfxRendererD3D11)
		{
			IUnityGraphicsD3D11* d3d = s_UnityInterfaces->Get<IUnityGraphicsD3D11>();			
			g_D3D11Device = d3d->GetDevice();
			g_DeviceType = kUnityGfxRendererD3D11;
		}
#	endif // if SUPPORT_D3D11

#	if SUPPORT_D3D9
		if (s_DeviceType == kUnityGfxRendererD3D9)
		{			
			IUnityGraphicsD3D9* d3d = s_UnityInterfaces->Get<IUnityGraphicsD3D9>();
			g_D3D9Device = d3d->GetDevice();
			g_DeviceType = kUnityGfxRendererD3D9;
		}
#	endif // if SUPPORT_D3D11
	}
	//s_CurrentAPI->ProcessDeviceEvent(eventType, s_UnityInterfaces);
	// Cleanup graphics API implementation upon shutdown
	if (eventType == kUnityGfxDeviceEventShutdown)
	{		
		s_DeviceType = kUnityGfxRendererNull;
		g_DeviceType = -1;
	}
}

// --------------------------------------------------------------------------
// UnityRenderEvent
// This will be called for GL.IssuePluginEvent script calls; eventID will
// be the integer passed to IssuePluginEvent. In this example, we just ignore
// that value.
static void render(UPRenderer *renderer);

static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
	// Unknown / unsupported graphics device type? Do nothing
	if (s_DeviceType == -1) {
		DebugLog("Device Type -1");
		return;
	}

	if (g_renderer_by_id.find(eventID) == g_renderer_by_id.end()) {
		DebugLog("Renderer Not found!");
		return;
	}

	UPRenderer *renderer = g_renderer_by_id[eventID];
	WaitForSingleObject(renderer->render_mutex, INFINITE);

	if (renderer->last_frame && renderer->texture) {
		render(renderer);
	}
	ReleaseMutex(renderer->render_mutex);
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
{
	return OnRenderEvent;
}

// Actual Render Function

static void render(UPRenderer *renderer) {	
#if SUPPORT_D3D9
	// D3D9 case
	if (g_DeviceType == kUnityGfxRendererD3D9)
	{
		// Update native texture from code		
		IDirect3DTexture9* d3dtex = static_cast<IDirect3DTexture9*>(renderer->texture);
		D3DSURFACE_DESC desc;
		d3dtex->GetLevelDesc(0, &desc);
		D3DLOCKED_RECT lr;
		d3dtex->LockRect(0, &lr, NULL, 0);
		BYTE *p = (BYTE *)lr.pBits;
		const uint8_t *image_data = (uint8_t*)renderer->last_frame;
		int stride = renderer->frame_width * 4;
		for (UINT i = 0; i < desc.Height; ++i) {
			memcpy(p, image_data, stride);
			p += lr.Pitch;
			image_data += stride;
		}
		d3dtex->UnlockRect(0);
	}
#endif


#if SUPPORT_D3D11
	// D3D11 case
	if (g_DeviceType == kUnityGfxRendererD3D11)
	{		
		ID3D11DeviceContext *context;
		g_D3D11Device->GetImmediateContext(&context);
		if (context) {			
			const uint8_t *image_data = (uint8_t*)renderer->last_frame;			
			int bytes_per_pixel = 4;
			context->UpdateSubresource(static_cast<ID3D11Resource*>(renderer->texture),
				0, 0, image_data, renderer->frame_width * bytes_per_pixel, 0);
			context->Release();
		}
	}
#endif


#if SUPPORT_OPENGL
	// OpenGL case
	if (g_DeviceType == kUnityGfxRendererOpenGL)
	{
		glBindTexture(GL_TEXTURE_2D, (GLuint)(size_t)(renderer->texture));
		glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, renderer->frame_width, renderer->frame_height,
#if WIN32
			GL_EXT_bgra, 0x8367, //GL_UNSIGNED_INT_8_8_8_8_REV
#else
			GL_BGRA, GL_UNSIGNED_INT_8_8_8_8_REV,
#endif
			renderer->last_frame);
	}
#endif
}
