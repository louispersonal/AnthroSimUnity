using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomWalkVector
{
    public static List<Vector2Int> RandomWalk(Vector2Int startPoint, Vector2Int endPoint, float numVerticesRatio, float maxAngleDisplacement)
    {
        int numVertices = Mathf.FloorToInt(Vector2Int.Distance(endPoint, startPoint) * numVerticesRatio);

        List<Vector2Int> vertices = new List<Vector2Int>();
        for (int c = 0; c < numVertices; c++)
        {
            vertices.Add(new Vector2Int(0, 0));
        }

        vertices[0] = startPoint;
        vertices[numVertices - 1] = endPoint;

        vertices = SubdivideRecursive(vertices, 0, numVertices - 1, maxAngleDisplacement);

        return InterpolatePoints(vertices);
    }

    private static List<Vector2Int> SubdivideRecursive(List<Vector2Int> vertices, int start, int end, float maxAngleDisplacement)
    {
        if (end - start <= 1)
        {
            return vertices;
        }

        int midPointIndex = (start + end) / 2;
        Vector2Int midPoint = FindMidpoint(vertices[start], vertices[end]);
        Vector2Int displacedMidpoint = DisplacePoint(vertices[start], midPoint, maxAngleDisplacement);
        vertices[midPointIndex] = displacedMidpoint;

        if (maxAngleDisplacement > 1f)
        {
            maxAngleDisplacement--;
        }

        // Recursively subdivide the left and right sections
        SubdivideRecursive(vertices, start, midPointIndex, maxAngleDisplacement);
        SubdivideRecursive(vertices, midPointIndex, end, maxAngleDisplacement);

        return vertices;  // Return the list after all recursive calls
    }

    private static Vector2Int FindMidpoint(Vector2Int startPoint, Vector2Int endPoint)
    {
        Vector2Int midpoint = startPoint + new Vector2Int((endPoint.x - startPoint.x) / 2, (endPoint.y - startPoint.y) / 2);
        return midpoint;
    }

    private static Vector2Int DisplacePoint(Vector2Int fixedPoint, Vector2Int displacePoint, float maxAngleDisplacement)
    {
        Vector2Int vector = displacePoint - fixedPoint;
        float displacementAngle = Random.Range(-maxAngleDisplacement, maxAngleDisplacement);
        Vector2Int displacementVector = RotateVector2Int(vector, displacementAngle);
        return fixedPoint + displacementVector;
    }
    private static Vector2Int RotateVector2Int(Vector2Int originalVector, float angleInDegrees)
    {
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector2 original = originalVector;

        float cosAngle = Mathf.Cos(angleInRadians);
        float sinAngle = Mathf.Sin(angleInRadians);

        Vector2 rotated = new Vector2(original.x * cosAngle - original.y * sinAngle, original.x * sinAngle + original.y * cosAngle);

        return new Vector2Int(Mathf.RoundToInt(rotated.x), Mathf.RoundToInt(rotated.y));
    }

    public static Vector2 RotateVector2(Vector2 original, float angleInDegrees)
    {
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;

        float cosAngle = Mathf.Cos(angleInRadians);
        float sinAngle = Mathf.Sin(angleInRadians);

        Vector2 rotated = new Vector2(original.x * cosAngle - original.y * sinAngle, original.x * sinAngle + original.y * cosAngle);

        return rotated;
    }

    private static List<Vector2Int> InterpolatePoints(List<Vector2Int> vertices)
    {
        List<Vector2Int> interpolatedPoints = new List<Vector2Int>();

        for (int i = 0; i < vertices.Count - 1; i++)
        {
            Vector2Int start = vertices[i];
            Vector2Int end = vertices[i + 1];

            interpolatedPoints.AddRange(GetLinePoints(start, end));
        }
        return interpolatedPoints;
    }

    private static List<Vector2Int> GetLinePoints(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> linePoints = new List<Vector2Int>();

        int x0 = start.x;
        int y0 = start.y;
        int x1 = end.x;
        int y1 = end.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int err = dx - dy;

        while (true)
        {
            linePoints.Add(new Vector2Int(x0, y0));

            if (x0 == x1 && y0 == y1) break;

            int e2 = err * 2;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return linePoints;
    }
}
