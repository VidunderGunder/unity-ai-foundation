using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class DepthCamera : MonoBehaviour
{
    public Camera depthCamera;
    public Material depthMaterial;

    private void Awake()
    {
        if (depthCamera == null) depthCamera = GetComponent<Camera>();
        depthCamera.depthTextureMode = DepthTextureMode.DepthNormals;

        if (depthMaterial == null) depthMaterial = new Material(Shader.Find("DepthShader"));
    }

    private void OnPreRender()
    {
        Shader.SetGlobalMatrix(Shader.PropertyToID("UNITY_MATRIX_IV"), depthCamera.cameraToWorldMatrix);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, depthMaterial);
    }
}