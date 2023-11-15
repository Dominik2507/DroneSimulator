using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class SurvailanceManager
{
    public string curveName;
    public int numberOfDrones = 0;
    public List<Drone> drones = new();

    public List<Vector3> points;
    public List<Vector3> navigationCurve = new();
    

    public bool isRunning = false;
    public LineRenderer navigationCurveRenderer = null;
    private GameObject curveRemoveButton;

    public SurvailanceManager(List<Vector3> p, string name)
    {
        points = new(p);
        curveName = name;
    }

    public virtual void Destroy()
    {
        isRunning = false;
        Object.Destroy(navigationCurveRenderer.gameObject);
        Object.Destroy(curveRemoveButton);

        foreach(Drone drone in drones)
        {
            Object.Destroy(drone.gameObject);
        }
    }

    public virtual void drawCurve()
    {
        GameObject rendererHolder = new();
        rendererHolder.transform.parent = SimulationManager.instance.gameObject.transform;
        curveRemoveButton = Object.Instantiate(UIManager.instance.curveRemoveButtonPrefab);
        curveRemoveButton.transform.SetParent(UIManager.instance.curveButtonPanel.transform);
        curveRemoveButton.GetComponent<Button>().onClick.AddListener(() => { SimulationManager.instance.RemoveSurvailanceManager(this); });
        curveRemoveButton.GetComponentInChildren<TextMeshProUGUI>().SetText(curveName);

        navigationCurveRenderer = rendererHolder.AddComponent<LineRenderer>();
        navigationCurveRenderer.positionCount = navigationCurve.Count;
        navigationCurveRenderer.startWidth = 0.1f; // Set the width of the line
        navigationCurveRenderer.endWidth = 0.1f; // Set the width of the line
        navigationCurveRenderer.startColor = Color.blue;
        navigationCurveRenderer.endColor = Color.blue;
        // Set the line points
        navigationCurveRenderer.SetPositions(navigationCurve.ToArray());
        
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

    public LineSurvailance(List<Vector3> p, string name) : base(p, name) {
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

    public PolygonLineSurvailance(List<Vector3> p, string name) : base(p, name)
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

    public GameObject areaLineDrawer = null;
 
    public AreaSurvailance(List<Vector3> p, string name) : base(p, name)
    {
        if (Utils.IsClockwise(p)) { p.Reverse(); }
        algorithm = new OutwardToCenter();
        Utils.RecursiveSplit(points, polygons);
        GenerateNavigationCurve();
    }

    public AreaSurvailance(List<Vector3> p, NavigationAlgorithm alg, string name) : base(p, name)
    {
        algorithm = alg;
        Utils.RecursiveSplit(points, polygons);
        GenerateNavigationCurve();
    }

    public override void Destroy()
    {
        Object.Destroy(areaLineDrawer);
        base.Destroy();
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

    public virtual void drawArea()
    {
        List<Vector3> copyPoints = new(points);
        //copyPoints.Add(points[0]);

        copyPoints.ForEach(point => point.y -= 0.5f);

        areaLineDrawer = new("EmptyChildObject " + SimulationManager.instance.gameObject.transform.childCount);
        areaLineDrawer.transform.parent = SimulationManager.instance.gameObject.transform;

        LineRenderer lineRenderer = areaLineDrawer.AddComponent<LineRenderer>();
        lineRenderer.positionCount = copyPoints.Count;
        lineRenderer.startWidth = 0.5f; // Set the width of the line
        lineRenderer.endWidth = 0.5f; // Set the width of the line

        // Set the line points
        lineRenderer.startColor = Color.Lerp(Color.blue, Color.red, Random.Range(0, 1));
        lineRenderer.endColor = Color.Lerp(Color.blue, Color.red, Random.Range(0, 1));
        lineRenderer.material.color = Color.Lerp(Color.blue, Color.red, Random.Range(0, 1));
        lineRenderer.SetPositions(copyPoints.ToArray());
    }
}



public class RectangleAreaSurvailance : AreaSurvailance
{
   
    public RectangleAreaSurvailance(List<Vector3> p, string name) : base(p, name)
    {
        if(p.Count != 4)
        {
            throw new System.Exception("Rectangle should only be instaciated with 4 Points");
        }

        GenerateNavigationCurve();
    }

    public RectangleAreaSurvailance(List<Vector3> p, NavigationAlgorithm alg, string name) : base(p, alg, name)
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
