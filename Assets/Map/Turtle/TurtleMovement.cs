using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class TurtleMovement : MonoBehaviour
{
    public AudioClip turtleCollisionAudio;
    public AudioClip turtleFallAudio;
    public AudioClip turtleJumpAudio;


    public float movementDuration = 2.0f;
    public float animationSpeed = 1.0f;
    public float failBounciness = 0.3f;
    public Vector3 moveDistance = Vector3.zero;

    private bool canReset = false; // separate from fail because external things can call reset
    private bool canFail = false;
    private Vector3 resetPosition = Vector3.zero;
    private Quaternion resetRotation = Quaternion.identity;
    private float resetBounciness = 0.0f;
    private RigidbodyConstraints resetConstraints;

    private LTDescr? tween;
    private Animator animator;
    private Rigidbody rb;
    private BoxCollider turtleCollider;
    private Queue<Action> queue;
    private Func<bool> conditionFunction = () => false;
    private TurtleProximity turtleProximity;

    // jumping things
    private bool isGrounded = false;
    private bool shouldJump = false;
    private bool canBeGrounded = true;

    public UnityEvent EndOfMovementEvent;

    private TMP_Text failureDialog;

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
        queue = new Queue<Action>();
        failureDialog = GameObject.Find("FailureDialog/Canvas/Text")?.GetComponent<TextMeshProUGUI>();

        resetPosition = transform.position;
        resetRotation = transform.rotation;
        resetBounciness = turtleCollider.material.bounciness;
        resetConstraints = rb.constraints;

        SetAnimSpeed(animationSpeed);
    }


    void FixedUpdate()
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

    public void StartQueue()
    {
        canFail = true;
        canReset = true;
        StartNextAction();
        failureDialog.text = "";
    }

    // I will not atone for my sins
    public void ConditionFacingWall()
    {
        queue.Enqueue(() =>
        {
            conditionFunction = () =>
            {
                RaycastHit hit;
                Debug.DrawRay(transform.position, transform.forward, Color.red, 10);
                if(Physics.Raycast(transform.position, transform.forward, out hit, 0.25f))
                {
                    if(hit.transform.parent.gameObject.name == "MapSpawner"){
                        return true;
                    }
                }
                return false;
            };
            StartNextAction();
        });
    }

    public void ConditionFacingCliff()
    {
        queue.Enqueue(() =>
        {
          conditionFunction = () =>
          {
              RaycastHit hit;
              Debug.DrawRay(transform.position + transform.forward * 0.5f, -transform.up, Color.red, 10);
              if(!Physics.Raycast(transform.position + transform.forward * 0.5f, -transform.up, out hit, 1.00f))
              {
                  Debug.Log("ConditionFacingMapEdge: Cliff detected!");
                  return true;
              }
              return false;
          };
          StartNextAction();
        });
    }

    public void ConditionFacingStepDown()
    {
        queue.Enqueue(() =>
        {
            conditionFunction = () =>
            {
                RaycastHit hit;
                Debug.DrawRay(transform.position + transform.forward * 0.5f, -transform.up, Color.red, 10);
                bool aheadLevel = Physics.Raycast(transform.position + transform.forward * 0.5f, -transform.up, out hit, 0.05f);
                bool aheadNotEmpty = Physics.Raycast(transform.position + transform.forward * 0.5f, -transform.up, out hit, 1.00f);

                if(!aheadLevel && aheadNotEmpty)
                {
                    Debug.Log("ConditionFacingStepDown: Ahead floor not level with turtle.");
                    return true;
                }
                return false;
            };
            StartNextAction();
        });
    }


    public void setConditionTrue()
    {
        queue.Enqueue(() =>
        {
            conditionFunction = () => true;
            StartNextAction();
        });
    }

    public void setConditionFalse()
    {
        queue.Enqueue(() =>
        {
            conditionFunction = () => false;
            StartNextAction();
        });
    }

    public void EnqueueIfStatementBegin()
    {
        queue.Enqueue(IfStatementBegin);
    }

    private void IfStatementBegin()
    {
        int elseBlockIndex = -1;
        int EndIndex = 0;
        bool isElse = false;
        bool isComplete = false;
        Action[] queueArray = queue.ToArray();

        for (int i = 0; i < queueArray.Length; i++)
        {
            if (queueArray[i] == IfStatementEnd)
            {
                EndIndex = i;
                isComplete = true;
                Debug.Log("IfStatement found its end. " + EndIndex);
            }
            if (queueArray[i] == ElseStatement)
            {
                elseBlockIndex = i;
                isElse = true;
                Debug.Log("Found Else Statement. " + elseBlockIndex);
            }
        }

        if (!isComplete)
        {
            Debug.Log("No IfStatementEnd Found");
            failureDialog.text += "Incomplete If Statement! (No IfEnd Block)\n";
            Fail();
            return;
        }

        if (!conditionFunction())
        {
            if (isElse)
            {
                for (int i = 0; i < elseBlockIndex; i++)
                {
                    queue.Dequeue();
                }
                Debug.Log("Condition not satisfied. Executing Else Statement.");
            }
            else
            {
                for (int i = 0; i < EndIndex; i++)
                {
                    queue.Dequeue();
                }
                Debug.Log("Condition not satisfied. Skipping.");
            }

        }
        else
        {
            Debug.Log("Condition satisfied.");

            if (isElse)
            {
                var toList = new List<Action>(queue);
                for (int i = elseBlockIndex + 1; i < EndIndex; i++)
                {
                    toList.RemoveAt(i);
                    Debug.Log(toList[i].ToString());
                }
                queue = new Queue<Action>(toList);
            }
        }
        StartNextAction();
    }

    public void EnqueueIfStatementEnd()
    {
        queue.Enqueue(IfStatementEnd);
    }

    private void IfStatementEnd()
    {
        StartNextAction();
    }

    public void EnqueueElseStatement()
    {
        queue.Enqueue(ElseStatement);
    }

    private void ElseStatement()
    {
        StartNextAction();
    }

    public void EnqueueWhileStatementBegin()
    {
        queue.Enqueue(WhileStatementBegin);
    }

    private List<Action>? loopList;
    private Queue<Action>? swapQueue;

    private void WhileStatementBegin()
    {
        Debug.Log("WhileStatementBegin called.");
        List<Action> queueList = new(queue);

        int EndIndex = 0;
        for (int i = 0; i < queueList.Count; i++)
        {
            Debug.Log(queueList[i]);
            if (queueList[i] == WhileStatementEnd)
            {
                EndIndex = i;
                Debug.Log("WhileStatement found its end. " + EndIndex);
            }
        }
        if (EndIndex == 0)
        {
            Debug.Log("No WhileStatementEnd found");
            failureDialog.text += "Incomplete While Statement! (No WhileEnd Block)";
            Fail();
            return;
        }

        loopList = queueList.GetRange(0, EndIndex + 1);
        Debug.Log("size of while loop: " + loopList.Count);
        // 1. save segment of queue from WhileStatementBegin to WhileStatementEnd.

        for (int i = 0; i < EndIndex + 1; i++)
        {
            queue.Dequeue();
        }

        // 2. remove saved segment from queue.

        swapQueue = new Queue<Action>(queue);
        queue = new Queue<Action>(loopList);
        //3. swap main queue with segment

        StartNextAction();
        // 4. execute
    }

    public void EnqueueWhileStatementEnd()
    {
        queue.Enqueue(WhileStatementEnd);
    }

    private void WhileStatementEnd()
    {
        Debug.Log("WhileStatementEnd called.");

        if (loopList == null || swapQueue == null)
        {
            Debug.LogError("WhileStatementEnd called without a loopList or swapQueue");
            return;
        }

        // 5. When the end is reached, WhileStatementEnd should
        if (conditionFunction())
        {
            queue = new Queue<Action>(loopList);
        }
        else
        {
            queue = swapQueue;
        }
        //     5a. check if the statement should repeat
        //     5b. if it should, place a the segment in the queue again.
        //     5c. if not, swap back in the queue and resume execution
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

    public void Reset()
    {
        if (!canReset)
        {
            return;
        }
        canReset = false;
        canFail = false;

        SetIsWalking(false);
        queue.Clear();
        tween?.reset();

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        turtleCollider.material.bounciness = resetBounciness;
        transform.SetPositionAndRotation(resetPosition, resetRotation);
        rb.constraints = resetConstraints;
    }

    private void StartNextAction()
    {
        if (queue.Count > 0)
        {
            queue.Dequeue().Invoke();
        }
        else
        {
            Fail(() =>
            {
                AudioSource.PlayClipAtPoint(turtleCollisionAudio, transform.position);
            });
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
        AudioSource.PlayClipAtPoint(turtleJumpAudio, transform.position);
        // animator.SetTrigger("jump");

        float jumpForce = Mathf.Sqrt(moveDistance.y * 1.5f * 2 * Mathf.Abs(Physics.gravity.y)); // h = (µsin(θ))^2 / 2g with 50% more height
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
        StartNextAction();
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

    private void Fail(Action? failAction = null)
    {
        if (!canFail)
        {
            return;
        }
        canFail = false;

        rb.constraints = RigidbodyConstraints.None;
        turtleCollider.material.bounciness = failBounciness;
        queue.Clear();
        tween?.reset();

        failAction?.Invoke();

        StartCoroutine(WaitAndReset());
    }
}
