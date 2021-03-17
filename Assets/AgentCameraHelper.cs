using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;

[ExecuteAlways]
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


  private void Start() {
    if (RGBCameraSensor == null || depthCameraSensor == null) {
      foreach (var cameraSensor in gameObject.GetComponents<CameraSensorComponent>()) {
        if (cameraSensor.SensorName.ToLower().Contains("rgb")) RGBCameraSensor = cameraSensor;
        if (cameraSensor.SensorName.ToLower().Contains("depth")) depthCameraSensor = cameraSensor;
      }
    }

    if (RGBCameraSensor == null) throw new System.Exception("No RGB Camera Sensor found - aborting.");
    if (depthCameraSensor == null) throw new System.Exception("No RGB Camera Sensor found - aborting.");

    SetResolutions();

    if (rgbResolution.x < 20) rgbResolution.x = 20;
    if (rgbResolution.y < 20) rgbResolution.y = 20;
    if (depthResolution.x < 20) depthResolution.x = 20;
    if (depthResolution.y < 20) depthResolution.y = 20;
  }

  private void SetResolutions() {
    // Cameras
    if (RGBDisplay != null) {
      RGBDisplay.x = rgbResolution.x;
      RGBDisplay.y = rgbResolution.y;
    }
    if (depthDisplay != null) {
      depthDisplay.x = rgbResolution.x;
      depthDisplay.y = rgbResolution.y;
    }

    // Camera sensors
    if (RGBCameraSensor != null) {
      RGBCameraSensor.Width = rgbResolution.x;
      RGBCameraSensor.Height = rgbResolution.y;
    }
    if (depthCameraSensor != null) {
      depthCameraSensor.Width = rgbResolution.x;
      depthCameraSensor.Height = rgbResolution.y;
    }

    // Render textures
    if (RGBRenderTexture != null) {
      RGBRenderTexture.width = rgbResolution.x;
      RGBRenderTexture.height = rgbResolution.y;
    }
    if (depthRenderTexture != null) {
      depthRenderTexture.width = rgbResolution.x;
      depthRenderTexture.height = rgbResolution.y;
    }
  }

}
