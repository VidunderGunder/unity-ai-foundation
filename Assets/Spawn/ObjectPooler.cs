using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ObjectPooler : MonoBehaviour {
  public ObjectPoolerData data;

  // Makeshift singleton solution (TODO: Make not static)
  public static ObjectPooler Instance;

  void Awake() {
    // Makeshift singleton solution (TODO: Make not static)
    Instance = this;
  }

  private void OnEnable() {
    Cleanup();
    InitializePools();
  }

  public void Cleanup() {
    if (Application.IsPlaying(gameObject)) {
      foreach (Transform child in transform) {
        Destroy(child.gameObject);
      }
    } else {
      foreach (Transform child in transform) {
        DestroyImmediate(child.gameObject);
      }
    }

    if (data.poolDictionary == null) {
      return;
    }

    if (Application.IsPlaying(gameObject)) {
      foreach (var pool in data.poolDictionary) {
        foreach (var obj in pool.Value) {
          Destroy(obj);
        }
      }
    } else {
      foreach (var pool in data.poolDictionary) {
        foreach (var obj in pool.Value) {
          DestroyImmediate(obj);
        }
      }
    }

    data.poolDictionary = null;
  }

  public Dictionary<string, Queue<GameObject>> InitializePools() {
    data.poolDictionary = new Dictionary<string, Queue<GameObject>>();

    foreach (var pool in data.pools) {
      if (pool.spawnables.Count.Equals(0)) break;
      Queue<GameObject> objectPool = new Queue<GameObject>();

      for (int i = 0; i < pool.amount; i++) {
        ObjectPoolerData.Spawnable spawnable = pool.spawnables[Random.Range(0, pool.spawnables.Count)];
        GameObject obj = Instantiate(spawnable.prefab);
        obj.SetActive(false);
        obj.transform.parent = transform;

        // Randomize (TODO: Move outside InitializePools, make callable and add reset)
        // ---------------------------------------------------------------------------
        obj.transform.position = transform.position;
        obj.transform.rotation = transform.rotation;
        obj.transform.rotation = Quaternion.Euler(
          Random.Range(spawnable.minScaleFactor, spawnable.maxScaleFactor),
          Random.Range(spawnable.minScaleFactor, spawnable.maxScaleFactor),
          Random.Range(spawnable.minScaleFactor, spawnable.maxScaleFactor)
        );
        switch (spawnable.scaleMethod) {
          case ObjectPoolerData.Spawnable.ScaleMethod.None:
            break;
          case ObjectPoolerData.Spawnable.ScaleMethod.Equal:
            float scaleFactor = Random.Range(spawnable.minScaleFactor, spawnable.maxScaleFactor);
            obj.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            break;
          case ObjectPoolerData.Spawnable.ScaleMethod.Random:
            obj.transform.localScale = RandomVector(spawnable.minScaleFactor, spawnable.maxScaleFactor);
            break;
          default:
            break;
        }
        obj.isStatic = spawnable.isStatic;
        if (!obj.isStatic) obj.GetComponent<Rigidbody>().mass *= obj.transform.localScale.sqrMagnitude;
        // ---------------------------------------------------------------------------

        objectPool.Enqueue(obj);
      }

      data.poolDictionary.Add(pool.poolName, objectPool);
    }

    return data.poolDictionary;
  }

  public GameObject SpawnFromPool(string pool, Vector3? position = null, Quaternion? rotation = null) {
    if (data == null || data.poolDictionary == null) {
      return null;
    }

    if (!data.poolDictionary.ContainsKey(pool)) {
      Debug.Log("Pool named \"" + pool + "\" doesn't exist.");
      return null;
    }

    if (data.poolDictionary[pool].Count.Equals(0)) {
      // Debug.Log("Pool named \"" + pool + "\" is empty.");
      return null;
    }

    GameObject objectToSpawn = data.poolDictionary[pool].Dequeue();
    objectToSpawn.SetActive(true);

    // Reset position, rotation and speed
    if (position != null) objectToSpawn.transform.position = (Vector3)position;
    if (rotation != null) objectToSpawn.transform.rotation = (Quaternion)rotation;
    if (!objectToSpawn.isStatic) {
      var rb = GetComponent<Rigidbody>();
      if (rb != null) {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
      }
    }

    // data.poolDictionary[pool].Enqueue(objectToSpawn);

    return objectToSpawn;
  }

  private Vector3 RandomVector(float min, float max) {
    return new Vector3(
      Random.Range(min, max),
      Random.Range(min, max),
      Random.Range(min, max)
  );
  }
}
