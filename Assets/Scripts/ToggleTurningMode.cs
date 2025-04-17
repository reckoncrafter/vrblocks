using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

using TurningMode = SnapTurnDynamicReassignment.TurningMode;
public class ToggleTurningMode : MonoBehaviour
{
    private SnapTurnDynamicReassignment dynamicReassignment;
    private Toggle toggle;
    void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnToggle);
    }
    void Start()
    {
        XROrigin rig = FindObjectOfType<XROrigin>();
        if(rig)
        {
            dynamicReassignment = rig.GetComponent<SnapTurnDynamicReassignment>();
        }
        SetToggleText();
    }

    void SetToggleText()
    {
        TextMeshProUGUI text = toggle.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        if(dynamicReassignment.turningMode == TurningMode.SnapTurn)
        {
            text.text = "Snap Turning";
        }
        else if(dynamicReassignment.turningMode == TurningMode.ContinuousTurn)
        {
            text.text = "Continuous Turning";
        }
        else
        {
            text.text = "ERROR";
        }
    }

    void OnToggle(bool setting)
    {
        dynamicReassignment.turningMode = dynamicReassignment.turningMode == 0 ? (TurningMode)1 : 0;
        dynamicReassignment.AssignTurnActions();
        SetToggleText();
    }
}
