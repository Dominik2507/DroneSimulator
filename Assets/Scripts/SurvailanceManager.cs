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
    public GameObject rendererHolder;
    private GameObject curveUIprefab;

    public SurvailanceManager(List<Vector3> p, string name)
    {
        points = new(p);
        curveName = name;
    }

    public virtual void Destroy()
    {
        isRunning = false;
        Object.Destroy(navigationCurveRenderer.gameObject);
        Object.Destroy(curveUIprefab);

        foreach(Drone drone in drones)
        {
            Object.Destroy(drone.gameObject);
        }
    }


    public virtual void drawCurve()
    {
        if (rendererHolder)
        {
            Object.Destroy(rendererHolder);
            Object.Destroy(curveUIprefab);
        }
        rendererHolder = new();
        rendererHolder.transform.parent = SimulationManager.instance.gameObject.transform;
        curveUIprefab = Object.Instantiate(UIManager.instance.curveUIPrefab);
        curveUIprefab.transform.SetParent(UIManager.instance.curveButtonPanel.transform);
        //curveUIprefab.GetComponentInChildren<Button>().onClick.AddListener(() => { SimulationManager.instance.RemoveSurvailanceManager(this); });
        curveUIprefab.GetComponentInChildren<TextMeshProUGUI>().SetText(curveName);

        curveUIprefab.GetComponentInChildren<TMP_InputField>()?.onSubmit.AddListener(onDroneNumberChanged);


        List<Button> buttons = new(curveUIprefab.GetComponentsInChildren<Button>());

        buttons.Find(button => button.name == "AddDrone")?.onClick.AddListener(addDrone);
        buttons.Find(button => button.name == "RemoveDrone")?.onClick.AddListener(removeDrone);
        buttons.Find(button => button.name == "RemoveButton")?.onClick.AddListener(() => { SimulationManager.instance.RemoveSurvailanceManager(this); });
        buttons.Find(button => button.name == "ViewButton")?.onClick.AddListener(() => { 
            SimulationManager.instance.currentGrid = this;
            UIManager.instance.switchToGridView();
            SimulationManager.instance.GetComponent<CameraSwitcher>().SwitchtoGridView(); 
        });
        
        //curveUIprefab.GetComponentInChildren<TextMeshProUGUI>().gameObject.GetComponent<Button>()?.onClick.AddListener(() => { SimulationManager.instance.currentGrid = this; });
        
        navigationCurveRenderer = rendererHolder.AddComponent<LineRenderer>();
        navigationCurveRenderer.positionCount = navigationCurve.Count;
        navigationCurveRenderer.startWidth = 0.5f; // Set the width of the line
        navigationCurveRenderer.endWidth = 0.5f; // Set the width of the line
        navigationCurveRenderer.startColor = Color.blue;
        navigationCurveRenderer.endColor = Color.blue;
        // Set the line points
        navigationCurveRenderer.SetPositions(navigationCurve.ToArray());
        
    }

    public bool droneNumberValidator(string input)
    {
        if (int.TryParse(input, out int number))
        {
            // Check if the number is greater than or equal to 1
            return number >= 1;
        }
        else
        {
            // Input is not a valid integer
            return false;
        }
    }


    public void onDroneNumberChanged(string newNumber)
    {
        
        if (int.TryParse(newNumber, out int droneNumber))
        {
            if(droneNumber >= 1) numberOfDrones = droneNumber;
            else
            {
                numberOfDrones = 1;
                TMP_InputField inputField = curveUIprefab?.GetComponentInChildren<TMP_InputField>();
                if (inputField != null)
                {
                    inputField.SetTextWithoutNotify(numberOfDrones.ToString());
                }

            }

            if (SimulationManager.instance.mode == SceneMode.runningSimulation)
            {
                startSimulation();

                if (SimulationManager.instance.isViewingGrid)
                {
                    AdjustCameraGrid(true);
                }
            }
        }
        else
        {
            Debug.Log("INPUT TYPE ERROR");
        }
    }

    public virtual void addDrone()
    {
        numberOfDrones++;

        TMP_InputField inputField = curveUIprefab?.GetComponentInChildren<TMP_InputField>();
        if (inputField != null)
        {
            inputField.SetTextWithoutNotify(numberOfDrones.ToString());
        }
        else
        {
            Debug.LogError("TMP_InputField component not found!");
        }

        if (SimulationManager.instance.mode == SceneMode.runningSimulation)
        {
            startSimulation();

            if (SimulationManager.instance.isViewingGrid)
            {
                AdjustCameraGrid(true);
            }
        }
    }

    public virtual void removeDrone()
    {
        numberOfDrones--;
        if (numberOfDrones < 1) numberOfDrones = 1;
        TMP_InputField inputField = curveUIprefab?.GetComponentInChildren<TMP_InputField>();
        if (inputField != null)
        {
            inputField.SetTextWithoutNotify(numberOfDrones.ToString());
        }
        else
        {
            Debug.LogError("TMP_InputField component not found!");
        }

        if (SimulationManager.instance.mode == SceneMode.runningSimulation)
        {
            startSimulation();

            if (SimulationManager.instance.isViewingGrid)
            {
                AdjustCameraGrid(true);
            }
        }
    }

    public virtual void addDrone(bool restart)
    {
        numberOfDrones++;
        TMP_InputField inputField = curveUIprefab?.GetComponentInChildren<TMP_InputField>();
        if (inputField != null)
        {
            inputField.SetTextWithoutNotify(numberOfDrones.ToString());
        }
        else
        {
            Debug.LogError("TMP_InputField component not found!");
        }

        if (restart)
        {
            startSimulation();

            if (SimulationManager.instance.isViewingGrid)
            {
                AdjustCameraGrid(true);
            }
        }

    }

    public virtual void removeDrone(bool restart)
    {
        numberOfDrones--;
        TMP_InputField inputField = curveUIprefab?.GetComponentInChildren<TMP_InputField>();
        if (inputField != null)
        {
            inputField.SetTextWithoutNotify(numberOfDrones.ToString());
        }
        else
        {
            Debug.LogError("TMP_InputField component not found!");
        }

        if (restart)
        {
            startSimulation();

            if (SimulationManager.instance.isViewingGrid)
            {
                AdjustCameraGrid(true);
            }
        }
    }

    public void AdjustCameraGrid(bool activateCameras = false)
    {
        if (drones.Count == 1)
        {
            if (activateCameras)
            {
                List<Camera> cameras = new List<Camera>(drones[0].GetComponentsInChildren<Camera>());
                Camera viewCamera = cameras.Find((Camera cm) => cm.gameObject.name == "ViewingCamera");
                if (viewCamera)
                {
                    if (activateCameras) viewCamera.enabled = true;
                }
            }
            return;
        }
        int cols = 2;
        int rows = 1;

        // 2x2 grid
        if (numberOfDrones < 5) { rows = 2; }
        // 3x3 grid
        else if (numberOfDrones < 10) { cols = 3; rows = 3; }
        // 4x4 grid
        else if (numberOfDrones < 17) { cols = 4; rows = 4; }
        // 4x4 slider
        else
        {
            // handle all UI things and whatever else
            return;
        }

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int index = row * cols + col;
                Debug.Log(row + ", " + col + " (" + rows + "," + cols + ")");
                if (index >= drones.Count) return;
                List<Camera> cameras = new List<Camera>(drones[index].GetComponentsInChildren<Camera>());
                Camera viewCamera = cameras.Find((Camera cm) => cm.gameObject.name == "ViewingCamera");
                if (viewCamera)
                {
                    if(activateCameras) viewCamera.enabled = true;
                    Debug.Log("Found drone's camera");
                    // Create a new Rect with the specified parameters
                    Rect newRect = new Rect(1.0f * col / cols, 1 - 1.0f * (row + 1) / rows, 1.0f / cols, 1.0f / rows);

                    // Assign the new rect to the camera's rect property
                    viewCamera.rect = newRect;
                }
            }
        }
    }

    public void DisableCameraGrid()
    {
        List<Camera> cameras = new();
        drones.ForEach((Drone drone) => {
            List<Camera> camerasArray = new List<Camera>(drone.GetComponentsInChildren<Camera>());
            Camera viewCamera = camerasArray.Find((Camera cm) => cm.gameObject.name == "ViewingCamera");
            if (viewCamera) cameras.Add(viewCamera);
        });

        cameras.ForEach((Camera cam) =>
        {
            cam.enabled = false;
        }
        );
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
            // //Debug.Log(index + " " + sp.ToString());

            GameObject newDroneObject = Object.Instantiate(SimulationManager.instance.dronePrefab, sp, Quaternion.identity);
            Drone newDrone = newDroneObject.GetComponent<Drone>();
            newDrone.manager = this;
            drones.Add(newDrone);

            newDrone.startPosition = sp;
            newDrone.ResetPosition();
            newDrone.movingTowardsIndex = (index + 1) % navigationCurve.Count;
        }

        AdjustCameraGrid(SimulationManager.instance.isViewingGrid);
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
        if (Utils.IsClockwise(points)) { points.Reverse(); }
        algorithm = new OutwardToCenter();
        Utils.RecursiveSplit(points, polygons);
        GenerateNavigationCurve();
    }

    public AreaSurvailance(List<Vector3> p, NavigationAlgorithm alg, string name) : base(p, name)
    {
        if (Utils.IsClockwise(points)) { points.Reverse(); }
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
        navigationCurve = new();
        if(algorithm is null)
        {
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
        copyPoints.Add(points[0]);

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
