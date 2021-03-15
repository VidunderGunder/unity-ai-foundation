// using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOptions {
  [Header("Transform")]
  public ScaleMethod scaleMethod = ScaleMethod.None;
  public float minScale = 1f;
  public float maxScale = 1f;

  public Vector3 rotationRange = 180f * Vector3.one;
  public bool isStatic = true;

  [Header("Spawn Areas")]
  public List<string> allowedSpawnAreas;
  public List<string> forbiddenSpawnAreas;
}

public enum ScaleMethod {
  None,
  Equal,
  Random,
}

[ExecuteAlways]
public class Spawner : MonoBehaviour {
  public EnvironmentData env;
  public List<Spawnable> objects;
  public ObjectPooler objectPooler;
  public List<GameObject> spawnAreas;

  [System.Serializable]
  public class Spawnable : SpawnOptions {
    public Transform transform;
    public Rigidbody rigidbody;
    public float originalMass;
  }

  public bool SpawnAreaTest() {
    bool passed = true;

    // TODO: Make test to enforce correct spawn areas
    // Maybe automate or make spawn areas safer instead?

    if (!passed) Debug.Log("Some warning (Spawner.SpawnAreaTest())");
    return passed;
  }

  private void Start() {
    if (!SpawnAreaTest()) return;
    SpawnAllPoolObjects();
  }

  // public void Spawn() {
  //   foreach (var pool in objectPooler.data.pools) {
  //     StartCoroutine(PeriodicObjectPoolSpawn(pool.poolName, 0.001f));
  //   }
  // }

  public void SpawnAllObjects() {
    foreach (var thing in objects) {
      // SpawnObject
    }
  }

  public void SpawnObject(Spawnable thing) {

  }

  public void SpawnAllPoolObjects() {
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
    RandomizePoolObject(obj, objectPooler.data.poolOptions[poolName]);

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

  private void RandomizeObject(Spawnable thing) {
    SetTransform(thing.transform, thing);

    if (!thing.isStatic) {
      Rigidbody rb = thing.transform.gameObject.GetComponent<Rigidbody>();
      // TODO: Move mass setup to initialization method and only run on start-up (or preferably on object added)
      if (thing == null) thing.originalMass = thing.rigidbody.mass;
      thing.rigidbody.mass = thing.originalMass * thing.transform.localScale.sqrMagnitude;
    }
  }

  private void RandomizePoolObject(GameObject obj, SpawnOptions options) {
    SetTransform(obj.transform, options);

    if (!options.isStatic) {
      Rigidbody rb = obj.GetComponent<Rigidbody>();
      // TODO: Make sure mass function is repeatable
      if (rb != null) rb.mass *= obj.transform.localScale.sqrMagnitude;
    }
  }

  private void SetTransform(Transform tf, SpawnOptions options) {
    tf.rotation.Randomize(options.rotationRange);
    tf.localScale = RandomScale(
      tf.localScale,
      options.minScale,
      options.maxScale,
      options.scaleMethod
    );

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
          tf.position = RandomPointWithinBounds(allowedBounds);
          var objectBounds = tf.gameObject.GetComponent<Renderer>().bounds;

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

        if (!success) tf.gameObject.SetActive(false);
      }
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
