using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class RenderPlugin
{
    [DllImport("RenderPlugin", EntryPoint = "CreateRenderer", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern int CreateRenderer();

    [DllImport("RenderPlugin", EntryPoint = "SetRendererFrame", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SetRendererFrame(int rendererId, IntPtr frame, int width, int height);

    [DllImport("RenderPlugin", EntryPoint = "SetRendererTexture", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SetRendererTexture(int rendererId, IntPtr texture, int width, int height);

    [DllImport("RenderPlugin", EntryPoint = "ShouldCreateNewTexture", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern int ShouldCreateNewTexture(int rendererId, ref int width, ref int height);

    [DllImport("RenderPlugin", EntryPoint = "SetLogFunction", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetLogFunction(IntPtr fp);

    [DllImport("RenderPlugin", EntryPoint = "GetRenderEventFunc", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetRenderEventFunc();

    [DllImport("RenderPlugin", EntryPoint = "DestroyRenderer", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern void DestroyRenderer(int rendererId);
}