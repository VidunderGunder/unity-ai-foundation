using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
  public ObjectPooler objectPooler;

  private void Start() {
    objectPooler = ObjectPooler.Instance;
    StartCoroutine(PeriodicObjectPoolSpawn(0.01f));
  }

  IEnumerator PeriodicObjectPoolSpawn(float period) {
    for (; ; ) {
      yield return new WaitForSeconds(period);
      foreach (var pool in objectPooler.data.poolDictionary) {
        if (objectPooler.SpawnFromPool(pool.Key, transform.position, Quaternion.identity).Equals(null)) yield break;
      }
    }
  }

  private void Randomize(GameObject thing) {
    // RandomThrow(thing.GetComponent<Rigidbody>());
    // RandomScaleEqually(thing.transform);
    // RandomScale(thing.transform);
  }

  private void RandomThrow(Rigidbody thing) {
    float max = 10f;
    thing.GetComponent<Rigidbody>().velocity = RandomVector(-max, max);
  }

  private void RandomScale(Transform thing, float min = 0.25f, float max = 2f) {
    thing.localScale = RandomVector(min, max);
  }

  private void RandomScaleEqually(Transform thing, float min = 0.25f, float max = 2f) {
    float scale = Random.Range(min, max);
    thing.localScale = new Vector3(scale, scale, scale);
  }

  private Vector3 RandomVector(float min, float max) {
    return new Vector3(
      Random.Range(min, max),
      Random.Range(min, max),
      Random.Range(min, max)
  );
  }
}
