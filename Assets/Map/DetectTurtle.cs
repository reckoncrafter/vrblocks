using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DetectTurtle : MonoBehaviour
{
    public GameObject levelUIManager;
    private PlayableDirector director;
    public GameObject GoalSphere;
    public GameObject Turtle;
    public float distanceActivationThreshold;
    public bool isNearby;

    public float distance;

    void Start(){
        director = levelUIManager.GetComponent<PlayableDirector>();
        Turtle = GameObject.Find("/MapSpawner/Turtle");
        GoalSphere = GameObject.Find("/MapSpawner/GoalSphere");

        Turtle.GetComponent<TurtleMovement>().EndOfMovementEvent.AddListener(CheckDistance);
    }

    void CheckDistance()
    {
        distance = Vector3.Distance (GoalSphere.transform.position, Turtle.transform.position);
        if (distance <= distanceActivationThreshold){
            isNearby = true;
            if(this.enabled){
                director.Play();
                this.enabled = false;
            }
        }
        else{
            isNearby = false;
        }
        Debug.Log(isNearby);
    }
}
