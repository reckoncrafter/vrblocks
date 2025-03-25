using UnityEngine;

public class DetectTurtle
{
    private PlayerUIManager? playerUIManager;
    public GameObject? Goal;
    public GameObject? Turtle;
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
        Goal = GameObject.Find("/MapSpawner/GoalFlag");

        Turtle.GetComponent<TurtleMovement>().EndOfMovementEvent.AddListener(CheckDistance);

        if (Turtle == null || Goal == null)
        {
            Debug.LogError("Turtle or Goal not found");
            return;
        }

        AddTurtleListener();
    }

    public void SetTurtleAndGoal(TurtleMovement tempTurtle, GameObject goal)
    {
        RemoveTurtleListener();

        Turtle = tempTurtle.gameObject;
        Goal = goal;

        AddTurtleListener();
    }

    void CheckDistance()
    {
        if (Turtle == null || Goal == null || playerUIManager == null)
        {
            Debug.LogError("Turtle, Goal, or PlayerUIManager is null for CheckDistance");
            return;
        }

        float distance = Vector3.Distance(Goal.transform.position, Turtle.transform.position);

        if (distance <= distanceActivationThreshold)
        {
            if (!isNearby)
            {
                Debug.Log("Goal reached!");
                Turtle.GetComponent<TurtleMovement>().Success();
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
