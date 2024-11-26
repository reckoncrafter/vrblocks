using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectTurtle : MonoBehaviour
{
    public GameObject GoalSphere;
    public GameObject Turtle;
    public float distanceActivationThreshold;
    public bool isNearby;

    public float distance;

    void Start(){
        Turtle = GameObject.Find("/MapSpawner/Turtle");
        GoalSphere = GameObject.Find("/MapSpawner/GoalSphere");
    }
    void Update()
    {
        distance = Vector3.Distance (GoalSphere.transform.position, Turtle.transform.position);

        if (distance <= distanceActivationThreshold){
            isNearby = true;
        }
        else{
            isNearby = false;
        }
    }
}
