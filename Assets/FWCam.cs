using Cinemachine;
using UnityEngine;

public class FWCam : MonoBehaviour
{
    [SerializeField] private EnvironmentData env;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    private void Update()
    {
        var distanceFactor = 0.75f;
        var objectWidth = env.instances.x * env.size + (env.instances.x - 1) * env.margin;
        var objectLength = env.instances.z * env.size + (env.instances.z - 1) * env.margin;
        var maxDim = Mathf.Max(objectWidth, objectLength);
        var objectCenter =
            new Vector3(objectWidth, 0, objectLength) / 2.0f -
            new Vector3(env.size, 0, env.size) / 2.0f;
        var cameraView =
            2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad *
                             virtualCamera.m_Lens.FieldOfView); // Visible height 1 meter in front
        var distance = distanceFactor * maxDim / cameraView; // Combined wanted distance from the object
        // distance += 0.5f * maxDim; // Estimated offset from the center to the outside of the object
        virtualCamera.transform.position = objectCenter - distance * virtualCamera.transform.forward;
    }
}