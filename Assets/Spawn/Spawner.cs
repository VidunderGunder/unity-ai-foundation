// using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnArea {
  Agent = 0,
  Random = 1,
}

public enum ScaleMethod {
  None,
  Equal,
  Random,
}

[ExecuteAlways]
public class Spawner : MonoBehaviour {
  public ObjectPooler objectPooler;
  public List<GameObject> spawnAreas;

  public bool SpawnAreaTest() {
    bool passed = true;

    // TODO: Make test to enforce correct spawn areas
    // Maybe automate or make spawn areas safer instead?

    if (!passed) Debug.Log("Some warning (Spawner.SpawnAreaTest())");
    return passed;
  }

  private void Start() {
    if (!SpawnAreaTest()) return;
    SpawnAll();
  }

  public void Spawn() {
    foreach (var pool in objectPooler.data.pools) {
      StartCoroutine(PeriodicObjectPoolSpawn(pool.poolName, 0.001f));
    }
  }

  public void SpawnAll() {
    foreach (var pool in objectPooler.data.pools) {
      for (; ; ) {
        if (SpawnFromPool(pool.poolName) == null) break;
      }
    }
  }

  public GameObject SpawnFromPool(string poolName, Vector3? position = null, Quaternion? rotation = null) {

    if (objectPooler.data == null || objectPooler.data.poolQueues == null) return null;
    if (!objectPooler.data.poolQueues.ContainsKey(poolName)) {
      Debug.Log("Pool named \"" + poolName + "\" doesn't exist.");
      return null;
    }
    if (objectPooler.data.poolQueues[poolName].Count.Equals(0)) return null;

    GameObject obj = objectPooler.data.poolQueues[poolName].Dequeue();

    if (obj == null) return null;

    obj.SetActive(true);

    ResetObject(obj);
    Randomize(obj, objectPooler.data.poolOptions[poolName]);

    // data.poolDictionary[pool].Enqueue(objectToSpawn);

    return obj;
  }

  private GameObject GetSpawnAreaFromPool(SpawnArea areaEnumIndex) {
    GameObject area = null;

    foreach (var a in spawnAreas) {
      System.Enum.TryParse(a.name, out SpawnArea aEnumIndex);
      if (aEnumIndex == areaEnumIndex) {
        area = a;
        break;
      }
    }

    return area;
  }

  private GameObject GetSpawnAreaFromPool(string areaName) {
    bool areaNameExists = System.Enum.TryParse(areaName, out SpawnArea areaEnumIndex);
    if (areaNameExists) return GetSpawnAreaFromPool(areaEnumIndex);
    return null;
  }

  private void Randomize(GameObject obj, ObjectPoolerData.PoolOptions options) {
    obj.transform.rotation = Quaternion.Euler(
      Random.Range(-options.rotationRange.x, options.rotationRange.x),
      Random.Range(-options.rotationRange.y, options.rotationRange.y),
      Random.Range(-options.rotationRange.z, options.rotationRange.z)
    );

    switch (options.scaleMethod) {
      case ScaleMethod.None:
        break;
      case ScaleMethod.Equal:
        float scaleFactor = Random.Range(options.minScaleFactor, options.maxScaleFactor);
        obj.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        break;
      case ScaleMethod.Random:
        obj.transform.localScale = RandomVector(options.minScaleFactor, options.maxScaleFactor);
        break;
      default:
        break;
    }

    // TODO: Spawn in spawn areas
    // --------------------------
    if (options.allowedSpawnAreas.Count > 0) {
      Debug.Log("Setting position");
      Debug.Log("------------------------------");
      SpawnArea areaEnumIndex = options.allowedSpawnAreas[Random.Range(0, options.allowedSpawnAreas.Count)];
      GameObject area = GetSpawnAreaFromPool(areaEnumIndex);

      if (area == null) {
        Debug.Log("Spawn area " + areaEnumIndex + " is not available.");
      } else {
        var allowedBounds = area.GetComponent<Renderer>().bounds;
        Debug.Log("allowedBounds.center: " + allowedBounds.center.ToString());
        Debug.Log("allowedBounds.size: " + allowedBounds.size.ToString());
        int maxAttempts = 10;
        int attempts = 0;
        bool success = false;

        for (var attempt = 0; attempt < maxAttempts; attempt++) {
          attempts++;
          obj.transform.position = RandomPointWithinBounds(allowedBounds);

          if (options.forbiddenSpawnAreas.Count == 0) {
            Debug.Log("No forbidden areas");
            success = true;
            break;
          }

          foreach (var forbiddenAreaEnumIndex in options.forbiddenSpawnAreas) {
            GameObject forbiddenArea = GetSpawnAreaFromPool(forbiddenAreaEnumIndex);
            if (forbiddenArea == null) Debug.Log("Couldn't get Forbidden Area", obj);
            Bounds forbiddenBounds = forbiddenArea.GetComponent<Renderer>().bounds;

            // Success checks
            // --------------
            if (obj.GetComponent<Renderer>().bounds.Intersects(forbiddenBounds)) {
              Debug.Log("In forbidden area", obj);
            } else {
              Debug.Log("In allowed area", obj);
              success = true;
              break;
            }
            // --------------
          }
          if (success) break;
        }

        Debug.Log("Spawn " + (success ? "successful!" : "failed..."), obj);
        Debug.Log("Retries: " + attempts.ToString(), obj);
        Debug.Log("------------------------------");

        if (!success) obj.SetActive(false);
      }
    }
    // --------------------------

    if (!options.isStatic) {
      Rigidbody rb = obj.GetComponent<Rigidbody>();
      if (rb != null) rb.mass *= obj.transform.localScale.sqrMagnitude;
    }
  }

  void ResetObject(GameObject obj) {
    obj.transform.position = transform.position;
    obj.transform.rotation = transform.rotation;
    if (!obj.isStatic) {
      var rb = GetComponent<Rigidbody>();
      if (rb != null) {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
      }
    }
  }

  private Vector3 RandomVector(float min, float max) {
    return new Vector3(
      Random.Range(min, max),
      Random.Range(min, max),
      Random.Range(min, max)
    );
  }

  private Vector3 RandomPointWithinBounds(Bounds bounds) {
    return bounds.center + new Vector3(
      Random.Range(bounds.min.x, bounds.max.x),
      Random.Range(bounds.min.y, bounds.max.y),
      Random.Range(bounds.min.z, bounds.max.z)
    );
  }

  IEnumerator PeriodicObjectPoolSpawn(string pool, float period) {
    for (; ; ) {
      if (objectPooler == null || SpawnFromPool(pool) == null) yield break;
      yield return new WaitForSeconds(period);
    }
  }
}
