using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ScaleSpawn : MonoBehaviour {
  public EnvironmentData env;
  public Vector3 scaleFactors = new Vector3(1f, 0.1f, 1f);
  public bool xLock = false;
  public bool yLock = true;
  public bool zLock = false;


  public void Scale() {
    transform.localScale = new Vector3(
      xLock ? scaleFactors.x : scaleFactors.x * env.size,
      yLock ? scaleFactors.y : scaleFactors.y * env.size,
      zLock ? scaleFactors.z : scaleFactors.z * env.size
    );
  }

  // TODO: Trigger on event - environment size change
  private void Update() {
    Scale();
  }
}
