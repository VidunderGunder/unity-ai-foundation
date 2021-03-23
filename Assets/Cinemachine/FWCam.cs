using Cinemachine;
using UnityEngine;

public class FWCam : MonoBehaviour
{
    [SerializeField] private EnvironmentData env;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    private void Update()
    {
        var distanceFactor = 0.75f;
        var objectWidth = env.Instances.x * env.Size + (env.Instances.x - 1) * env.Margin;
        var objectLength = env.Instances.z * env.Size + (env.Instances.z - 1) * env.Margin;
        var maxDim = Mathf.Max(objectWidth, objectLength);
        var objectCenter =
            new Vector3(objectWidth, 0, objectLength) / 2.0f -
            new Vector3(env.Size, 0, env.Size) / 2.0f;
        var cameraView =
            2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad *
                             virtualCamera.m_Lens.FieldOfView); // Visible height 1 meter in front
        var distance = distanceFactor * maxDim / cameraView; // Combined wanted distance from the object
        // distance += 0.5f * maxDim; // Estimated offset from the center to the outside of the object
        virtualCamera.transform.position = objectCenter - distance * virtualCamera.transform.forward;
    }
}