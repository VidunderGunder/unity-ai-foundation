using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnvironmentData : ScriptableObject {
  [Range(0, 1f)] public float difficulty = 0;
}
