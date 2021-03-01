using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentSettings", menuName = "Environment Settings")]
public class EnvironmentSettings : ScriptableObject {
  public static List<GameObject> prefabEnvironments = new List<GameObject>();
  public static float maxDimension = 10f;

  public List<GameObject> prefabEnvironmentsHelper;
  public float maxDimensionHelper;

  [Range(0, 1f)] public float difficulty = 0;
  public EnvironmentType environmentType = EnvironmentType.Simple;

  public void OnAfterDeserialize() {
    prefabEnvironments = prefabEnvironmentsHelper;

    foreach (var prefabEnvironment in prefabEnvironments) {
      Vector3 size = prefabEnvironment.GetComponent<Renderer>().bounds.size;
      if (size.x > maxDimension) maxDimension = size.x;
      if (size.y > maxDimension) maxDimension = size.y;
      if (size.z > maxDimension) maxDimension = size.z;
    }
  }

  public void OnBeforeSerialize() {
    prefabEnvironmentsHelper = prefabEnvironments;
    maxDimensionHelper = maxDimension;
  }

  // private readonly GameObject environment;

  // public GameObject Environment { get { return environment; } }
  // public Vector3 Size { get { return environment.GetComponent<Renderer>().bounds.size; } }
  // public float Width { get { return Size.x; } }
  // public float Height { get { return Size.y; } }
  // public float Length { get { return Size.z; } }
}
