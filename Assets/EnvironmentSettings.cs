using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentSettings", menuName = "Environment Settings")]
public class EnvironmentSettings : ScriptableObject {
  public float maxDimension = 10f;
  [Range(0, 1f)] public float difficulty = 0;
  public EnvironmentType environmentType = EnvironmentType.Simple;

  // private readonly GameObject environment;

  // public GameObject Environment { get { return environment; } }
  // public Vector3 Size { get { return environment.GetComponent<Renderer>().bounds.size; } }
  // public float Width { get { return Size.x; } }
  // public float Height { get { return Size.y; } }
  // public float Length { get { return Size.z; } }
}
