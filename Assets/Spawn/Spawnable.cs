using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Spawnable : ScriptableObject {
  public GameObject prefab;
  public bool isStatic = true;
  public ScaleMethod scaleMethod = ScaleMethod.None;
  public Vector3 rotationRange = Vector3.zero;

  public enum ScaleMethod {
    None,
    Equal,
    Random,
  }
}
