using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RandomTerrain : MonoBehaviour {
  public EnvironmentData env;
  public List<MeshGenerator> generators;

  public void Randomize() {
    foreach (MeshGenerator gen in generators) {
      gen.scale = env.size;
    }
  }

  private void OnValidate() {
    Randomize();
  }

  void Start() {
    Randomize();
  }
}
