using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEditor;

[CreateAssetMenu]
public class ObjectPoolerData : ScriptableObject {
  public List<Pool> pools;
  public Dictionary<string, Queue<GameObject>> poolDictionary;

  [System.Serializable]
  public class Pool {
    public string poolName;
    public int amount;
    public List<Spawnable> spawnables;
    public List<Material> materials;
  }

  [System.Serializable]
  public class Spawnable {
    public GameObject prefab;

    [Header("Transform")]
    public ScaleMethod scaleMethod = ScaleMethod.None;
    public float minScaleFactor = 1f;
    public float maxScaleFactor = 1f;
    public Vector3 rotationRange = 360f * Vector3.one;
    public bool isStatic = true;

    public enum ScaleMethod {
      None,
      Equal,
      Random,
    }
  }

  //  [System.Serializable]
  //   public class Pool {
  //     public GameObject prefab;
  //     private string tag;
  //     public int amount;
  //   }
}
