using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[HelpURL("http://example.com/docs/MyComponent.html")]
public class ObjectPooler : MonoBehaviour
{
    public Dictionary<string, PoolOptions> poolOptions;

    public Dictionary<string, Queue<GameObject>> poolQueues;

    // public ObjectPoolerData data;
    public List<Pool> pools;

    private void OnEnable()
    {
        Cleanup();
        InitializePools();
    }

    private void OnDisable()
    {
        Cleanup();
    }

    [ContextMenu("Clear Pool & Delete Children")]
    public void Cleanup()
    {
        if (Application.IsPlaying(gameObject))
            foreach (Transform child in transform)
                Destroy(child.gameObject);
        else
            foreach (Transform child in transform)
                DestroyImmediate(child.gameObject);

        if (poolQueues == null) return;
        if (Application.IsPlaying(gameObject))
            foreach (var pool in poolQueues)
            foreach (var obj in pool.Value)
                Destroy(obj);
        else
            foreach (var pool in poolQueues)
            foreach (var obj in pool.Value)
                DestroyImmediate(obj);
        poolQueues = null;
    }

    [ContextMenu("Fill Pools")]
    public Dictionary<string, Queue<GameObject>> InitializePools()
    {
        poolQueues = new Dictionary<string, Queue<GameObject>>();
        poolOptions = new Dictionary<string, PoolOptions>();

        foreach (var pool in pools)
        {
            if (pool.prefabs.Count.Equals(0)) break;
            var objectPool = new Queue<GameObject>();

            for (var i = 0; i < pool.amount; i++)
            {
                var obj = Instantiate(pool.prefabs[Random.Range(0, pool.prefabs.Count)]);
                var rb = obj.GetComponent<Rigidbody>();
                obj.SetActive(false);
                obj.transform.parent = transform;
                obj.isStatic = pool.isStatic;
                if (obj.isStatic)
                {
                    if (rb != null) rb.isKinematic = true;
                }
                else
                {
                    if (rb != null) rb.mass *= obj.transform.localScale.sqrMagnitude;
                }

                objectPool.Enqueue(obj);
            }

            poolQueues.Add(pool.poolName, objectPool);
            PoolOptions options = pool;
            poolOptions.Add(pool.poolName, options);
        }

        return poolQueues;
    }

    [Serializable]
    public class PoolOptions : SpawnOptions
    {
        public int amount;

        [Header("General")] public string poolName;

        public bool scaleAmountWithDifficulty = false;
    }

    [Serializable]
    public class Pool : PoolOptions
    {
        [Header("Objects to Spawn")] public List<GameObject> prefabs;
    }

    [Serializable]
    public class Spawnable
    {
        public GameObject prefab;
    }
}