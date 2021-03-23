using UnityEngine;

[ExecuteAlways]
[CreateAssetMenu]
public class EnvironmentData : ScriptableObject
{
    [Header("General")] [Range(0, 1f)] public float difficulty = 0;

    public Vector3Int instances = Vector3Int.one;
    public float margin = 5f;
    public float cumulativeRewardToNextDifficulty = 10f;
    public float nextDifficulty = 0.1f;

    [Header("Grid")] public GameObject prefab;

    [Range(30f, 500f)] public float size = 100f;
    public bool skipFirstInstance;
    public Vector3 startPosition = Vector3.zero;
}