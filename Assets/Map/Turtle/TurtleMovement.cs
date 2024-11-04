using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleMovement : MonoBehaviour
{
    public float movementDuration = 2.0f;
    public float animationSpeed = 1.0f;
    public Vector3 moveDistance = Vector3.zero;

    private Animator animator;
    private Queue<Action> queue;

    private readonly string walkingParam = "isWalking";
    private readonly string animSpeedParam = "animSpeed";

    void Start()
    {
        animator = GetComponent<Animator>();
        queue = new Queue<Action>();

        animator.SetFloat(animSpeedParam, animationSpeed);

        // temp
        // Similar to what you'd see in lua
        // alternatively, you can give this a function to call when the step ends so the queue is in lua (not implemented)
        WalkForward();
        WalkForward();
        StartQueue();
    }

    public void StartQueue()
    {
        StartNextAction();
    }

    public void WalkForward()
    {
        queue.Enqueue(PerformWalkForward);
    }

    private void SetIsWalking(bool value)
    {
        animator.SetBool(walkingParam, value);
    }


    private void StartNextAction()
    {
        if (queue.Count > 0)
        {
            queue.Dequeue().Invoke();
        }
    }

    private void PerformWalkForward()
    {
        SetIsWalking(true);

        Vector3 targetPosition = transform.position + Vector3.Scale(Vector3.forward, moveDistance);
        if (targetPosition.x > 0.01)
        {
            transform.LeanMoveX(targetPosition.x, movementDuration).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
            {
                SetIsWalking(false);
                StartNextAction();
            });
        }
        else
        {
            transform.LeanMoveZ(targetPosition.z, movementDuration).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
            {
                SetIsWalking(false);
                StartNextAction();
            });
        }
    }
}
