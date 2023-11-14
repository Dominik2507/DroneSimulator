using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SurvailanceManager
{
    public int numberOfDrones = 0;
    public List<Drone> drones = new();

    public List<Vector3> points;
    public List<Vector3> navigationCurve = new();

    public bool isRunning = false;

    public SurvailanceManager(List<Vector3> p)
    {
        points = new(p);
    }

    public virtual void drawCurve()
    {
        LineRenderer lineRenderer = SimulationManager.instance.gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = navigationCurve.Count;
        lineRenderer.startWidth = 0.1f; // Set the width of the line
        lineRenderer.endWidth = 0.1f; // Set the width of the line
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.blue;
        // Set the line points
        lineRenderer.SetPositions(navigationCurve.ToArray());
    }

    public virtual void addDrone(bool restart = true)
    {
        numberOfDrones++;
        //GameObject newDroneObject = Object.Instantiate(SimulationManager.instance.dronePrefab);
        //Drone newDrone = newDroneObject.GetComponent<Drone>();
        //newDrone.manager = this;
        //drones.Add(newDrone);
        if (restart) startSimulation();
    }

    public virtual void removeDrone(bool restart = true)
    {
        numberOfDrones--;
        //Drone removedDrone = drones[0];
        //drones.RemoveAt(0);
        //Object.Destroy(removedDrone);

        if (restart) startSimulation();
    }

    public virtual void startSimulation()
    {
        if(numberOfDrones == 0 || navigationCurve.Count == 0)
        {
            return;
        }
        if (isRunning) endSimulation();

        for (int i = 0; i < numberOfDrones; i++)
        {
            int index = Mathf.RoundToInt(1.0f * i * navigationCurve.Count / numberOfDrones);
            index %= navigationCurve.Count;

            Vector3 sp = navigationCurve[index];
            // Debug.Log(index + " " + sp.ToString());

            GameObject newDroneObject = Object.Instantiate(SimulationManager.instance.dronePrefab, sp, Quaternion.identity);
            Drone newDrone = newDroneObject.GetComponent<Drone>();
            newDrone.manager = this;
            drones.Add(newDrone);

            newDrone.startPosition = sp;
            newDrone.ResetPosition();
            newDrone.movingTowardsIndex = (index + 1) % navigationCurve.Count;
        }
        isRunning = true;
    }

    public virtual void endSimulation()
    {
        isRunning = false;

        // Create a separate list to store drones to remove
        List<Drone> dronesToRemove = new List<Drone>();

        // Add drones to the removal list
        foreach (Drone d in drones)
        {
            dronesToRemove.Add(d);
        }

        // Remove drones and destroy them from the separate list
        foreach (Drone d in dronesToRemove)
        {
            drones.Remove(d);
            Object.Destroy(d.transform.gameObject);
        }
    }


    public abstract void GenerateNavigationCurve();

}

public class LineSurvailance : SurvailanceManager
{

    public LineSurvailance(List<Vector3> p) : base(p) {
        GenerateNavigationCurve();
    }
    
    public override void GenerateNavigationCurve()
    {
        navigationCurve.AddRange(points);
        List<Vector3> copyPoints = new(points);
        copyPoints.Reverse();
        navigationCurve.AddRange(copyPoints);
    }
}

public class PolygonLineSurvailance : SurvailanceManager
{

    public PolygonLineSurvailance(List<Vector3> p) : base(p)
    {
        GenerateNavigationCurve();
    }

    public override void GenerateNavigationCurve()
    {
        navigationCurve.AddRange(points);
   
        navigationCurve.Add(points[0]);
        
    }

}

public class AreaSurvailance : SurvailanceManager
{
    public NavigationAlgorithm algorithm = null;

    public List<List<Vector3>> polygons = new();
 
    public AreaSurvailance(List<Vector3> p) : base(p)
    {
        algorithm = new OutwardToCenter();
        Utils.RecursiveSplit(points, polygons);
        GenerateNavigationCurve();
    }

    public AreaSurvailance(List<Vector3> p, NavigationAlgorithm alg) : base(p)
    {
        algorithm = alg;
        Utils.RecursiveSplit(points, polygons);
        GenerateNavigationCurve();
    }

    public override void GenerateNavigationCurve()
    {
        if(algorithm is null)
        {
            Debug.Log("No area algorith given");
            navigationCurve.AddRange(points);
            navigationCurve.Add(points[0]);
            return;
        }

        foreach (var polygon in polygons)
        {
            algorithm.generateCurve(this, polygon);
        }

    }

    public virtual void drawArea(bool OnlyCircumfence = false)
    {

        
        List<Vector3> copyPoints = new(points);
        copyPoints.Add(points[0]);

        copyPoints.ForEach(point => point.y -= 0.5f);

        GameObject emptyObject = new("EmptyChildObject " + SimulationManager.instance.gameObject.transform.childCount);
        emptyObject.transform.parent = SimulationManager.instance.gameObject.transform;

        LineRenderer lineRenderer = emptyObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = copyPoints.Count;
        lineRenderer.startWidth = 0.5f; // Set the width of the line
        lineRenderer.endWidth = 0.5f; // Set the width of the line

        // Set the line points
        lineRenderer.startColor = Color.Lerp(Color.blue, Color.red, Random.Range(0, 1));
        lineRenderer.endColor = Color.Lerp(Color.blue, Color.red, Random.Range(0, 1));
        lineRenderer.material.color = Color.Lerp(Color.blue, Color.red, Random.Range(0, 1));
        lineRenderer.SetPositions(copyPoints.ToArray());

        
        if (OnlyCircumfence) return;

        Mesh mesh = new Mesh();
        SimulationManager.instance.areaDrawer.GetComponent<MeshFilter>().mesh = mesh;

        

        Vector3[] vertices = copyPoints.ToArray();
        int[] triangles = new int[(vertices.Length - 2) * 3];

        // Ensure vertices are relative to the GameObject's position
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] -= SimulationManager.instance.areaDrawer.transform.position;
        }

        int vertexIndex = 0;

        for (int i = 1; i < vertices.Length - 1; i++)
        {
            triangles[vertexIndex] = 0;
            triangles[vertexIndex + 1] = i;
            triangles[vertexIndex + 2] = i + 1;
            vertexIndex += 3;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

       //SimulationManager.instance.areaDrawer.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
    }
}



public class RectangleAreaSurvailance : AreaSurvailance
{
   
    public RectangleAreaSurvailance(List<Vector3> p) : base(p)
    {
        if(p.Count != 4)
        {
            throw new System.Exception("Rectangle should only be instaciated with 4 Points");
        }

        GenerateNavigationCurve();
    }

    public RectangleAreaSurvailance(List<Vector3> p, NavigationAlgorithm alg) : base(p, alg)
    {
        if (p.Count != 4)
        {
            throw new System.Exception("Rectangle should only be instaciated with 4 Points");
        }
        GenerateNavigationCurve();
    }

    public override void GenerateNavigationCurve()
    {
        if (algorithm is null) return;
        algorithm.generateCurve(this);
    }
} 
