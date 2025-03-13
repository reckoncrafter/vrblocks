using UnityEngine;

public class DetectTurtle
{
    private PlayerUIManager? playerUIManager;
    private GameObject? GoalSphere;
    private GameObject? Turtle;
    public AudioClip? TurtleSuccessAudio { get; set; }
    private readonly float distanceActivationThreshold = 0.25f;
    private bool isNearby;

    void AddTurtleListener()
    {
        if (Turtle != null)
        {
            Turtle.GetComponent<TurtleMovement>().EndOfMovementEvent.AddListener(CheckDistance);
        }

        playerUIManager = GameObject.Find("PlayerUIManager").GetComponent<PlayerUIManager>();
    }

    void RemoveTurtleListener()
    {
        if (Turtle != null)
        {
            Turtle.GetComponent<TurtleMovement>().EndOfMovementEvent.RemoveListener(CheckDistance);
        }
    }

    public void FindTurtle()
    {
        RemoveTurtleListener();

        Turtle = GameObject.Find("/MapSpawner/Turtle");
        GoalSphere = GameObject.Find("/MapSpawner/GoalSphere");

        Turtle.GetComponent<TurtleMovement>().EndOfMovementEvent.AddListener(CheckDistance);

        if (Turtle == null || GoalSphere == null)
        {
            Debug.LogError("Turtle or GoalSphere not found");
            return;
        }

        AddTurtleListener();
    }

    public void SetTurtleAndGoal(TurtleMovement tempTurtle, GameObject goal)
    {
        RemoveTurtleListener();

        Turtle = tempTurtle.gameObject;
        GoalSphere = goal;

        AddTurtleListener();
    }

    void CheckDistance()
    {
        if (Turtle == null || GoalSphere == null || playerUIManager == null)
        {
            Debug.LogError("Turtle, GoalSphere, or PlayerUIManager is null for CheckDistance");
            return;
        }

        float distance = Vector3.Distance(GoalSphere.transform.position, Turtle.transform.position);

        if (distance <= distanceActivationThreshold)
        {
            if (!isNearby)
            {
                AudioSource.PlayClipAtPoint(TurtleSuccessAudio, Turtle.transform.position);
                playerUIManager.EnableEndScreen();
            }

            isNearby = true;
        }
        else
        {
            isNearby = false;
        }
    }
}
