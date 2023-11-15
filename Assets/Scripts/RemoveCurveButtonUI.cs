using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveCurveButtonUI : MonoBehaviour
{
    public SurvailanceManager sm;

    public void HandleClick()
    {
        SimulationManager.instance.RemoveSurvailanceManager(sm);
    }
}
