using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[ExecuteAlways]
public class EnvironmentTerrainGenerator : MonoBehaviour
{
    private readonly float resolution = 0.5f;
    public float borderHeight = 0;

    [Header("Mesh Borders")] public bool borders = true;

    public int borderThickness = 1;
    public bool centered = true;
    private float displacement;

    [Header("Data")] public EnvironmentData env;

    public float maxDisplacement = -1.25f;

    private Mesh mesh;
    private MeshCollider meshCollider;
    private int[] triangles;

    [Header("Mesh Options")]
    [Range(0.1f, 1f)]
    public float unevenness = 0.1f;

    private Vector3[] vertices;
    private int xNodes;
    private float xScale;
    private int zNodes;
    private float zScale;

    // TODO: Trigger on events:
    private void Awake()
    {
        Generate();
    }

    private void Enabled()
    {
        Generate();
    }

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        var simple = false;

        if (simple)
        {
            xNodes = 2;
            zNodes = 2;
        }
        else
        {
            xNodes = (int) Mathf.Round(resolution * env.Size);
            zNodes = (int) Mathf.Round(resolution * env.Size);
        }

        xScale = env.Size / xNodes;
        zScale = env.Size / zNodes;

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        var flat = Random.Range(0, 1f) > 0.5f ? true : false;
        displacement = flat ? 0 : Random.Range(0, maxDisplacement);

        CreateShape();
        UpdateMesh();

        meshCollider = new MeshCollider();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    private void CreateShape()
    {
        vertices = new Vector3[(xNodes + 1) * (zNodes + 1)];

        var xStart = centered ? -zNodes / 2f : 0;
        var xEnd = centered ? zNodes / 2f : zNodes;
        var zStart = centered ? -xNodes / 2f : 0;
        var zEnd = centered ? xNodes / 2f : xNodes;

        for (float i = 0, z = zStart; z <= zEnd; z++)
            for (var x = xStart; x <= xEnd; x++)
            {
                float y;

                if (
                    (borders &
                     (x <= (int) xStart + borderThickness)) |
                    (x >= (int) xEnd - borderThickness) |
                    (z <= (int) zStart + borderThickness) |
                    (z >= (int) zEnd - borderThickness)
                )
                    y = borderHeight;
                else
                    y = Mathf.PerlinNoise(x * unevenness, z * unevenness) * maxDisplacement;

                vertices[(int) i] = new Vector3(x * xScale, y, z * zScale);
                i++;
            }

        triangles = new int[xNodes * zNodes * 6];
        var vertexCount = 0;
        var triangleCount = 0;

        for (var z = 0; z < zNodes; z++)
        {
            for (var x = 0; x < xNodes; x++)
            {
                triangles[triangleCount + 0] = vertexCount + 0;
                triangles[triangleCount + 1] = vertexCount + xNodes + 1;
                triangles[triangleCount + 2] = vertexCount + 1;
                triangles[triangleCount + 3] = vertexCount + 1;
                triangles[triangleCount + 4] = vertexCount + xNodes + 1;
                triangles[triangleCount + 5] = vertexCount + xNodes + 2;

                vertexCount++;
                triangleCount += 6;
            }

            vertexCount++;
        }
    }

    private void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }
}