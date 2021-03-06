using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour {
  public ObjectPoolerData data;

  #region Singleton
  public static ObjectPooler Instance;
  void Awake() {
    Instance = this;
  }
  #endregion

  void Start() {
    InitializePools();
  }

  public Dictionary<string, Queue<GameObject>> InitializePools() {
    data.poolDictionary = new Dictionary<string, Queue<GameObject>>();
    foreach (var pool in data.pools) {
      Queue<GameObject> objectPool = new Queue<GameObject>();
      for (int i = 0; i < pool.amount; i++) {
        GameObject obj = Instantiate(pool.prefab);
        obj.SetActive(false);
        objectPool.Enqueue(obj);
      }

      data.poolDictionary.Add(pool.prefab.name, objectPool);
    }
    return data.poolDictionary;
  }

  public GameObject SpawnFromPool(string pool, Vector3 position, Quaternion rotation) {
    if (!data.poolDictionary.ContainsKey(pool)) {
      Debug.Log("Pool with tag " + pool + " doesn't exist.");
      return null;
    }

    GameObject objectToSpawn = data.poolDictionary[pool].Dequeue();
    objectToSpawn.SetActive(true);
    objectToSpawn.transform.position = position;
    objectToSpawn.transform.rotation = rotation;
    objectToSpawn.transform.parent = transform;

    data.poolDictionary[pool].Enqueue(objectToSpawn);

    return objectToSpawn;
  }
}
