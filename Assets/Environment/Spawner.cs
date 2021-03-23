using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnOptions
{
    [Header("Spawn Areas")] public List<Collider> allow;

    public List<Collider> avoid;
    public bool isStatic = true;
    public float maxScale = 1f;
    public float minScale = 1f;

    public Vector3 rotationRange = 180f * Vector3.one;

    [Header("Transform")] public ScaleMethod scaleMethod = ScaleMethod.None;
}

public enum ScaleMethod
{
    None,
    Equal,
    Random
}

public class Spawner : MonoBehaviour
{
    public EnvironmentData env;
    public LayerMask ground = -1;
    public ObjectPooler objectPooler;
    public List<Spawnable> objects;

    // private void Start() {
    //   Spawn();
    // }

    public void Spawn()
    {
        DisableAllPoolObjects();
        SpawnAllObjects();
        SpawnAllPoolObjects();
        MoveToGround();
    }

    private void MoveToGround()
    {
        foreach (var thing in objects)
        {
            if (!thing.spawnOnGround) continue;
            thing.transform.PlaceOnGround(ground, thing.groundOffset);
        }
    }

    public void SpawnAllObjects()
    {
        foreach (var thing in objects) SpawnObject(thing);
    }

    public void SpawnObject(Spawnable thing)
    {
        ResetSpawnable(thing);
        RandomizeObject(thing);
    }

    public void DisableAllPoolObjects()
    {
        foreach (Transform child in transform) child.gameObject.SetActive(false);
    }

    public void SpawnAllPoolObjects()
    {
        foreach (var pool in objectPooler.pools)
        {
            var amount =
                pool.scaleAmountWithDifficulty
                    ? env.difficulty <= 0.05f
                        ? 0
                        : Random.Range(
                            0,
                            (int) (pool.amount * env.difficulty * env.difficulty) + 1
                        )
                    : pool.amount;
            for (var i = 0; i < amount; i++) SpawnFromPool(pool.poolName);
        }
    }

    public GameObject SpawnFromPool(string poolName, Vector3? position = null, Quaternion? rotation = null)
    {
        if (objectPooler == null || objectPooler.poolQueues == null) return null;
        if (!objectPooler.poolQueues.ContainsKey(poolName))
        {
            Debug.Log("Pool named \"" + poolName + "\" doesn't exist.");
            return null;
        }

        if (objectPooler.poolQueues[poolName].Count.Equals(0)) return null;

        var obj = objectPooler.poolQueues[poolName].Dequeue();

        if (obj == null) return null;

        obj.SetActive(true);

        ResetObject(obj);
        RandomizePoolObject(obj, objectPooler.poolOptions[poolName]);

        objectPooler.poolQueues[poolName].Enqueue(obj);

        return obj;
    }

    private void RandomizeObject(Spawnable thing)
    {
        SetTransform(thing.transform, thing);

        // TODO: Move mass setup to initialization method and only run on start-up (or preferably on object added)
        if (!thing.isStatic && thing.scaleMethod != ScaleMethod.None && thing.rigidbody != null)
        {
            if (thing.originalMass == null) thing.originalMass = thing.rigidbody.mass;
            thing.rigidbody.mass =
                thing.originalMass != null
                    ? (float) thing.originalMass * thing.transform.localScale.sqrMagnitude
                    : thing.rigidbody.mass = 1f;
        }
    }

    private void RandomizePoolObject(GameObject obj, SpawnOptions options)
    {
        SetTransform(obj.transform, options);

        // TODO: Move mass setup to initialization method and only run on start-up (or preferably on object added)
        if (!options.isStatic)
        {
            var rb = obj.GetComponent<Rigidbody>();
            if (rb != null) rb.mass *= obj.transform.localScale.sqrMagnitude;
        }
    }

    private Transform SetTransform(Transform tf, SpawnOptions options)
    {
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

    private Vector3 GetAllowedRandomPosition(Transform tf, SpawnOptions options)
    {
        if (options.allow.Count > 0)
        {
            var allowed = options.allow[Random.Range(0, options.allow.Count)];

            if (options.avoid.Count == 0)
            {
                tf.position = RandomPointWithinBounds(allowed.bounds);
                return tf.position;
            }

            var maxAttempts = 25;
            var success = false;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var forbiddenCollisions = 0;

                if (success) break;
                tf.position = RandomPointWithinBounds(allowed.bounds);

                var collisions = Physics.OverlapBox(
                    tf.position,
                    tf.lossyScale,
                    tf.rotation
                );

                foreach (var forbidden in options.avoid)
                    foreach (var collision in collisions)
                        if (collision.Equals(forbidden))
                            forbiddenCollisions++;

                if (forbiddenCollisions.Equals(0))
                {
                    success = true;
                    break;
                }
            }

            if (!success) tf.gameObject.SetActive(false);
        }


        return tf.position;
    }

    public Vector3 RandomScale(Vector3 scale, float min, float max, ScaleMethod method = ScaleMethod.None)
    {
        switch (method)
        {
            case ScaleMethod.None:
                return scale;
            case ScaleMethod.Equal:
                var equalScale = Random.Range(min, max);
                return new Vector3(equalScale, equalScale, equalScale);
            case ScaleMethod.Random:
                return RandomVector(min, max);
            default:
                return scale;
        }
    }

    private void ResetObject(GameObject obj)
    {
        obj.transform.localPosition = Vector3.zero;
        obj.transform.position = transform.position;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.rotation = transform.rotation;

        if (!obj.isStatic)
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void ResetSpawnable(Spawnable thing)
    {
        thing.transform.localPosition = Vector3.zero;
        thing.transform.position = transform.position;
        thing.transform.localRotation = Quaternion.identity;
        thing.transform.rotation = transform.rotation;

        if (thing.rigidbody != null)
        {
            thing.rigidbody.velocity = Vector3.zero;
            thing.rigidbody.angularVelocity = Vector3.zero;
        }
    }

    private Vector3 RandomVector(float min, float max)
    {
        return new Vector3(
            Random.Range(min, max),
            Random.Range(min, max),
            Random.Range(min, max)
        );
    }

    private Vector3 RandomPointWithinBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    [Serializable]
    public class Spawnable : SpawnOptions
    {
        public float groundOffset = 0;
        [NonSerialized] public float? originalMass;
        public Rigidbody rigidbody;
        public bool spawnOnGround = false;
        public Transform transform;
    }
}