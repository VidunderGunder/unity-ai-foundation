using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {
  public delegate void ResetEnvironment();
  public static event ResetEnvironment OnResetEnvironment;

  private void OnGUI() {
    if (GUI.Button(new Rect(Screen.width / 2 - 50, 5, 100, 30), "Click")) {
      if (OnResetEnvironment != null) OnResetEnvironment();
    }
  }
}
