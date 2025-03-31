using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TurtleMovement : MonoBehaviour
{
    public AudioClip turtleCollisionAudio;
    public AudioClip turtleFallAudio;
    public AudioClip turtleJumpAudio;
    public AudioClip turtleSuccessAudio;


    public float movementDuration = 2.0f;
    public float animationSpeed = 1.0f;
    public float failBounciness = 0.3f;
    public Vector3 moveDistance = Vector3.zero;

    public bool canReset = false; // separate from fail because external things can call reset
    public bool canFail = false;
    private Vector3 resetPosition = Vector3.zero;
    private Quaternion resetRotation = Quaternion.identity;
    private float resetBounciness = 0.0f;
    private RigidbodyConstraints resetConstraints;

    private LTDescr? tween;
    private Animator animator;
    private Rigidbody rb;
    private BoxCollider turtleCollider;

    // jumping things
    private bool isGrounded = false;
    public bool shouldJump = false; // sorry! i need it!
    private bool canBeGrounded = true;

    public UnityEvent EndOfMovementEvent;
    public UnityEvent FailEvent = new UnityEvent();
    public UnityEvent SuccessEvent = new UnityEvent();

    public Action afterJumpAction;

    private EmoteBoard emoteBoard;


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
        turtleCollider = GetComponent<BoxCollider>();

        resetPosition = transform.position;
        resetRotation = transform.rotation;
        resetBounciness = turtleCollider.material.bounciness;
        resetConstraints = rb.constraints;

        SetAnimSpeed(animationSpeed);
        afterJumpAction = () => { };

        emoteBoard = GetComponentInChildren<EmoteBoard>();
    }


    public void FixedUpdate()
    {
        if (!Physics.Raycast(transform.position, -transform.up, moveDistance.y * 2))
        {
            Fail(() =>
            {
                AudioSource.PlayClipAtPoint(turtleFallAudio, transform.position);
            });
        }

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
        EndMovement();
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

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
        {
            if (isGrounded)
            {
                FailOnHitNose();
            }
            else
            {
                FailOnHitBack();
            }
        }
    }
    // I will not atone for my sins
    public bool ConditionFacingWall()
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position, transform.forward, Color.red, 10);
        if (Physics.Raycast(transform.position, transform.forward, out hit, 0.25f))
        {
            if (hit.transform.parent.gameObject.name == "MapSpawner")
            {
                return true;
            }
        }
        return false;
    }

    public bool ConditionFacingCliff()
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position + transform.forward * 0.5f, -transform.up, Color.red, 10);
        if (!Physics.Raycast(transform.position + transform.forward * 0.5f, -transform.up, out hit, 1.00f))
        {
            Debug.Log("ConditionFacingMapEdge: Cliff detected!");
            return true;
        }
        return false;
    }

    public bool ConditionFacingStepDown()
    {

        RaycastHit hit;
        Debug.DrawRay(transform.position + transform.forward * 0.5f, -transform.up, Color.red, 10);
        bool aheadLevel = Physics.Raycast(transform.position + transform.forward * 0.5f, -transform.up, out hit, 0.05f);
        bool aheadNotEmpty = Physics.Raycast(transform.position + transform.forward * 0.5f, -transform.up, out hit, 1.00f);

        if (!aheadLevel && aheadNotEmpty)
        {
            Debug.Log("ConditionFacingStepDown: Ahead floor not level with turtle.");
            return true;
        }
        return false;
    }


    public bool ConditionTrue()
    {
        return true;
    }

    public bool ConditionFalse()
    {
        return false;
    }


    public void Reset()
    {
        if (!canReset)
        {
            return;
        }
        canReset = false;
        canFail = false;

        SetIsWalking(false);
        //queue.Clear();
        tween?.reset();

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        turtleCollider.material.bounciness = resetBounciness;
        transform.SetPositionAndRotation(resetPosition, resetRotation);
        rb.constraints = resetConstraints;
    }

    private void EndMovement()
    {
        SetIsWalking(false);

        EndOfMovementEvent.Invoke();
    }

    public void PerformWalkForward()
    {
        emoteBoard.Emote(EmoteBoard.Emotes.MoveForward);
        SetIsWalking(true);

        Vector3 targetPosition = transform.position + Vector3.Scale(transform.forward, moveDistance);

        tween = null;
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

    public void PerformRotateRight()
    {
        emoteBoard.Emote(EmoteBoard.Emotes.RotateRight);
        PerformRotate(90.0f);
    }

    public void PerformRotateLeft()
    {
        emoteBoard.Emote(EmoteBoard.Emotes.RotateLeft);
        PerformRotate(-90.0f);
    }

    private void PerformRotate(float angle)
    {
        SetIsWalking(true);

        tween = transform.LeanRotateY(transform.rotation.eulerAngles.y + angle, movementDuration);
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
        emoteBoard.Emote(EmoteBoard.Emotes.Jump);
        animator.SetTrigger("Jump");
    }

    private void AddJumpForce()
    { // to be called by the jump animation
        AudioSource.PlayClipAtPoint(turtleJumpAudio, transform.position);

        float jumpForce = Mathf.Sqrt(moveDistance.y * 1.5f * 2 * Mathf.Abs(Physics.gravity.y)); // h = (µsin(θ))^2 / 2g with 50% more height
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;

        afterJumpAction();
        afterJumpAction = () => { };
    }

    private void FailOnHitBack()
    {
        Fail(() =>
        {
            AudioSource.PlayClipAtPoint(turtleCollisionAudio, transform.position);
            rb.AddTorque(new Vector3(UnityEngine.Random.Range(-10f, 10f), 0, UnityEngine.Random.Range(-10f, 10f)), ForceMode.Impulse);
        });
    }

    private void FailOnHitNose()
    {
        Fail(() =>
        {
            AudioSource.PlayClipAtPoint(turtleCollisionAudio, transform.position);
            rb.AddForce((-transform.forward + transform.up) * Mathf.Sqrt(moveDistance.y * 0.3f * 2 * Mathf.Abs(Physics.gravity.y)), ForceMode.Impulse);
            rb.AddTorque(-transform.right * 5, ForceMode.Impulse);
        });
    }

    // temp for demo purposes but could be a feature
    private IEnumerator WaitAndReset()
    {
        yield return new WaitForSeconds(3.0f);
        Reset();
    }

    public void Fail(Action? failAction = null)
    {
        if (!canFail)
        {
            return;
        }
        emoteBoard.Emote(EmoteBoard.Emotes.Failure);
        canFail = false;

        FailEvent.Invoke();

        rb.constraints = RigidbodyConstraints.None;
        turtleCollider.material.bounciness = failBounciness;
        tween?.reset();

        failAction?.Invoke();

        canReset = true;
        StartCoroutine(WaitAndReset());
    }

    public void Success()
    {
        emoteBoard.Emote(EmoteBoard.Emotes.Success);
        canFail = false;
        canReset = false;
        tween?.reset();
        SetIsWalking(false);
        AudioSource.PlayClipAtPoint(turtleSuccessAudio, transform.position);
        SuccessEvent.Invoke();
    }
}
