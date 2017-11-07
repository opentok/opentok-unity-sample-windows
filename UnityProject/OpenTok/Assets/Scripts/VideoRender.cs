using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenTok;
using System;
using System.Runtime.InteropServices;

public class VideoRender : IVideoRenderer
{
    int rendererId;
    IntPtr[] buffer = { IntPtr.Zero };
    int[] strides = { 0 };
    int w, h;
    
    public VideoRender(int rID)
    {
        rendererId = rID;
        GCHandle.Alloc(buffer, GCHandleType.Pinned);
        GCHandle.Alloc(strides, GCHandleType.Pinned);     
    }    

    public void RenderFrame(VideoFrame frame)
    {
        if (w != frame.Width || h != frame.Height)
        {
            Debug.LogFormat("Size changed!, ({0},{1}) => ({2},{3})", w, h, frame.Width, frame.Height);
            w = frame.Width; h = frame.Height;
            if (buffer[0] != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer[0]);
            }            
            buffer[0] = Marshal.AllocHGlobal(frame.Width * frame.Height * 4);
            strides[0] = frame.Width * 4;            
        }
        frame.ConvertInPlace(PixelFormat.FormatArgb32, buffer, strides);        
        RenderPlugin.SetRendererFrame(rendererId, buffer[0], frame.Width, frame.Height);        
        frame.Dispose();
    }

    public void Dispose()
    {        
        RenderPlugin.DestroyRenderer(rendererId);
        if (buffer[0] != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(buffer[0]);
        }
    }
}