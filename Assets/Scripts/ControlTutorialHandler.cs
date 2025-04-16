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
    public InputActionReference disableControlLabels;
    ControllerModels cm;
    bool isActive = true;

    void Start()
    {
        cm = GetComponent<ControllerModels>();
        IEnumerator delay()
        {
            yield return new WaitForSeconds(0.5f);
            cm.EnableControllerModel(true, true);
            cm.EnableControllerModel(true, false);
        }
        StartCoroutine(delay());

        disableControlLabels.action.started += (ctx) => {toggleTutorial();};
    }
    public void toggleTutorial()
    {
        isActive = !isActive;
        rightTutorial.SetActive(isActive);
        leftTutorial.SetActive(isActive);
        cm.EnableControllerModel(isActive, true);
        cm.EnableControllerModel(isActive, false);
    }
}
