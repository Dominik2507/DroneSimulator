using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;
    
    public List<GameObject> panels;
    public int runningSimulationPanel = 0;
    public int createAreasPanel = 1;
    public int gridViewPannel = 2;
    public int overheadViewPanel = 3;

    public GameObject curveList;
    public GameObject curveButtonPanel;
    public GameObject curveUIPrefab;
    public GameObject dronePanelButtons;

    public GameObject premadeAreasPanel;
    public GameObject freeDrawPanel;

    public Button freeDrawModeButton;
    public Button presetsModeButton;

    public Button startSimulation;
    public Button endSimulation;

    public RawImage grayMaterial;

    public Button dropdownCurveList;
    public bool curveListIsExpaned = true;

    public Slider speedSlider;

    public TMP_InputField DroneFovInputField;
    public TMP_InputField verticalOffsetInputField;

    SceneMode currMode = SceneMode.none;
    void Start()
    {
        if (instance == null) instance = this;
    }

    private void Update()
    {
        if (currMode != SimulationManager.instance.mode)
        {
            if(currMode == SceneMode.runningSimulation)
            {
                onEndSimulation();
            }
            
            currMode = SimulationManager.instance.mode;
            
            switch (currMode)
            {
                case SceneMode.freeDraw:
                    onFreeDrawButtonClick();
                    break;
                case SceneMode.presetDraw:
                    onPresetsButtonClick();
                    break;
                case SceneMode.runningSimulation:
                    onRunningSimulation();
                    break;
            }
        }

    }

    public void onFreeDrawButtonClick()
    {
        freeDrawModeButton.interactable = false;
        presetsModeButton.interactable = true;
        SimulationManager.instance.mode = SceneMode.freeDraw;

        premadeAreasPanel.active = false;
        freeDrawPanel.active = true;
    }

    public void onPresetsButtonClick()
    {
        freeDrawModeButton.interactable = true;
        presetsModeButton.interactable = false;
        SimulationManager.instance.mode = SceneMode.presetDraw;

        premadeAreasPanel.active = true;
        freeDrawPanel.active = false;
    }

    public void onRunningSimulation()
    {
        SimulationManager.instance.gameObject.GetComponent<CameraSwitcher>().EnableOrtographic();
        SimulationManager.instance.mode = SceneMode.runningSimulation;

        panels[createAreasPanel].active = false;
        panels[runningSimulationPanel].active = true;
    }

    public void onEndSimulation()
    {
        SimulationManager.instance.gameObject.GetComponent<CameraSwitcher>().EnableOrtographic();

        panels[createAreasPanel].active = true;
        panels[runningSimulationPanel].active = false;

        if (SimulationManager.instance.areas.Count == 0) startSimulation.interactable = false;
    }

    public void enableStartSimulation()
    {
        startSimulation.interactable = true;
    }

    public void disableStartSimulation()
    {
        startSimulation.interactable = false;
    }

    public void switchToGridView()
    {
        panels[overheadViewPanel].active = false;
        panels[gridViewPannel].active = true;
        //curveButtonPanel.active = false;
    }

    public void switchToOverheadView()
    {
        panels[gridViewPannel].active = false;
        panels[overheadViewPanel].active = true;
        //curveButtonPanel.active = true;
    }

    public void onDropdownClick()
    {
        if (curveListIsExpaned)
        {
            curveList.SetActive(false);
            //curveList.transform.localScale = Vector3.zero;

        }
        else
        {
            curveList.SetActive(true);
            //curveList.transform.localScale = Vector3.one;
        }
        curveListIsExpaned = !curveListIsExpaned;
        dropdownCurveList.transform.localScale = Vector3.Scale(dropdownCurveList.transform.localScale, new Vector3(1, -1, 1));
    }

    public void onSpeedChange()
    {
        SimulationManager.instance.SetSimSpeed(speedSlider.value);
    }

    public void onFOVChange()
    {
        if (int.TryParse(DroneFovInputField.text, out int FOV))
        {
            SimulationManager.instance.SetDroneFOV(FOV);
        }
        else
        {
            Debug.Log("INPUT TYPE ERROR");
        }
    }
    public void onOffsetChange()
    {
        if (float.TryParse(verticalOffsetInputField.text, out float offset))
        {
            SimulationManager.instance.SetDroneOffset(offset);
        }
        else
        {
            Debug.Log("INPUT TYPE ERROR");
        }
    }
}
