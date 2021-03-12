using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleSpawn : MonoBehaviour {
  public EnvironmentData env;

  public void Scale() {
    transform.localScale = Vector3.one * env.size;
  }
}
