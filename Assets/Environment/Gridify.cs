using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gridify : MonoBehaviour
{
    [SerializeField] private EnvironmentData env;
    [SerializeField] private List<GameObject> pools;

    private Vector3 distance;

    private void Awake()
    {
        Sync();
    }

    private void Sync()
    {
        // if (!Application.isPlaying)
        // {
        //     foreach (Transform child in transform)
        //     {
        //         child.gameObject.SetActive(false);
        //         DestroyImmediate(child.gameObject, true);
        //     }
        SyncDistance();
        FillPool();
        // }
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }
    }

    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }
    }

    private void SyncDistance()
    {
        if (env == null) return;
        distance = new Vector3(
            env.Size + env.Margin,
            50,
            env.Size + env.Margin
        );
    }

    private void FillPool()
    {
        for (var x = 0; x < env.Instances.x; x++)
            for (var z = 0; z < env.Instances.z; z++)
                for (var y = 0; y < env.Instances.y; y++)
                    if (!(env.SkipFirstInstance && x.Equals(0) && y.Equals(0) && z.Equals(0)))
                    {
                        Vector3 spot = new Vector3(
                            env.StartPosition.x + x * distance.x,
                            env.StartPosition.y + y * distance.y,
                            env.StartPosition.z + z * distance.z
                        );
                        var obj = Instantiate(
                            env.Prefab,
                            spot,
                            Quaternion.identity
                        );
                        obj.transform.parent = transform;
                    }
    }
}