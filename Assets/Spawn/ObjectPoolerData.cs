using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ObjectPoolerData : ScriptableObject {
  public List<Pool> pools;
  public Dictionary<string, Queue<GameObject>> poolQueues;
  public Dictionary<string, PoolOptions> poolOptions;

  [System.Serializable]
  public class PoolOptions : SpawnOptions {
    [Header("General")]
    public string poolName;
    public int amount;
  }

  [System.Serializable]
  public class Pool : PoolOptions {
    [Header("Objects to Spawn")]
    public List<GameObject> prefabs;
  }

  [System.Serializable]
  public class Spawnable {
    public GameObject prefab;
  }
}
