using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [Tooltip("start, control 1, control 2, end")] [Obsolete("Use bladeShape curve instead")]
    public Vector2[] bezierPoints = new Vector2[4];

    [SerializeField] private AnimationCurve bladeShape;
    private float _bladeShapeDuration;
    [SerializeField] private AnimationCurve bladeWidth;
    private float _bladeWidthDuration;

    [Range(5, 25)] [SerializeField] private int vertexCount = 11;
    [SerializeField] private float bladeHeight = 1.0f;
    [SerializeField] private Material grassMaterial;

    [SerializeField] private int bladeCount = 100;

    private void DrawBezierCurve()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(bezierPoints[0].x, bezierPoints[0].y, 0), 0.05f);
        Gizmos.DrawSphere(new Vector3(bezierPoints[3].x, bezierPoints[3].y, 0), 0.05f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(bezierPoints[1].x, bezierPoints[1].y, 0), 0.05f);
        Gizmos.DrawSphere(new Vector3(bezierPoints[2].x, bezierPoints[2].y, 0), 0.05f);

        Gizmos.color = Color.white;
        for (var i = 0; i <= vertexCount; i++)
        {
            var t = i / (float)vertexCount;
            var point = CalculateBezierPoint(t, bezierPoints[0], bezierPoints[1], bezierPoints[2], bezierPoints[3]);
            Gizmos.DrawSphere(new Vector3(point.x, point.y, 0), 0.025f);
        }
    }

    private Mesh CreateGrassBladeMesh()
    {
        var mesh = new Mesh();
        // meshFilter.mesh = mesh;

        var vertices = new Vector3[vertexCount];
        var triangles = new int[(vertexCount - 1) * 6];

        // create vertices
        for (var i = 0; i < vertexCount - 3; i++)
        {
            var width = bladeWidth.Evaluate(_bladeWidthDuration * (i / (float)vertexCount));
            var point = CalculateBezierPoint(i / (float)vertexCount, bezierPoints);
            vertices[i++] = new Vector3(-width / 2, point.y * bladeHeight, point.x);
            vertices[i] = new Vector3(width / 2, point.y * bladeHeight, point.x);
        }

        // add tip
        {
            // TODO: replace y and z with bezier position
            var point = CalculateBezierPoint((vertexCount - 3) / (float)(vertexCount - 1), bezierPoints);
            var width = bladeWidth.Evaluate(_bladeWidthDuration * (vertexCount - 1) / vertexCount);
            vertices[^3] = new Vector3(-width, point.y * bladeHeight, point.x);
            vertices[^2] = new Vector3(width, point.y * bladeHeight, point.x);
            point = CalculateBezierPoint(1, bezierPoints);
            vertices[^1] = new Vector3(0, point.y, point.x);
        }

        // create triangles
        {
            var i = 0;
            for (var row = 0; row < vertexCount - 3; row += 2)
            {
                // flip the order of the triangles above
                triangles[i++] = row;
                triangles[i++] = row + 2;
                triangles[i++] = row + 1;
                triangles[i++] = row + 1;
                triangles[i++] = row + 2;
                triangles[i++] = row + 3;
            }

            // add tip
            triangles[^3] = vertexCount - 3;
            triangles[^2] = vertexCount - 1;
            triangles[^1] = vertexCount - 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    private static Vector2 CalculateBezierPoint(float t, IReadOnlyList<Vector2> points)
    {
        return CalculateBezierPoint(t, points[0], points[1], points[2], points[3]);
    }

    [Obsolete("Use blade shape curve instead")]
    private static Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        var u = 1 - t;
        var tt = t * t;
        var uu = u * u;
        var uuu = uu * u;
        var ttt = tt * t;

        var p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;

        return p;
    }

    float GetAnimationCurveDuration(AnimationCurve curve)
    {
        // Check if the curve has any keys.
        if (curve.keys.Length == 0)
            return 0f;
        // Get the first and last keyframes.
        var firstKey = curve.keys[0];
        var lastKey = curve.keys[^1];

        // Calculate the duration.
        return lastKey.time - firstKey.time;
    }


    private void Awake()
    {
        _bladeShapeDuration = GetAnimationCurveDuration(bladeShape);
        _bladeWidthDuration = GetAnimationCurveDuration(bladeWidth);

        vertexCount = Mathf.Max(5, vertexCount);
        if (vertexCount % 2 == 0)
        {
            vertexCount++;
        }

        var planeDimensions = GameObject.Find("Platform").GetComponent<PlaneGenerator>().dimensions;
        var sqrt = Mathf.Ceil(Mathf.Sqrt(bladeCount));

        var mesh = CreateGrassBladeMesh();

        for (int x = 0, i = 0; x < sqrt; x++)
        for (var z = 0; z < sqrt; z++)
        {
            var blade = new GameObject($"GrassBlade{i++}", typeof(GrassBlade));
            blade.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { grassMaterial });
            blade.GetComponent<MeshFilter>().mesh = mesh;

            blade.transform.SetPositionAndRotation(
                new Vector3((x / sqrt) * planeDimensions.x + Random.Range(-0.1f, 0.1f), 0,
                    (z / sqrt) * planeDimensions.y + Random.Range(-0.1f, 0.1f)),
                Quaternion.Euler(0, Random.Range(0, 360), 0));
            blade.transform.lossyScale.Set(1, Random.Range(0.8f, 1.2f), 1);
        }
    }
}