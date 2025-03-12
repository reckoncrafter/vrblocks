using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectTurtle : MonoBehaviour
{
    public GameObject GoalSphere;
    public GameObject Turtle;
    public AudioClip turtleSuccessAudio;
    public float distanceActivationThreshold;
    public bool isNearby;

    public float distance;

    void Start()
    {
        Turtle = GameObject.Find("/MapSpawner/Turtle");
        GoalSphere = GameObject.Find("/MapSpawner/GoalSphere");

        Turtle.GetComponent<TurtleMovement>().EndOfMovementEvent.AddListener(CheckDistance);
    }

    void CheckDistance()
    {
        distance = Vector3.Distance(GoalSphere.transform.position, Turtle.transform.position);

        if (distance <= distanceActivationThreshold)
        {
            if (!isNearby)
            {
                AudioSource.PlayClipAtPoint(turtleSuccessAudio, Turtle.transform.position);
            }

            isNearby = true;
        }
        else
        {
            isNearby = false;
        }
    }
}
