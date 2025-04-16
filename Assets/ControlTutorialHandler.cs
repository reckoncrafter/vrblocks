using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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

        disableControlLabels.action.started += (ctx) => {toggleTutorial(!isActive);};
    }
    void toggleTutorial(bool enabled)
    {
        isActive = enabled;
        rightTutorial.SetActive(enabled);
        leftTutorial.SetActive(enabled);
        cm.EnableControllerModel(enabled, true);
        cm.EnableControllerModel(enabled, false);
    }
}
