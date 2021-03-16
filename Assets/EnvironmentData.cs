using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[CreateAssetMenu]
public class EnvironmentData : ScriptableObject {
  [Header("General")]
  [Range(0, 1f)] public float difficulty = 0;
  [Range(30f, 500f)] public float size = 100f;

  [Header("Grid")]
  public GameObject prefab;
  public bool skipFirstInstance;
  public Vector3Int instances = Vector3Int.one;
  public Vector3 startPosition = Vector3.zero;
  public float margin = 5f;
}
