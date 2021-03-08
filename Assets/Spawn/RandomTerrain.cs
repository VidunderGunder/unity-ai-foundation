using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RandomTerrain : MonoBehaviour {
  public RandomTerrainData data;
  public List<MeshGenerator> meshGenerators;

  public void Randomize() {
    foreach (MeshGenerator generator in meshGenerators) {
      generator.scale = data.scale;
    }
  }

  private void OnValidate() {
    Randomize();
  }

  void Start() {
    Randomize();
  }
}
