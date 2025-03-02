using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class LevelUIManager : MonoBehaviour
{
    public PlayableDirector director;
    
    [Header("Signal Timeline to Play")]
    public List<GameObject> interactables;

    void Start()
    {
        foreach(GameObject interactable in interactables)
        {
            if(interactable.GetComponent<Button>() != null)
            {
                interactable.GetComponent<Button>().onClick.AddListener(ResumeTimeline);
            }
            else if(interactable.GetComponent<XRGrabInteractable>() != null)
            {
               interactable.GetComponent<XRGrabInteractable>().selectEntered.AddListener(ResumeTimeline); 
            }
        }
    }

    public void ResumeTimeline()
    {
        director.Play();
    }

    public void ResumeTimeline(SelectEnterEventArgs args)
    {
        director.Play();
    }
}
