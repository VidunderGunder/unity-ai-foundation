using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ObjectPoolerData : ScriptableObject
{
    public Dictionary<string, PoolOptions> poolOptions;
    public Dictionary<string, Queue<GameObject>> poolQueues;
    public List<Pool> pools;

    [Serializable]
    public class PoolOptions : SpawnOptions
    {
        public int amount;

        [Header("General")] public string poolName;
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