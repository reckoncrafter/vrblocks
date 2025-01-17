using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class TurtleMovement : MonoBehaviour
{
    public float movementDuration = 2.0f;
    public float animationSpeed = 1.0f;
    public Vector3 moveDistance = Vector3.zero;
    public Queue<Action> queue;

    private Animator animator;
    private Rigidbody rb;

    // jumping things
    private bool isGrounded = false;
    private bool shouldJump = false;
    private bool canBeGrounded = true;

    public UnityEvent EndOfMovementEvent;

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
        rb = GetComponent<Rigidbody>();
        queue = new Queue<Action>();

        SetAnimSpeed(animationSpeed);

        // temp
        // Similar to what you'd see in lua
        // alternatively, you can give this a function to call when the step ends so the queue is in lua (not implemented)
        WalkForward();
        RotateRight();
        WalkForward();
        RotateLeft();
        WalkForward();
        WalkForward();
        RotateRight();
        Jump();
        WalkForward();
        Jump();
        WalkForward();

        StartQueue();
    }


    void Update()
    {
        if (shouldJump && isGrounded)
        {
            PerformJump();
            shouldJump = false;
            StartCoroutine(WaitAndCanBeGrounded());
        }
    }

    // wait for the turtle to leave the ground before checking if it's grounded again
    private IEnumerator WaitAndCanBeGrounded()
    {
        yield return new WaitForSeconds(0.5f);
        canBeGrounded = true;
    }

    void OnCollisionStay()
    {
        if (canBeGrounded)
        {
            isGrounded = true;
            canBeGrounded = false;
        }
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

    public void Jump()
    {
        queue.Enqueue(HandleJump);
    }

    private void StartNextAction()
    {
        if (queue.Count > 0)
        {
            queue.Dequeue().Invoke();
        }
        else
        {
            Debug.Log("No Actions in Queue!");
        }
    }

    private void EndMovement()
    {
        SetIsWalking(false);
        StartNextAction();

        EndOfMovementEvent.Invoke();
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

    private void HandleJump()
    {
        StartCoroutine(WaitAndSetJump());
    }

    // can't change a bool multiple times in the same frame
    private IEnumerator WaitAndSetJump()
    {
        yield return null;
        shouldJump = true;
    }

    private void PerformJump()
    {
        // animator.SetTrigger("jump");
        float jumpForce = Mathf.Sqrt(moveDistance.y * 1.5f * 2 * -Physics.gravity.y); // h = (µsin(θ))^2 / 2g with 50% more height
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
        StartNextAction();
    }
}
