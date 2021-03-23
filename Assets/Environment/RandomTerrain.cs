using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RandomTerrain : MonoBehaviour
{
    public EnvironmentData env;
    public List<MeshGenerator> generators;

    public void Randomize()
    {
        foreach (var gen in generators) gen.scale = env.Size;
    }

    private void OnValidate()
    {
        Randomize();
    }

    private void Start()
    {
        Randomize();
    }
}