using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class PlaneGenerator : MonoBehaviour
{
    public Vector2 dimensions = new(0, 0);

    [SerializeField] private int resolution = 1;

    [SerializeField] private bool drawGizmos;

    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private Vector2 _prevDimensions;
    private int _prevResolution;
    private int[] _triangles;

    private Vector3[] _vertices;

    private void Awake()
    {
        _mesh = new Mesh();
        _meshFilter = GetComponent<MeshFilter>();
        _mesh.indexFormat = IndexFormat.UInt32;
        _meshFilter.mesh = _mesh;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_prevResolution == resolution || _prevDimensions == dimensions)
            return;

        _prevResolution = resolution;
        _prevDimensions = dimensions;

        GeneratePlane();
        AssignMesh();
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;
        Gizmos.color = Color.red;
        foreach (var vertex in _vertices)
            Gizmos.DrawSphere(vertex, 0.1f);
    }

    private void GeneratePlane()
    {
        // Create vertices
        _vertices = new Vector3[(resolution + 1) * (resolution + 1)];

        var xStep = dimensions.x / resolution;
        var yStep = dimensions.y / resolution;

        for (var x = 0; x <= resolution; x++)
        for (var y = 0; y <= resolution; y++)
            _vertices[x * (resolution + 1) + y] = new Vector3(x * xStep, 0, y * yStep);

        // Create triangles
        _triangles = new int[resolution * resolution * 6];

        ulong i = 0;
        for (var row = 0; row < resolution; row++)
        for (var col = 0; col < resolution; col++)
        {
            // first triangle
            _triangles[i++] = row * (resolution + 1) + col;
            _triangles[i++] = row * (resolution + 1) + col + 1;
            _triangles[i++] = (row + 1) * (resolution + 1) + col;

            // second triangle
            _triangles[i++] = (row + 1) * (resolution + 1) + col;
            _triangles[i++] = row * (resolution + 1) + col + 1;
            _triangles[i++] = (row + 1) * (resolution + 1) + col + 1;
        }
    }

    private void AssignMesh()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.RecalculateNormals();
    }
}