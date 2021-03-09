using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Spawner : MonoBehaviour {
  public ObjectPooler objectPooler;

  private void Start() {
    objectPooler = ObjectPooler.Instance;
    Spawn();
  }

  public void Spawn() {
    StartCoroutine(PeriodicObjectPoolSpawn(0.01f, "Primitives"));
    StartCoroutine(PeriodicObjectPoolSpawn(0.01f, "Terrains"));
  }

  IEnumerator PeriodicObjectPoolSpawn(float period, string pool) {
    for (; ; ) {
      if (objectPooler == null || objectPooler.SpawnFromPool(pool) == null) yield break;
      yield return new WaitForSeconds(period);
    }
  }
}
