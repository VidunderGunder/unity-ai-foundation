using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Make changes show immediately when editing

[ExecuteAlways]
[CreateAssetMenu]
public class RandomTerrainData : ScriptableObject {
  [Range(0, 500f)] public float scale = 100f;
}
