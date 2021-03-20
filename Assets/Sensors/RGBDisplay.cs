using UnityEngine;

[ExecuteAlways]
public class RGBDisplay : MonoBehaviour
{
    public float aspectRatio = 16f / 9f;
    public Camera cam;
    public int x = 160;
    public int y = 90;

    private void Awake()
    {
        if (cam == null) cam = GetComponent<Camera>();
    }

    private void Update()
    {
        cam.pixelRect = new Rect(0, 0, x, y);
        cam.aspect = aspectRatio;
    }
}