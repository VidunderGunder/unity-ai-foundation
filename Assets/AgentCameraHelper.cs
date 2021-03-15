using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;

public class AgentCameraHelper : MonoBehaviour {
  [Header("Dependencies")]
  public CameraSensorComponent RGBCameraSensor;
  public CameraSensorComponent depthCameraSensor;

  [Header("Options")]
  [Tooltip("Decides size of agent input. Also syncs with RenderTexture. Must be 20 or more.")]
  public Vector2Int rgbResolution = new Vector2Int(20, 20);
  [Tooltip("Decides size of agent input. Also syncs with RenderTexture. Must be 20 or more.")]
  public Vector2Int depthResolution = new Vector2Int(20, 20);

  [Header("UI or Debugging")]
  public RGBDisplay RGBDisplay;
  public RenderTexture RGBRenderTexture;
  public DepthDisplay depthDisplay;
  public RenderTexture depthRenderTexture;


  private void Awake() {
    if (RGBCameraSensor == null || depthCameraSensor == null) {
      foreach (var cameraSensor in gameObject.GetComponents<CameraSensorComponent>()) {
        if (cameraSensor.SensorName.ToLower().Contains("rgb")) RGBCameraSensor = cameraSensor;
        if (cameraSensor.SensorName.ToLower().Contains("depth")) depthCameraSensor = cameraSensor;
      }
    }

    SetResolutions();
  }

  private void SetResolutions() {
    // Cameras
    if (RGBDisplay != null) {
      if (RGBDisplay.x < 20) RGBDisplay.x = 20;
      RGBDisplay.x = rgbResolution.x;
      if (RGBDisplay.y < 20) RGBDisplay.y = 20;
      RGBDisplay.y = rgbResolution.y;
    }
    if (depthDisplay != null) {
      if (depthDisplay.x < 20) depthDisplay.x = 20;
      depthDisplay.x = rgbResolution.x;
      if (depthDisplay.y < 20) depthDisplay.y = 20;
      depthDisplay.y = rgbResolution.y;
    }

    // Camera sensors
    if (RGBCameraSensor != null) {
      if (RGBCameraSensor.Width < 20) RGBCameraSensor.Width = 20;
      RGBCameraSensor.Width = rgbResolution.x;
      if (RGBCameraSensor.Height < 20) RGBCameraSensor.Height = 20;
      RGBCameraSensor.Height = rgbResolution.y;
    }
    if (depthCameraSensor != null) {
      if (depthCameraSensor.Width < 20) depthCameraSensor.Width = 20;
      depthCameraSensor.Width = rgbResolution.x;
      if (depthCameraSensor.Height < 20) depthCameraSensor.Height = 20;
      depthCameraSensor.Height = rgbResolution.y;
    }

    // Render textures
    if (RGBRenderTexture != null) {
      if (RGBRenderTexture.width < 20) RGBRenderTexture.width = 20;
      RGBRenderTexture.width = rgbResolution.x;
      if (RGBRenderTexture.height < 20) RGBRenderTexture.height = 20;
      RGBRenderTexture.height = rgbResolution.y;
    }
    if (depthRenderTexture != null) {
      if (depthRenderTexture.width < 20) depthRenderTexture.width = 20;
      depthRenderTexture.width = rgbResolution.x;
      if (depthRenderTexture.height < 20) depthRenderTexture.height = 20;
      depthRenderTexture.height = rgbResolution.y;
    }
  }

}
