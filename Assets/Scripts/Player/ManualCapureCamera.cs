using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualCapureCamera : ComponentEx
{
    [SerializeField]
    new Camera camera;

    int depthBufferBits = 16;
    Vector2Int textureSize;
    List<RenderTexture> renderTextures = new List<RenderTexture>();

    public int renderTextureCount => renderTextures.Count;

    public RenderTexture GetRenderTexture(int index) => renderTextures[index];

    protected override void Awake()
    {
        camera.enabled = false;
    }

    private RenderTexture CreateRT(Vector2Int newTextureSize, int depthBufferBits)
    {
        RenderTexture rt = new RenderTexture(newTextureSize.x, newTextureSize.y, depthBufferBits, RenderTextureFormat.ARGB32);
        if (rt.Create())
            return rt;
        return null;
    }

    private void Resize(Vector2Int newTextureSize)
    {
        if (this.textureSize == newTextureSize)
            return;

        this.textureSize = newTextureSize;
        int count = renderTextures.Count;

        foreach (RenderTexture renderTexture in renderTextures)
            renderTexture.Release();
        renderTextures.Clear();

        for(int i = 0; i < count; ++i)
            renderTextures.Add(CreateRT(newTextureSize, depthBufferBits));
    }

    public void ReserveRenderTextures(Vector2Int targetTextureSize, int count)
    {
        Resize(targetTextureSize);

        for (int i = renderTextures.Count; i < count; ++i)
            renderTextures.Add(CreateRT(targetTextureSize, depthBufferBits));
    }

    public void Capture(Vector2Int targetTextureSize, int count, Action<Camera, int> beginCaptureAction = null, Action<Camera, int> endCaptureAction = null)
    {
        ReserveRenderTextures(targetTextureSize, count);

        camera.enabled = true;
        for (int i = 0; i < count; ++i)
        {
            beginCaptureAction?.Invoke(camera, i);

            camera.targetTexture = renderTextures[i];
            camera.Render();

            endCaptureAction?.Invoke(camera, i);
        }
        camera.enabled = false;
    }
}
