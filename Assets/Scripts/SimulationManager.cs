using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SceneMode
{
    freeDraw, presetDraw, runningSimulation, none
}

public class SimulationManager : MonoBehaviour
{
    public GameObject dronePrefab;
 
    public SceneMode mode = SceneMode.freeDraw;

    public List<SurvailanceManager> areas = new();

    public static SimulationManager instance = null;
    public bool useTriangleCollapsing = false;
    public GameObject areaDrawer;

    public SurvailanceManager currentGrid;

    private bool isRunningSimulations = false;
    public bool isViewingGrid = false;
    private SceneMode modeBeforeSimulation = SceneMode.freeDraw;


    public float simSpeed = 1;
    public int FOV = 60;
    public float verticalOffset = 4;
    private void Start()
    {

        if (instance is null) instance = this;

    }
    public void example1()
    {

        List<Vector3> examplePoints = new();


        examplePoints.Add(new Vector3(-10, 1, 0));
        examplePoints.Add(new Vector3(10, 1, 0));

        LineSurvailance ls = new(examplePoints, "Segmented line");

        ls.addDrone(false);
        
        AddArea(ls);
    }

    public void example2()
    {
        List<Vector3> examplePoints = new();


        examplePoints.Add(new Vector3(0, 1, 10));
        examplePoints.Add(new Vector3(20, 1, 10));
        examplePoints.Add(new Vector3(10, 1, 0));

        PolygonLineSurvailance ps = new(examplePoints, "Cycle from lines");

        ps.addDrone(false);

        AddArea(ps);
    }

    public void example3()
    {
        List<Vector3> examplePoints = new();
        OutwardToCenter algorithm = new();

        examplePoints.Add(new Vector3(-10, 1,  10));
        examplePoints.Add(new Vector3( 10, 1, -10));
        examplePoints.Add(new Vector3(-10, 1, -10));
        examplePoints.Add(new Vector3( 10, 1,  10));

        AreaSurvailance ps = new(examplePoints, algorithm, "Area - Hourglass");

        ps.addDrone(false);

        AddArea(ps);
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
            examplePoints.Add(new Vector3(x, 1, y));
        }

        OutwardToCenter algorithm = new();
        AreaSurvailance ps = new(examplePoints, algorithm, "Area - Hexagon");

        ps.addDrone(false);

        AddArea(ps);

    }
    public void Example5()
    {
        areas.Clear();
        List<Vector3> examplePoints = new();
        
        for (int i = 0; i < 7; i++)
        {
            float x = Random.Range(-25f, 25f);
            float y = Random.Range(-25f, 25f);
            examplePoints.Add(new Vector3(x, 1, y));
        }

        OutwardToCenter algorithm = new();
        AreaSurvailance ps = new(examplePoints, algorithm, "Area - random 7 dots");

        ps.addDrone(false);

        AddArea(ps);
    }

    public void Example6()
    {
        List<Vector3> examplePoints = new();
        float radius = 10f;

        for (int i = 0; i < 8; i++)
        {
            float angle = 2 * Mathf.PI / 8 * i;
            float x = radius * Mathf.Cos(angle) + Random.Range(-3, 3);
            float y = radius * Mathf.Sin(angle) + Random.Range(-3, 3);
            examplePoints.Add(new Vector3(x, 1, y));
        }

        OutwardToCenter algorithm = new();
        AreaSurvailance ps = new(examplePoints, algorithm, "Area - Octagon with noise");

        ps.addDrone(false);

        AddArea(ps);
    }

    public void StartSimulation()
    {
        modeBeforeSimulation = mode;
        mode = SceneMode.runningSimulation;

        currentGrid = areas[0];

        foreach(SurvailanceManager sm in areas)
        {
            if(sm.navigationCurveRenderer is null) sm.drawCurve();
            if(sm is AreaSurvailance sArea)
            {
                if (sArea.areaLineDrawer is null) sArea.drawArea();
            }
            sm.startSimulation();
        }

        isRunningSimulations = true;
    }

    public void EndSimulation()
    {
        foreach (SurvailanceManager sm in areas)
        {
            sm.endSimulation();
        }

        mode = modeBeforeSimulation;
        //Debug.Log(mode);
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

    public void RemoveSurvailanceManager(SurvailanceManager s)
    {
        s.Destroy();
        areas.Remove(s);
        if(areas.Count < 1)
        {
            EndSimulation();
            UIManager.instance.disableStartSimulation();
        }
    }

    public void AddArea(SurvailanceManager s)
    {
        areas.Add(s);
        s.drawCurve();
        UIManager.instance.enableStartSimulation();
    }

    public void CloseApplication()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                    Application.Quit();
        #endif
    }

    public void SetSimSpeed(float value)
    {
        simSpeed = value;
    }

    public void SetDroneFOV(int value)
    {
        FOV = value;
        onDroneSettingsChange();
    }

    public void SetDroneOffset(float value)
    {
        verticalOffset = value;
        onDroneSettingsChange();
    }

    public void onDroneSettingsChange() {

        EndSimulation();

        foreach(SurvailanceManager area in areas)
        {
            area.GenerateNavigationCurve();
            area.drawCurve();
        }

        StartSimulation();
    }
}

