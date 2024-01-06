using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FreeDrawScript : MonoBehaviour
{

    public GameObject pointPrefab;
    public LineRenderer navigationCurveRenderer = null;
    private List<GameObject> addedPoints = new();

    bool isDraggingPoint = false;
    GameObject draggedPoint = null;



    void Start()
    {
        GameObject rendererHolder = new();
        rendererHolder.transform.parent = SimulationManager.instance.gameObject.transform;
        navigationCurveRenderer = rendererHolder.AddComponent<LineRenderer>();
        navigationCurveRenderer.SetVertexCount(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (SimulationManager.instance.mode != SceneMode.freeDraw) return;

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // Check for mouse click
            if (Input.GetMouseButtonDown(0))
            {
                // Raycast from the mouse position to the world
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if(hit.collider.gameObject.layer == 3)
                    {
                        isDraggingPoint = true;
                        draggedPoint = hit.collider.gameObject;
                    }
                    else if (hit.collider.gameObject.layer == 6)
                    {
                        // Instantiate a point at the hit position
                        Vector3 pointPos = hit.point;
                        pointPos.y = 1;
                        GameObject newPoint = Instantiate(pointPrefab, pointPos, Quaternion.identity);
                        addedPoints.Add(newPoint);
                    }
                }

            }
            if (isDraggingPoint && draggedPoint != null)
            {
                // Raycast from the mouse position to the world
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Vector3 pointPos = hit.point;
                    pointPos.y = 1;
                    draggedPoint.transform.position = pointPos;
                }
            }

            if (isDraggingPoint && draggedPoint != null && Input.GetMouseButtonUp(0))
            {
                // Raycast from the mouse position to the world
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Vector3 pointPos = hit.point;
                    pointPos.y = 1;
                    draggedPoint.transform.position = pointPos;
                    isDraggingPoint = false;
                    draggedPoint = null;
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                // Raycast from the mouse position to the world
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if(hit.collider.gameObject.layer == 3)
                    {
                        GameObject deletePoint = hit.collider.gameObject;
                        addedPoints.Remove(deletePoint);
                        Destroy(deletePoint);
                    }
                }

            }
        }

        if (addedPoints.Count > 0) previewCurve();
    }

    public void EndDrawAndGenerateCurve()
    {
        OutwardToCenter algorithm = new();

        AreaSurvailance ps = new(addedPoints.ConvertAll<Vector3>((GameObject point) => { return point.transform.position; }), algorithm, "Free drawn algorithm");

        foreach(GameObject go in addedPoints)
        {
            Destroy(go);
        }
        addedPoints = new();

        Destroy(navigationCurveRenderer.gameObject);

        GameObject rendererHolder = new();
        rendererHolder.transform.parent = SimulationManager.instance.gameObject.transform;
        navigationCurveRenderer = rendererHolder.AddComponent<LineRenderer>();
        navigationCurveRenderer.SetVertexCount(0);
        ps.addDrone(false);

        SimulationManager.instance.AddArea(ps);

        //navigationCurveRenderer = rendererHolder.AddComponent<LineRenderer>();

    }

    public virtual void previewCurve()
    {
  
        navigationCurveRenderer.positionCount = addedPoints.Count;
        navigationCurveRenderer.startWidth = 0.5f; // Set the width of the line
        navigationCurveRenderer.endWidth = 0.5f; // Set the width of the line
        navigationCurveRenderer.startColor = Color.blue;
        navigationCurveRenderer.endColor = Color.blue;
        // Set the line points
        List<Vector3> points = addedPoints.ConvertAll<Vector3>((GameObject point) => { return point.transform.position; });
        points.Add(points[0]);
        navigationCurveRenderer.SetPositions(points.ToArray());

    }
}
