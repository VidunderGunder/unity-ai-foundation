using System;
using Unity.MLAgents.Sensors;
using UnityEngine;

[ExecuteAlways]
public class AgentCameraHelper : MonoBehaviour
{
    [Header("RGB")]
    public CameraSensorComponent rgbCameraSensor;
    public RGBDisplay rgbDisplay;
    public RenderTexture rgbRenderTexture;

    [Tooltip("Decides size of agent input. Also syncs with RenderTexture. Must be 20 or more.")]
    public Vector2Int rgbResolution = new Vector2Int(20, 20);

    [Header("Depth")]
    public CameraSensorComponent depthCameraSensor;
    public DepthDisplay depthDisplay;
    public RenderTexture depthRenderTexture;

    [Tooltip("Decides size of agent input. Also syncs with RenderTexture. Must be 20 or more.")]
    public Vector2Int depthResolution = new Vector2Int(20, 20);

    private void Start()
    {
        EnsureMinimumPixels();
        SyncComponents();
    }

    private void EnsureMinimumPixels(int pixels = 5)
    {
        if (rgbResolution.x < pixels) rgbResolution.x = pixels;
        if (rgbResolution.y < pixels) rgbResolution.y = pixels;
        if (depthResolution.x < pixels) depthResolution.x = pixels;
        if (depthResolution.y < pixels) depthResolution.y = pixels;
    }

    private void SyncComponents()
    {
        // RGB
        if (rgbDisplay != null)
        {
            rgbDisplay.gameObject.SetActive(rgbCameraSensor != null);
            if (rgbDisplay.x != rgbResolution.x) rgbDisplay.x = rgbResolution.x;
            if (rgbDisplay.y != rgbResolution.y) rgbDisplay.y = rgbResolution.y;
        }
        if (rgbCameraSensor != null)
        {
            if (rgbCameraSensor.Width != rgbResolution.x) rgbCameraSensor.Width = rgbResolution.x;
            if (rgbCameraSensor.Height != rgbResolution.y) rgbCameraSensor.Height = rgbResolution.y;
        }
        if (rgbRenderTexture != null)
        {
            if (rgbRenderTexture.width != rgbResolution.x) rgbRenderTexture.width = rgbResolution.x;
            if (rgbRenderTexture.height != rgbResolution.y) rgbRenderTexture.height = rgbResolution.y;
        }

        // Depth
        if (depthDisplay != null)
        {
            depthDisplay.gameObject.SetActive(depthCameraSensor != null);
            if (depthDisplay.x != depthResolution.x) depthDisplay.x = depthResolution.x;
            if (depthDisplay.y != depthResolution.y) depthDisplay.y = depthResolution.y;
        }
        if (depthCameraSensor != null)
        {
            if (depthCameraSensor.Width != depthResolution.x) depthCameraSensor.Width = depthResolution.x;
            if (depthCameraSensor.Height != depthResolution.y) depthCameraSensor.Height = depthResolution.y;
        }
        if (depthRenderTexture != null)
        {
            if (depthRenderTexture.width != depthResolution.x) depthRenderTexture.width = depthResolution.x;
            if (depthRenderTexture.height != depthResolution.y) depthRenderTexture.height = depthResolution.y;
        }
    }
}