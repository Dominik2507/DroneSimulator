using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;

    public GameObject curveButtonPanel;
    void Start()
    {
        if (instance == null) instance = this;
    }
    

}
