using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(ControlTutorialHandler))]
public class ControlTutorialHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ControlTutorialHandler controlTutorialHandler = (ControlTutorialHandler)target;
        if(GUILayout.Button("Toggle Control Labels"))
        {
            controlTutorialHandler.toggleTutorial();
        }
    }
}

#endif

public class ControlTutorialHandler : MonoBehaviour
{
    public GameObject rightTutorial;
    public GameObject leftTutorial;
    public GameObject controlLabelPopup;
    public InputActionReference disableControlLabels;
    public float popupTimeout;
    ControllerModels cm;
    bool isActive = false;

    void Start()
    {
        IEnumerator close_popup()
        {
            yield return new WaitForSeconds(popupTimeout);
            controlLabelPopup.SetActive(false);
        }
        controlLabelPopup.SetActive(true);
        StartCoroutine(close_popup());
        cm = GetComponent<ControllerModels>();
        disableControlLabels.action.started += (ctx) => {toggleTutorial();};
    }
    public void toggleTutorial()
    {
        if(controlLabelPopup.activeInHierarchy)
        {
            controlLabelPopup.SetActive(false);
        }
        isActive = !isActive;
        rightTutorial.SetActive(isActive);
        leftTutorial.SetActive(isActive);
        cm.EnableControllerModel(isActive, true);
        cm.EnableControllerModel(isActive, false);
    }
}
