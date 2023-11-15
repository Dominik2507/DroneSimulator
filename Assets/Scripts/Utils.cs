using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static Vector3 CalculateHalfAngleVector(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        Vector3 AB = pointB - pointA;
        Vector3 AC = pointC - pointA;

        // Calculate the angle between AB and AC
        float angle = Vector3.SignedAngle(AB, AC, Vector3.up);

        // Ensure the angle is positive (clockwise from AB)
        if (angle < 0)
        {
            angle += 360;
        }

        // Calculate the half-angle in degrees
        float halfAngle = angle / 2;

        // Get the normalized vectors of AB and AC
        Vector3 normalizedAB = AB.normalized;
        Vector3 normalizedAC = AC.normalized;

        // Rotate the normalized vector AB by half of the angle between AB and AC
        Quaternion rotation = Quaternion.Euler(0, halfAngle, 0);
        Vector3 halfAngleVector = rotation * normalizedAB;

        return halfAngleVector;
    }

    public static Vector3? GetMiddlePointIfSmallArea(Vector3 pointA, Vector3 pointB, Vector3 pointC, float thresholdArea)
    {
        // Calculate the vectors AB and AC
        Vector3 AB = pointB - pointA;
        Vector3 AC = pointC - pointA;

        // Calculate the cross product of AB and AC to get the area of the triangle
        float triangleArea = Vector3.Cross(AB, AC).magnitude / 2;

        // Check if the area is less than the provided threshold
        if (triangleArea < thresholdArea)
        {
            // Calculate the middle point of the triangle
            Vector3 middlePoint = (pointA + pointB + pointC) / 3;
            return middlePoint;
        }
        else
        {
            // If the area is greater than the threshold, return null
            return null;
        }
    }

    public static Vector3? GetMiddlePointIfWithinSquare(List<Vector3> polygonPoints, float x)
    {
        // Find the minimum and maximum coordinates to determine the bounding box
        float minX = float.MaxValue, minZ = float.MaxValue;
        float maxX = float.MinValue, maxZ = float.MinValue;

        foreach (Vector3 point in polygonPoints)
        {
            minX = Mathf.Min(minX, point.x);
            minZ = Mathf.Min(minZ, point.y);
            maxX = Mathf.Max(maxX, point.x);
            maxZ = Mathf.Max(maxZ, point.y);
        }

        // Calculate the dimensions of the bounding box
        float boxWidth = maxX - minX;
        float boxHeight = maxZ - minZ;

        // Check if the bounding box fits within the x * x square
        if (boxWidth < x && boxHeight < x)
        {
            // Calculate the middle point of the bounding box
            Vector3 middlePoint = new Vector3((minX + maxX) / 2, (minZ + maxZ) / 2, 0);
            return middlePoint;
        }
        else
        {
            return null; // Bounding box doesn't fit in the x * x square
        }
    }


    public static bool IsPointInPolygon(List<Vector3> polygon, Vector3 pointP)
    {
        int polygonLength = polygon.Count;
        bool isInside = false;

        for (int i = 0, j = polygonLength - 1; i < polygonLength; j = i++)
        {
            if (((polygon[i].z > pointP.z) != (polygon[j].z > pointP.z)) &&
                (pointP.x < (polygon[j].x - polygon[i].x) * (pointP.z - polygon[i].z) / (polygon[j].z - polygon[i].z) + polygon[i].x))
            {
                isInside = !isInside;
            }
        }

        return isInside;
    }

    public static void RecursiveSplit(List<Vector3> points, List<List<Vector3>> result)
    {
        if (points.Count < 4)
        {
            result.Add(points);
            return;
        }

        int maxIterations = 50;
        int iter = 0;
        int n = points.Count;
        for (int i = 0; i < n; i++)
        {
            int nexti = (i + 1) % n;
            int j = (i + 2) % n;
           
            while ((j+1)%n != i)
            {
                if(maxIterations < iter)
                {
                    Debug.LogError("Max iterations reached");
                    return;
                }
                int nextj = (j + 1) % n;

                iter++;

                if (DoIntersect(points[i], points[nexti], points[j], points[nextj], out Vector3 intersection))
                {
                    Debug.Log(i + " " + nexti + " " + j + " " + nextj);

                    List<Vector3> poly1 = new();
                    List<Vector3> poly2 = new();

                    poly1.Add(intersection);
                    poly2.Add(intersection);

                    for (int k = nexti; k != (j + 1) % n; k = (k + 1) % n)
                    {
                        poly1.Add(points[k]);
                    }

                    for (int k = nextj; k != (i + 1) % n; k = (k + 1) % n)
                    {
                        poly2.Add(points[k]);
                    }

                    if (!IsClockwise(poly1))
                    {
                        poly1.Remove(intersection);
                        poly1.Add(intersection);
                        poly1.Reverse();
                    }

                    if (!IsClockwise(poly2))
                    {
                        poly2.Remove(intersection);
                        poly2.Add(intersection);
                        poly2.Reverse();
                    }

                    // Add the split polygons to the result
                    RecursiveSplit(poly1, result);
                    RecursiveSplit(poly2, result);
                    return;
                }
                j = (j + 1) % n;
            }
        }
        // If no intersections found, add the whole polygon
        result.Add(points);
    }

    static bool DoIntersect(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2, out Vector3 intersection)
    {
        //Debug.Log("Started checking intersection.");
        float slope1 = (q1.z - p1.z) / (p1.x - q1.x);
        float slope2 = (q2.z - p2.z) / (p2.x - q2.x);

        intersection = Vector3.zero;

        // Check if the lines are not parallel
        if (!Mathf.Approximately(slope1, slope2)
            //|| IsPointOnSegment(p1, p2, q2)
            //|| IsPointOnSegment(q1, p2, q2)
            //|| IsPointOnSegment(p2, p1, q1)
            //|| IsPointOnSegment(q2, p1, q1)
        )
        {
            intersection = FindIntersection(p1, q1, p2, q2);

            bool IsIntersecting = IsPointOnSegment(intersection, p1, q1) && IsPointOnSegment(intersection, p2, q2);

            if (IsIntersecting)
            {
                Debug.Log("FOUND INTERSECTION");
                Debug.Log(intersection.ToString());
                return true;
            }

            //Debug.Log("Intersection is not inside segments");
        }
       
        //Debug.Log("Doesnt intersect: " + "slope1 = " + slope1 + ", slope2 = " + slope2);
        //Debug.Log(intersection.ToString());
        

        return false;
    }

    static bool IsPointOnSegment(Vector3 p, Vector3 q1, Vector3 q2)
    {
        // Check if point P is on the segment formed by Q1 and Q2
        return (p.x <= Mathf.Max(q1.x, q2.x) && p.x >= Mathf.Min(q1.x, q2.x) &&
                p.z <= Mathf.Max(q1.z, q2.z) && p.z >= Mathf.Min(q1.z, q2.z));
    }


    static Vector3 FindIntersection(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2)
    {
        Vector3 intersection = new();

        float t = (q1.z - p1.z) / (q1.x - p1.x);
        float s = (q2.z - p2.z) / (q2.x - p2.x);

        float calculatedX = (t * p1.x - p1.z - s * p2.x + p2.z) / (t - s);
        float calculatedZ = t * (calculatedX - p1.x) + p1.z;
        intersection.x = calculatedX;
        intersection.y = p1.y;
        intersection.z = calculatedZ;

        return intersection;
    }


    public static bool IsClockwise(List<Vector3> polygon)
    {
        int n = polygon.Count;
        float area = 0;

        for (int i = 0; i < n; i++)
        {
            int next = (i + 1) % n;
            area += (polygon[next].x - polygon[i].x) * (polygon[next].z + polygon[i].z);
        }

        return area < 0;
    }

}
