Unity Native Rendering Plugin
===================================

Unity supports plugins built in C or C++ that can be called from the Unity project using a C# bridge. In this plugin, we will natively render the video stream into DirectX textures using native code instead of managed code.

How to Build
-----------------

Open `RenderPlugin.sln` file, select `x64` and `Release` configuration and build the project. When built it should generate a RenderPlugin.dll file in `x64/Release` folder. You will need to copy that Dll to the Unity Project folder.

Exploring the Code
---------------------

The main part of the plugin lives in [RenderPlugin.cpp](RenderPlugin.cpp) file. As you can read in the [main README file](../../README.md), the plugin will receive frames from the Unity Project side, and it's main task is to draw them in the DirectX texture set as render target.

First, we need to create the instance of the renderer that will draw a particular stream. This is done by calling `int CreateRenderer()`. This method will return the identifier that you need to use in further calls. Each video stream of the Unity Project side needs to call this method and save the identifier. In our sample, this is called twice, once by the publisher and another by the subscriber.

Before feeding with frames, the Unity Project will first create a `Texture2D` object and will send the native handle to the plugin by calling: `void SetRendererTexture(int renderer_id, void *texturePtr, int width, int height)`.

Once the texture handler is set, the frames are fed up by the function `void SetRendererFrame(int renderer_id, void *frame, int width, int height);` This function will save just the last frame, so if a frame comes before the previous one is renderer, it will be discarded.

The rendering of the frames happens in a different function. The plugin works following the [Hollywood principle](https://en.wikipedia.org/wiki/Inversion_of_control), that means that the texture drawing times are controlled by the Unity Project. The other part of the equation will request the plugin to update the textures by calling `static void OnRenderEvent(int eventID)` function. There, the plugin will use DirectX API to draw the previously set frame.

References
--------------

For more information about Unity Native rendering plugins, please see [official reference](https://docs.unity3d.com/Manual/NativePlugins.html)