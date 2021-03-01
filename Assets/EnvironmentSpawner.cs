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
    if (environment == null) environment = GetEnvironment();
  }

  private void Start() {
    environment.transform.parent = transform; // Doesn't work :(
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
    simpleEnvironment.transform.localScale = new Vector3(settings.maxDimension, 1f, settings.maxDimension);
    return simpleEnvironment;
  }

  public GameObject GetRandomEnvironment() {
    // TODO
    return GetSimpleEnvironment();
  }

  public GameObject GetPrefabEnvironment() {
    // TODO
    return GetSimpleEnvironment();
  }
}


