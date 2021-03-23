using UnityEngine;

public class Gridify : MonoBehaviour
{
    private Vector3 distance;
    public EnvironmentData env;

    private void Start()
    {
        if (env == null) return;

        distance = new Vector3(
            env.Size + env.Margin,
            50,
            env.Size + env.Margin
        );

        for (var x = 0; x < env.Instances.x; x++)
            for (var z = 0; z < env.Instances.z; z++)
                for (var y = 0; y < env.Instances.y; y++)
                    if (!(env.SkipFirstInstance && x.Equals(0) && y.Equals(0) && z.Equals(0)))
                        Instantiate(
                            env.Prefab,
                            new Vector3(
                                env.StartPosition.x + x * distance.x,
                                env.StartPosition.y + y * distance.y,
                                env.StartPosition.z + z * distance.z
                            ),
                            Quaternion.identity
                        );
    }
}