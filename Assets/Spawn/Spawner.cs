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
  public List<GameObject> allowedSpawnAreas;
  public List<GameObject> forbiddenSpawnAreas;
}

public enum ScaleMethod {
  None,
  Equal,
  Random,
}

// [ExecuteAlways]
public class Spawner : MonoBehaviour {
  public EnvironmentData env;
  public List<Spawnable> objects;
  public ObjectPooler objectPooler;
  public List<GameObject> spawnAreas;

  [System.Serializable]
  public class Spawnable : SpawnOptions {
    public Transform transform;
    public Rigidbody rigidbody;
    [System.NonSerialized]
    public float? originalMass = null;
  }

  private void Start() {
    Spawn();
  }

  public void Spawn() {
    SpawnAllObjects();
    SpawnAllPoolObjects();
  }

  // public void Spawn() {
  //   foreach (var pool in objectPooler.pools) {
  //     StartCoroutine(PeriodicObjectPoolSpawn(pool.poolName, 0.001f));
  //   }
  // }

  public void SpawnAllObjects() {
    foreach (var thing in objects) {
      SpawnObject(thing);
    }
  }

  public void SpawnObject(Spawnable thing) {
    ResetObject(thing.transform.gameObject);
    thing.transform.gameObject.SetActive(true);
    RandomizeObject(thing);
  }

  public void SpawnAllPoolObjects() {
    foreach (var pool in objectPooler.pools) {
      for (; ; ) {
        if (SpawnFromPool(pool.poolName) == null) break;
      }
    }
  }

  public GameObject SpawnFromPool(string poolName, Vector3? position = null, Quaternion? rotation = null) {

    if (objectPooler == null || objectPooler.poolQueues == null) return null;
    if (!objectPooler.poolQueues.ContainsKey(poolName)) {
      Debug.Log("Pool named \"" + poolName + "\" doesn't exist.");
      return null;
    }
    if (objectPooler.poolQueues[poolName].Count.Equals(0)) return null;

    GameObject obj = objectPooler.poolQueues[poolName].Dequeue();

    if (obj == null) return null;

    obj.SetActive(true);

    ResetObject(obj);
    RandomizePoolObject(obj, objectPooler.poolOptions[poolName]);

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

    // TODO: Move mass setup to initialization method and only run on start-up (or preferably on object added)
    if (!thing.isStatic && thing.scaleMethod != ScaleMethod.None && thing.rigidbody != null) {
      if (thing.originalMass == null) thing.originalMass = thing.rigidbody.mass;
      thing.rigidbody.mass =
        thing.originalMass != null
        ? (float)thing.originalMass * thing.transform.localScale.sqrMagnitude
        : thing.rigidbody.mass = 1f;
    }
  }

  private void RandomizePoolObject(GameObject obj, SpawnOptions options) {
    SetTransform(obj.transform, options);

    // TODO: Move mass setup to initialization method and only run on start-up (or preferably on object added)
    if (!options.isStatic) {
      Rigidbody rb = obj.GetComponent<Rigidbody>();
      if (rb != null) rb.mass *= obj.transform.localScale.sqrMagnitude;
    }
  }

  private Transform SetTransform(Transform tf, SpawnOptions options) {
    tf.rotation = tf.rotation.Randomize(options.rotationRange);
    tf.localScale = RandomScale(
      tf.localScale,
      options.minScale,
      options.maxScale,
      options.scaleMethod
    );

    if (options.allowedSpawnAreas.Count > 0) {
      GameObject area = options.allowedSpawnAreas[Random.Range(0, options.allowedSpawnAreas.Count)];

      if (area == null) {
        Debug.Log("Spawn area " + area.name + " is not available.");
      } else {
        var allowedBounds = area.GetComponent<Collider>().bounds;
        int maxAttempts = 10;
        int attempts = 0;
        bool success = false;

        for (var attempt = 0; attempt < maxAttempts; attempt++) {
          attempts++;
          tf.position = RandomPointWithinBounds(allowedBounds);
          var objectBounds = tf.gameObject.GetComponent<Collider>().bounds;

          if (options.forbiddenSpawnAreas.Count == 0) {
            success = true;
            break;
          }

          foreach (var forbiddenArea in options.forbiddenSpawnAreas) {
            if (forbiddenArea == null) Debug.Log("Couldn't get Forbidden Area" + forbiddenArea.name);
            Bounds forbiddenBounds = forbiddenArea.GetComponent<Collider>().bounds;

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

    return tf;
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
    obj.transform.localPosition = Vector3.zero;
    obj.transform.position = transform.position;
    obj.transform.localRotation = Quaternion.identity;
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
    return new Vector3(
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
