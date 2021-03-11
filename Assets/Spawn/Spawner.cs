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

  private GameObject GetSpawnAreaFromPool(string areaName) {
    GameObject area = null;

    foreach (var a in spawnAreas) {
      if (a.name == areaName) {
        area = a;
        break;
      }
    }

    return area;
  }

  private void Randomize(GameObject obj, ObjectPoolerData.PoolOptions options) {
    obj.transform.rotation.Randomize(options.rotationRange);
    obj.transform.localScale = RandomScale(
      obj.transform.localScale,
      options.minScale,
      options.maxScale,
      options.scaleMethod
    );

    // TODO: Spawn in spawn areas
    // --------------------------
    if (options.allowedSpawnAreas.Count > 0) {
      string areaName = options.allowedSpawnAreas[Random.Range(0, options.allowedSpawnAreas.Count)];
      GameObject area = GetSpawnAreaFromPool(areaName);

      if (area == null) {
        Debug.Log("Spawn area " + areaName + " is not available.");
      } else {
        var allowedBounds = area.GetComponent<Renderer>().bounds;
        int maxAttempts = 10;
        int attempts = 0;
        bool success = false;

        for (var attempt = 0; attempt < maxAttempts; attempt++) {
          attempts++;
          obj.transform.position = RandomPointWithinBounds(allowedBounds);
          var objectBounds = obj.GetComponent<Renderer>().bounds;

          if (options.forbiddenSpawnAreas.Count == 0) {
            success = true;
            break;
          }

          foreach (var forbiddenAreaName in options.forbiddenSpawnAreas) {
            GameObject forbiddenArea = GetSpawnAreaFromPool(forbiddenAreaName);
            if (forbiddenArea == null) Debug.Log("Couldn't get Forbidden Area" + forbiddenAreaName);
            Bounds forbiddenBounds = forbiddenArea.GetComponent<Renderer>().bounds;

            if (objectBounds.Intersects(forbiddenBounds)) {
              // Resume
            } else {
              success = true;
              break;
            }
          }
          if (success) break;
        }

        if (!success) obj.SetActive(false);
      }
    }
    // --------------------------

    if (!options.isStatic) {
      Rigidbody rb = obj.GetComponent<Rigidbody>();
      // TODO: Make sure mass function is repeatable
      if (rb != null) rb.mass *= obj.transform.localScale.sqrMagnitude;
    }
  }

  // New functions
  // ----------------------------------------------------------------------------
  public Vector3 RandomScale(Vector3 scale, float min, float max, ScaleMethod method = ScaleMethod.None) {
    switch (method) {
      case ScaleMethod.None:
        return scale;
      case ScaleMethod.Equal:
        float equalScale = Random.Range(min, max);
        return new Vector3(equalScale, equalScale, equalScale);
      case ScaleMethod.Random:
        return RandomVector(min, max);
      default:
        return scale;
    }
  }

  // // TODO: Spawn in spawn areas
  // // --------------------------
  // if (options.allowedSpawnAreas.Count > 0) {
  //   string areaName = options.allowedSpawnAreas[Random.Range(0, options.allowedSpawnAreas.Count)];
  //   GameObject area = GetSpawnAreaFromPool(areaName);

  //   if (area == null) {
  //     Debug.Log("Spawn area " + areaName + " is not available.");
  //   } else {
  //     var allowedBounds = area.GetComponent<Renderer>().bounds;
  //     int maxAttempts = 10;
  //     int attempts = 0;
  //     bool success = false;

  //     for (var attempt = 0; attempt < maxAttempts; attempt++) {
  //       attempts++;
  //       obj.transform.position = RandomPointWithinBounds(allowedBounds);
  //       var objectBounds = obj.GetComponent<Renderer>().bounds;

  //       if (options.forbiddenSpawnAreas.Count == 0) {
  //         success = true;
  //         break;
  //       }

  //       foreach (var forbiddenAreaName in options.forbiddenSpawnAreas) {
  //         GameObject forbiddenArea = GetSpawnAreaFromPool(forbiddenAreaName);
  //         if (forbiddenArea == null) Debug.Log("Couldn't get Forbidden Area" + forbiddenAreaName);
  //         Bounds forbiddenBounds = forbiddenArea.GetComponent<Renderer>().bounds;

  //         if (objectBounds.Intersects(forbiddenBounds)) {
  //           // Resume
  //         } else {
  //           success = true;
  //           break;
  //         }
  //       }
  //       if (success) break;
  //     }

  //     if (!success) obj.SetActive(false);
  //   }
  // }
  // // --------------------------

  // if (!options.isStatic) {
  //   Rigidbody rb = obj.GetComponent<Rigidbody>();
  //   if (rb != null) rb.mass *= obj.transform.localScale.sqrMagnitude;
  // }
  // ----------------------------------------------------------------------------

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
