using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ObjectPoolerData : ScriptableObject {
  public List<Pool> pools;
  public Dictionary<string, Queue<GameObject>> poolQueues;
  public Dictionary<string, PoolOptions> poolOptions;

  public class PoolOptions {
    public string poolName;
    public int amount;

    [Header("Transform")]
    public ScaleMethod scaleMethod = ScaleMethod.None;
    public float minScale = 1f;
    public float maxScale = 1f;
    public Vector3 rotationRange = 360f * Vector3.one;
    public bool isStatic = true;

    [Header("Spawn Areas")]
    public List<string> allowedSpawnAreas;
    public List<string> forbiddenSpawnAreas;
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
