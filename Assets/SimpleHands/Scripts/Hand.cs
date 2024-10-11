using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class Hand : MonoBehaviour
{
  [SerializeField] private InputActionReference gripAction;
  [SerializeField] private InputActionReference triggerAction;
  Animator animator;

  private float currGrip;
  private float currTrigger;

  private readonly string animatorGripParam = "Grip";
  private readonly string animatorTriggerParam = "Trigger";

  void Start()
  {
    animator = GetComponent<Animator>();
  }

  void Update()
  {
    if (!animator)
    {
      Debug.LogError("No animator found");
      return;
    }

    float grip = gripAction.action.ReadValue<float>();
    float trigger = triggerAction.action.ReadValue<float>();
    if (currGrip != grip || currTrigger != trigger)
    {
      currGrip = grip;
      currTrigger = trigger;
    }

    animator.SetFloat(animatorGripParam, grip);
    animator.SetFloat(animatorTriggerParam, trigger);
  }
}
