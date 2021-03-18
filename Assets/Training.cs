using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Training : MonoBehaviour {
  // Force game focus when playing to not disable Agent's Input Actuator
  private void OnApplicationFocus(bool focusStatus) {
    if (EditorApplication.isPlaying) EditorApplication.ExecuteMenuItem("Window/General/Game");
  }
}
