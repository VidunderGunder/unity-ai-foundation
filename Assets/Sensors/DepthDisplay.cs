using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DepthDisplay : MonoBehaviour {
  public Camera cam;
  public int x = 160;
  public int y = 90;
  public float aspectRatio = 16f / 9f;

  void Awake() {
    if (cam == null) cam = GetComponent<Camera>();
    cam.pixelRect = new Rect(0, 0, x, y);
    cam.aspect = aspectRatio;
  }
}
