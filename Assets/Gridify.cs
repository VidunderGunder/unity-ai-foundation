using UnityEngine;

public class Gridify : MonoBehaviour
{
    private Vector3 distance;
    public EnvironmentData env;

    private void Start()
    {
        if (env == null) return;

        distance = new Vector3(
            env.size + env.margin,
            50,
            env.size + env.margin
        );

        for (var x = 0; x < env.instances.x; x++)
        for (var z = 0; z < env.instances.z; z++)
        for (var y = 0; y < env.instances.y; y++)
            if (!(env.skipFirstInstance && x.Equals(0) && y.Equals(0) && z.Equals(0)))
                Instantiate(
                    env.prefab,
                    new Vector3(
                        env.startPosition.x + x * distance.x,
                        env.startPosition.y + y * distance.y,
                        env.startPosition.z + z * distance.z
                    ),
                    Quaternion.identity
                );
    }
}