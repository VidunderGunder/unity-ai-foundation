using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[CreateAssetMenu]
public class EnvironmentData : ScriptableObject {
  [Header("Settings")]
  [Range(0, 1f)] public float difficulty = 0;
  [Range(0, 500f)] public float size = 100f;

  [Header("Data")]
  public RandomTerrainData randomTerrain;
}
