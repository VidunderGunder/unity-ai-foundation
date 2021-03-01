using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnvironmentType {
  Simple,
  Random,
  Prefab
};

public class EnvironmentSpawner : MonoBehaviour {
  public EnvironmentSettings settings;

  private GameObject environment;

  private void Awake() {
    if (settings == null) settings = new EnvironmentSettings();
    Spawn();
  }

  private void Spawn() {
    if (environment == null) environment = GetEnvironment();
    environment.transform.parent = transform;
  }

  public GameObject GetEnvironment() {
    switch (settings.environmentType) {
      case EnvironmentType.Simple:
        return GetSimpleEnvironment();
      case EnvironmentType.Random:
        return GetRandomEnvironment();
      case EnvironmentType.Prefab:
        return GetPrefabEnvironment();
      default:
        return GetSimpleEnvironment();
    }
  }

  public GameObject GetSimpleEnvironment() {
    GameObject simpleEnvironment = GameObject.CreatePrimitive(PrimitiveType.Plane);
    simpleEnvironment.transform.localScale = new Vector3(EnvironmentSettings.maxDimension, 1f, EnvironmentSettings.maxDimension);
    simpleEnvironment.name = "Simple Environment";
    return simpleEnvironment;
  }

  public GameObject GetRandomEnvironment() {
    // TODO
    return GetSimpleEnvironment();
  }

  public GameObject GetPrefabEnvironment() {
    if (EnvironmentSettings.prefabEnvironments.Count > 0) {
      GameObject prefabEnvironment =
        EnvironmentSettings.prefabEnvironments[
          Random.Range(
            0,
            EnvironmentSettings.prefabEnvironments.Count
          )
        ];

      prefabEnvironment.transform.localScale =
        new Vector3(
          EnvironmentSettings.maxDimension,
          1f,
          EnvironmentSettings.maxDimension
        );

      prefabEnvironment.name = "Simple Environment";

      return prefabEnvironment;
    }
    Debug.Log("No prefabs available. Spawning simple environment.");
    return GetSimpleEnvironment();
  }
}


