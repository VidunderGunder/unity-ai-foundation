using System;
using Unity.MLAgents.Sensors;
using UnityEngine;

[ExecuteAlways]
public class AgentCameraHelper : MonoBehaviour
{
    public CameraSensorComponent depthCameraSensor;
    public DepthDisplay depthDisplay;
    public RenderTexture depthRenderTexture;

    [Tooltip("Decides size of agent input. Also syncs with RenderTexture. Must be 20 or more.")]
    public Vector2Int depthResolution = new Vector2Int(20, 20);

    [Header("Dependencies")] public CameraSensorComponent RGBCameraSensor;

    [Header("UI or Debugging")] public RGBDisplay RGBDisplay;

    public RenderTexture RGBRenderTexture;

    [Header("Options")] [Tooltip("Decides size of agent input. Also syncs with RenderTexture. Must be 20 or more.")]
    public Vector2Int rgbResolution = new Vector2Int(20, 20);


    private void Start()
    {
        if (RGBCameraSensor == null) throw new Exception("No RGB Camera Sensor found - aborting.");
        if (depthCameraSensor == null) throw new Exception("No RGB Camera Sensor found - aborting.");

        EnsureMinimumPixels();
        SetResolutions();
    }

    private void EnsureMinimumPixels(int pixels = 20)
    {
        if (rgbResolution.x < pixels) rgbResolution.x = pixels;
        if (rgbResolution.y < pixels) rgbResolution.y = pixels;
        if (depthResolution.x < pixels) depthResolution.x = pixels;
        if (depthResolution.y < pixels) depthResolution.y = pixels;
    }

    private void SetResolutions()
    {
        // Cameras
        if (RGBDisplay != null)
        {
            if (RGBDisplay.x != rgbResolution.x) RGBDisplay.x = rgbResolution.x;
            if (RGBDisplay.y != rgbResolution.y) RGBDisplay.y = rgbResolution.y;
        }

        if (depthDisplay != null)
        {
            if (depthDisplay.x != rgbResolution.x) depthDisplay.x = rgbResolution.x;
            if (depthDisplay.y != rgbResolution.y) depthDisplay.y = rgbResolution.y;
        }

        // Camera sensors
        if (RGBCameraSensor != null)
        {
            if (RGBCameraSensor.Width != rgbResolution.x) RGBCameraSensor.Width = rgbResolution.x;
            if (RGBCameraSensor.Height != rgbResolution.y) RGBCameraSensor.Height = rgbResolution.y;
        }

        if (depthCameraSensor != null)
        {
            if (depthCameraSensor.Width != rgbResolution.x) depthCameraSensor.Width = rgbResolution.x;
            if (depthCameraSensor.Height != rgbResolution.y) depthCameraSensor.Height = rgbResolution.y;
        }

        // Render textures
        if (RGBRenderTexture != null)
        {
            if (RGBRenderTexture.width != rgbResolution.x) RGBRenderTexture.width = rgbResolution.x;
            if (RGBRenderTexture.height != rgbResolution.y) RGBRenderTexture.height = rgbResolution.y;
        }

        if (depthRenderTexture != null)
        {
            if (depthRenderTexture.width != rgbResolution.x) depthRenderTexture.width = rgbResolution.x;
            if (depthRenderTexture.height != rgbResolution.y) depthRenderTexture.height = rgbResolution.y;
        }
    }
}