using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ObjectPooler : MonoBehaviour {
  public ObjectPoolerData data;

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

    if (data.poolQueues == null) return;
    if (Application.IsPlaying(gameObject)) {
      foreach (var pool in data.poolQueues) {
        foreach (var obj in pool.Value) {
          Destroy(obj);
        }
      }
    } else {
      foreach (var pool in data.poolQueues) {
        foreach (var obj in pool.Value) {
          DestroyImmediate(obj);
        }
      }
    }
    data.poolQueues = null;
  }

  public Dictionary<string, Queue<GameObject>> InitializePools() {
    data.poolQueues = new Dictionary<string, Queue<GameObject>>();
    data.poolOptions = new Dictionary<string, ObjectPoolerData.PoolOptions>();

    foreach (var pool in data.pools) {
      if (pool.prefabs.Count.Equals(0)) break;
      Queue<GameObject> objectPool = new Queue<GameObject>();

      for (int i = 0; i < pool.amount; i++) {
        GameObject obj = Instantiate(pool.prefabs[Random.Range(0, pool.prefabs.Count)]);
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        obj.SetActive(false);
        obj.transform.parent = transform;
        obj.isStatic = pool.isStatic;
        if (obj.isStatic) {
          if (rb != null) rb.isKinematic = true;
        } else {
          if (rb != null) rb.mass *= obj.transform.localScale.sqrMagnitude;
        }
        objectPool.Enqueue(obj);
      }

      data.poolQueues.Add(pool.poolName, objectPool);
      ObjectPoolerData.PoolOptions options = (ObjectPoolerData.PoolOptions)pool;
      data.poolOptions.Add(pool.poolName, options);
    }

    return data.poolQueues;
  }
}
