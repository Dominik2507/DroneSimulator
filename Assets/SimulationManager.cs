using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public GameObject dronePrefab;

    public List<SurvailanceManager> areas = new();

    public static SimulationManager instance = null;

    public GameObject areaDrawer;

    private void Start()
    {

        if (instance is null) instance = this;

    }
    public void example1()
    {
        List<Vector3> examplePoints = new();


        examplePoints.Add(new Vector3(0, 15, 10));
        examplePoints.Add(new Vector3(20, 15, 10));

        LineSurvailance ls = new(examplePoints);

        ls.addDrone(false);
        
        areas.Add(ls);

        StartSimulation();
    }

    public void example2()
    {
        List<Vector3> examplePoints = new();


        examplePoints.Add(new Vector3(0, 20, 10));
        examplePoints.Add(new Vector3(20, 20, 10));
        examplePoints.Add(new Vector3(10, 20, 0));

        PolygonLineSurvailance ps = new(examplePoints);

        ps.addDrone(false);

        areas.Add(ps);

        StartSimulation();
    }

    public void example3()
    {
        List<Vector3> examplePoints = new();
        OutwardToCenter algorithm = new();

        examplePoints.Add(new Vector3(-10, 15,  10));
        examplePoints.Add(new Vector3( 10, 15, -10));
        examplePoints.Add(new Vector3(-10, 15, -10));
        examplePoints.Add(new Vector3( 10, 15,  10));

        AreaSurvailance ps = new(examplePoints, algorithm);

        ps.addDrone(false);

        areas.Add(ps);

        StartSimulation();
    }

    public void Example4()
    {
        //HEXAGON

        List<Vector3> examplePoints = new();
        float radius = 5f;
        
        for (int i = 0; i < 6; i++)
        {
            float angle = 2 * Mathf.PI / 6 * i;
            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);
            examplePoints.Add(new Vector3(x, 15, y));
        }

        OutwardToCenter algorithm = new();
        AreaSurvailance ps = new(examplePoints, algorithm);

        ps.addDrone(false);

        areas.Add(ps);

        StartSimulation();
    }
    public void Example5()
    {
        areas.Clear();
        List<Vector3> examplePoints = new();
        
        for (int i = 0; i < 7; i++)
        {
            float x = Random.Range(-25f, 25f);
            float y = Random.Range(-25f, 25f);
            examplePoints.Add(new Vector3(x, 15, y));
        }

        OutwardToCenter algorithm = new();
        AreaSurvailance ps = new(examplePoints, algorithm);

        ps.addDrone(false);

        areas.Add(ps);

        StartSimulation();
    }

    public void Example6()
    {
        List<Vector3> examplePoints = new();
        float radius = 10f;

        for (int i = 0; i < 8; i++)
        {
            float angle = 2 * Mathf.PI / 6 * i;
            float x = radius * Mathf.Cos(angle) + Random.Range(-5, 5);
            float y = radius * Mathf.Sin(angle) + Random.Range(-5, 5);
            examplePoints.Add(new Vector3(x, 15, y));
        }

        OutwardToCenter algorithm = new();
        AreaSurvailance ps = new(examplePoints, algorithm);

        ps.addDrone(false);

        areas.Add(ps);

        StartSimulation();
    }

    public void StartSimulation()
    {
        foreach(SurvailanceManager sm in areas)
        {
            sm.startSimulation();
            sm.drawCurve();
            if(sm is AreaSurvailance sArea)
            {
                sArea.drawArea(true);
            }
        }
    }

    public void AddDrone()
    {
        foreach (SurvailanceManager sm in areas)
        {
            sm.addDrone();
        }
    }

    public void RemoveDrone()
    {
        foreach (SurvailanceManager sm in areas)
        {
            sm.removeDrone();
        }
    }
}

