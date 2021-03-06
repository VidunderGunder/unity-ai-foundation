using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteAlways]
[CreateAssetMenu]
public class EnvironmentData : ScriptableObject
{
    [Header("General")]
    [SerializeField] [Range(0, 1f)] private float difficulty = 0;
    [SerializeField] private int maxSteps = 1000;
    [SerializeField] private Vector3Int instances = Vector3Int.one;
    [SerializeField] private float margin = 5f;
    [SerializeField] private bool debug = false;

    [Header("Grid")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3 startPosition = Vector3.zero;
    [SerializeField] [Range(30f, 500f)] private float size = 100f;
    [SerializeField] private bool skipFirstInstance;

    [Space]
    public bool isTraining = false;

    public float Difficulty { get => difficulty; }
    public int MaxSteps { get => maxSteps; set => maxSteps = value; }
    public Vector3Int Instances
    {
        get => instances;
        set
        {
            for (int i = 0; i < 3; i++)
                instances[i] = value[i] > 0 ? value[i] : 0;
        }
    }
    public float Margin { get => margin; }
    public GameObject Prefab { get => prefab; }
    public float Size { get => size; }
    public bool SkipFirstInstance { get => skipFirstInstance; }
    public Vector3 StartPosition { get => startPosition; }
    public bool Debug { get => debug; }
    // public bool IsTraining { get => isTraining; set => isTraining = value; }

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