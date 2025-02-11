using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayTutorialOnPickup : MonoBehaviour
{
    public GameObject levelUIManager;
    private PlayableDirector director;
    
    void Start()
    {
        director = levelUIManager.GetComponent<PlayableDirector>();
        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(TriggerPlayTutorial);
    }

    public void TriggerPlayTutorial(SelectEnterEventArgs args)
    {
        director.Play();
    }
}
