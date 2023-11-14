using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NavigationAlgorithm
{
    public abstract void generateCurve(SurvailanceManager manager, List<Vector3> polygon = null);
}

public class CenterToOutward : NavigationAlgorithm
{
    public override void generateCurve(SurvailanceManager manager, List<Vector3> polygon = null)
    {
        OutwardToCenter cc = new();
        cc.generateCurve(manager);

        manager.navigationCurve.Reverse();
    }
}

public class OutwardToCenter : NavigationAlgorithm
{
    public override void generateCurve(SurvailanceManager manager, List<Vector3> polygon = null)
    {
        if (manager is AreaSurvailance)
        {

            if (manager is RectangleAreaSurvailance)
            {
                generateRectangleCurve((RectangleAreaSurvailance)manager, polygon);
            }
            else
            {
                generateFreeFormCurve((AreaSurvailance)manager, polygon);
            }
        }
        else
        {
            //LINE ALGORITHMS
        }
    }

    public void generateRectangleCurve(RectangleAreaSurvailance manager, List<Vector3> polygon = null)
    {
        Drone d = SimulationManager.instance.dronePrefab.GetComponent<Drone>();
        float visible_width = 2 * d.vertical_offset * Mathf.Tan(Mathf.PI * d.GetComponentInChildren<Camera>().fieldOfView / 180);
        Debug.Log(visible_width);


        List<Vector3> navigationCurve = manager.navigationCurve;
        List<Vector3> points = new(manager.points);

        navigationCurve.AddRange(points);
        navigationCurve.Add(points[0]);

        float a = Vector3.Distance(points[0], points[1]);
        float b = Vector3.Distance(points[1], points[2]);

        while (Mathf.Max(a, b) > visible_width)
        {
            points[0] += (Vector3.back + Vector3.right) * visible_width;
            points[1] += (Vector3.forward + Vector3.right) * visible_width;
            points[2] += (Vector3.forward + Vector3.left) * visible_width;
            points[3] += (Vector3.back + Vector3.left) * visible_width;

            navigationCurve.AddRange(points);
            navigationCurve.Add(points[0]);
            a = Vector3.Distance(points[0], points[1]);
            b = Vector3.Distance(points[1], points[2]);
        }


    }
    public void generateFreeFormCurve(AreaSurvailance manager, List<Vector3> polygon = null)
    {
        Drone d = SimulationManager.instance.dronePrefab.GetComponent<Drone>();
        float visible_width = 2 * d.vertical_offset * Mathf.Tan(Mathf.PI * d.GetComponentInChildren<Camera>().fieldOfView / 180);
        // Debug.Log(visible_width);

        List<Vector3> points;
        if(polygon == null)
        {
            points = new(manager.points);
        }
        else
        {
            if(manager.navigationCurve.Count == 0 && manager.polygons.Count > 1)
            {
                manager.navigationCurve.AddRange(manager.points);
                manager.navigationCurve.Add(manager.points[0]);

            }
            points = new(polygon);
        }

        List<Vector3> navigationCurve = manager.navigationCurve;

        navigationCurve.AddRange(points);
        navigationCurve.Add(points[0]);

        List<Vector3> currentPolygon = new(points);

        int max_iterations = 100;
        int iteration = 0;
        while (iteration < max_iterations)
        {
            iteration++;
            List<Vector3> temp = new();


            bool removedPoints;
            do
            {
                removedPoints = false;
                List<Vector3> filteredPolygon = new(currentPolygon);
                if (currentPolygon.Count < 3) break;
                for (int i = 0; i < currentPolygon.Count - 1; i++)
                {

                    Vector3 previousPoint = currentPolygon[(i - 1 + currentPolygon.Count) % currentPolygon.Count];
                    Vector3 currentPoint = currentPolygon[i];
                    Vector3 nextPoint = currentPolygon[(i + 1) % currentPolygon.Count];

                    Vector3? approximatedTriangle = Utils.GetMiddlePointIfSmallArea(previousPoint, currentPoint, nextPoint, Mathf.Pow(visible_width / 2, 2) * Mathf.PI);
                    if (approximatedTriangle is not null && filteredPolygon.Count > 3)
                    {
                        Debug.Log("Aproximated triangle as: " + ((Vector3)approximatedTriangle).x + " " + ((Vector3)approximatedTriangle).y + " " + ((Vector3)approximatedTriangle).z);
                        
                        filteredPolygon[(i - 1 + filteredPolygon.Count) % filteredPolygon.Count] = (Vector3)approximatedTriangle;
                        filteredPolygon.Remove(currentPoint);
                        filteredPolygon.Remove(nextPoint);

                        removedPoints = true;
                        break;
                    }
                    else if (approximatedTriangle is not null && filteredPolygon.Count == 3)
                    {
                        filteredPolygon.Clear();
                        filteredPolygon.Add((Vector3)approximatedTriangle);
                    }

                }
                currentPolygon = new(filteredPolygon);

            } while (removedPoints);

            if (currentPolygon.Count < 3)
            {
                navigationCurve.AddRange(currentPolygon);
                break;
            }

            for (int i = 0; i < currentPolygon.Count; i++)
            {

                Vector3 previousPoint = currentPolygon[(i - 1 + currentPolygon.Count) % currentPolygon.Count];
                Vector3 currentPoint = currentPolygon[i];
                Vector3 nextPoint = currentPolygon[(i + 1) % currentPolygon.Count];

                Vector3 moveDirection = Utils.CalculateHalfAngleVector(currentPoint, previousPoint, nextPoint);

                Vector3 addedPoint = currentPoint + moveDirection * visible_width;

                if (Utils.IsPointInPolygon(currentPolygon, addedPoint)) temp.Add(addedPoint);

            }

            if (temp.Count == 0) break;

            navigationCurve.AddRange(temp);
            navigationCurve.Add(temp[0]);
            currentPolygon = new(temp);

            Vector3? polygonCenter = Utils.GetMiddlePointIfWithinSquare(currentPolygon, visible_width);
            if (polygonCenter is not null)
            {
                navigationCurve.Add((Vector3)polygonCenter);
                break;
            }
        }
        
        navigationCurve.Add(points[0]);

    }

}