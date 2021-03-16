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
  public List<Collider> allow;
  public List<Collider> avoid;
}

public enum ScaleMethod {
  None,
  Equal,
  Random,
}

public class Spawner : MonoBehaviour {
  public EnvironmentData env;
  public List<Spawnable> objects;
  public ObjectPooler objectPooler;

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
    DisableAllPoolObjects();
    SpawnAllObjects();
    SpawnAllPoolObjects();
  }

  public void SpawnAllObjects() {
    foreach (var thing in objects) {
      SpawnObject(thing);
    }
  }

  public void SpawnObject(Spawnable thing) {
    ResetObject(thing.transform.gameObject);
    RandomizeObject(thing);
  }

  public void DisableAllPoolObjects() {
    foreach (Transform child in transform) {
      child.gameObject.SetActive(false);
    }
  }

  public void SpawnAllPoolObjects() {
    foreach (var pool in objectPooler.pools) {
      for (int i = 0; i < pool.amount; i++) {
        SpawnFromPool(pool.poolName);
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

    objectPooler.poolQueues[poolName].Enqueue(obj);

    return obj;
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
    tf.position = GetAllowedRandomPosition(tf, options);

    return tf;
  }

  private Vector3 GetAllowedRandomPosition(Transform tf, SpawnOptions options) {
    if (options.allow.Count > 0) {
      Collider allowed = options.allow[Random.Range(0, options.allow.Count)];

      if (options.avoid.Count == 0) {
        tf.position = RandomPointWithinBounds(allowed.bounds);
        return tf.position;
      }

      int maxAttempts = 25;
      bool success = false;

      for (var attempt = 0; attempt < maxAttempts; attempt++) {
        int forbiddenCollisions = 0;

        if (success) break;
        tf.position = RandomPointWithinBounds(allowed.bounds);

        Collider[] collisions = Physics.OverlapBox(
          tf.position,
          tf.lossyScale,
          tf.rotation
        );

        foreach (var forbidden in options.avoid) {
          foreach (var collision in collisions) {
            if (collision.Equals(forbidden)) {
              forbiddenCollisions++;
            }
          }
        }

        if (forbiddenCollisions.Equals(0)) {
          success = true;
          break;
        }
      }
    }

    return tf.position;
  }

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
}
