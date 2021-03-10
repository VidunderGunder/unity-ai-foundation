using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SpawnAreasData : ScriptableObject {
  [Header("Dependencies")]
  public RandomTerrainData terrainData;

  [Header("Spawn Areas")]
  public GameObject agent;
  public GameObject randomObjects;
}