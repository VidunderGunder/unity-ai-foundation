using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ObjectPoolerData : ScriptableObject {
  public List<Pool> pools;
  public Dictionary<string, Queue<GameObject>> poolDictionary;

  public class Pool {
    public GameObject prefab;
    public int amount;
  }
}
