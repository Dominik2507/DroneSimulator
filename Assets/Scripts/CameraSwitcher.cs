using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera ortograficTopDownCamera;
    public Camera perspectiveCamera;

    public int currentViewingAreaIndex = 0;

    void Start()
    {
        // Disable all cameras except the first one initially
        EnableCamera(ortograficTopDownCamera);
        DisableCamera(perspectiveCamera);
    }

    void Update()
    {
        // Check for a key press or any condition to switch cameras
        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    // Toggle between the cameras
        //    ToggleCameras();
        //}
    }

    void ToggleCameras()
    {
        // Swap the active state of the cameras
        ortograficTopDownCamera.enabled = !ortograficTopDownCamera.enabled;
        perspectiveCamera.enabled = !perspectiveCamera.enabled;
    }

    public void EnableOrtographic()
    {
        SimulationManager.instance.isViewingGrid = false;
        EnableCamera(ortograficTopDownCamera);
        DisableCamera(perspectiveCamera);
    }

    public void EnablePerspectiveCamera()
    {
        EnableCamera(perspectiveCamera);
        DisableCamera(ortograficTopDownCamera);
    }
    void EnableCamera(Camera camera)
    {
        // Enable the specified camera
        if (camera != null)
        {
            camera.enabled = true;
        }
    }

    void DisableCamera(Camera camera)
    {
        // Disable the specified camera
        if (camera != null)
        {
            camera.enabled = false;
        }
    }

    public void SwitchtoGridView()
    {
        if (SimulationManager.instance.mode != SceneMode.runningSimulation) return;
        SimulationManager.instance.isViewingGrid = true;
        //ENABLE DRONE CAMERAS
        foreach (SurvailanceManager area in SimulationManager.instance.areas)
        {
            area.DisableCameraGrid();
        }

        SimulationManager.instance.currentGrid.AdjustCameraGrid(true);

        DisableCamera(ortograficTopDownCamera);
        DisableCamera(perspectiveCamera);
    }

    public void SwitchToOverheadView()
    {
        SimulationManager.instance.isViewingGrid = false;
        //DISABLE DRONE CAMERAS
        SimulationManager.instance.currentGrid.DisableCameraGrid();

        EnableOrtographic();
    }
}

