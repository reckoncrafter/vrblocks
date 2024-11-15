using System;
using System.Collections.Generic;
using UnityEngine;

public class TurtleMovement : MonoBehaviour
{
    public float movementDuration = 2.0f;
    public float animationSpeed = 1.0f;
    public Vector3 moveDistance = Vector3.zero;

    private Animator animator;
    private Queue<Action> queue;

    private void SetIsWalking(bool value)
    {
        animator.SetBool("isWalking", value);
    }

    private void SetAnimSpeed(float value)
    {
        animator.SetFloat("animSpeed", value);
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        queue = new Queue<Action>();

        SetAnimSpeed(animationSpeed);

        // temp
        // Similar to what you'd see in lua
        // alternatively, you can give this a function to call when the step ends so the queue is in lua (not implemented)
        WalkForward();
        RotateRight();
        WalkForward();
        RotateLeft();
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

    public void RotateLeft()
    {
        queue.Enqueue(() => PerformRotate(-90));
    }

    public void RotateRight()
    {
        queue.Enqueue(() => PerformRotate(90));
    }

    private void StartNextAction()
    {
        if (queue.Count > 0)
        {
            queue.Dequeue().Invoke();
        }
    }

    private void EndMovement()
    {
        SetIsWalking(false);
        StartNextAction();
    }

    private void PerformWalkForward()
    {
        SetIsWalking(true);

        Vector3 targetPosition = transform.position + Vector3.Scale(transform.forward, moveDistance);

        LTDescr tween = null;
        if (Math.Abs(targetPosition.x - transform.position.x) > 0.01)
        {
            tween = transform.LeanMoveX(targetPosition.x, movementDuration);
        }
        else
        {
            tween = transform.LeanMoveZ(targetPosition.z, movementDuration);
        }

        tween.setEase(LeanTweenType.easeInOutQuad).setOnComplete(EndMovement);
    }

    private void PerformRotate(float angle)
    {
        SetIsWalking(true);

        LTDescr tween = transform.LeanRotateY(transform.rotation.eulerAngles.y + angle, movementDuration);
        tween.setEase(LeanTweenType.easeInOutQuad).setOnComplete(EndMovement);
    }
}
