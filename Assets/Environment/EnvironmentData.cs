using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteAlways]
[CreateAssetMenu]
public class EnvironmentData : ScriptableObject
{
    [Header("General")]
    [SerializeField] [Range(0, 1f)] private float difficulty = 0;
    [SerializeField] private Vector3Int instances = Vector3Int.one;
    [SerializeField] private float margin = 5f;
    [SerializeField] private bool debug = false;
    public float cumulativeRewardToNextDifficulty = 10f;
    public float nextDifficulty = 0.1f;

    [Header("Grid")]
    [SerializeField] private GameObject prefab;
    [SerializeField] [Range(30f, 500f)] private float size = 100f;
    [SerializeField] private bool skipFirstInstance;
    [SerializeField] private Vector3 startPosition = Vector3.zero;


    public float Difficulty { get => difficulty; }
    public Vector3Int Instances { get => instances; }
    public float Margin { get => margin; }
    public GameObject Prefab { get => prefab; }
    public float Size { get => size; }
    public bool SkipFirstInstance { get => skipFirstInstance; }
    public Vector3 StartPosition { get => startPosition; }
    public bool Debug { get => debug; }

#if UNITY_EDITOR
    /// <summary>
    /// Sets difficulty to given value, but forces value to range [0, 1f]. 
    /// Debug mode enables change blaming, to see what method (and which file) is responsible for changes.
    /// </summary>
    public void SetDifficulty(
        float value,
        [CallerMemberName]
        string caller = "",
        [CallerFilePath]
        string file = ""
    )
    {
        if (debug)
        {
            UnityEngine.Debug.Log(
                "Difficulty changed to "
                + value.ToString()
                + " by method \""
                + caller
                + "\" in file \""
                + file
                + "\"."
            );
        }
        HandleSetDifficulty(value);
    }
#else
    public void SetDifficulty(float value) { HandleSetDifficulty(value); }
#endif

    private void HandleSetDifficulty(float value)
    {
        difficulty =
            value >= 1f
            ? 1f
            : value <= 0
            ? 0
            : value;
    }
}