using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using OpenTok;

public class OpenTokRenderer: MonoBehaviour
{    
    private Texture2D texture;
    public int rendererId;

    public bool Enabled { get; set; }
    void Start()
    {        
        texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        GetComponent<MeshRenderer>().material.mainTexture = texture;
        rendererId = RenderPlugin.CreateRenderer();
        Debug.LogFormat("Creating renderer: {0}", rendererId);
    }

    void Update()
    {
        if (!Enabled) return;

        int newWidth = 0, newHeight = 0;
        if (RenderPlugin.ShouldCreateNewTexture(rendererId, ref newWidth, ref newHeight) != 0)
        {                      
            texture = new Texture2D(newWidth, newHeight, TextureFormat.BGRA32, false);
            RenderPlugin.SetRendererTexture(rendererId, texture.GetNativeTexturePtr(),
                                            newWidth, newHeight);
            GetComponent<MeshRenderer>().material.mainTexture = texture;
        }
        GL.IssuePluginEvent(RenderPlugin.GetRenderEventFunc(), rendererId);
    }   
}
