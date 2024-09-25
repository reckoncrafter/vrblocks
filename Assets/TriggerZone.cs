using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
  public string targetTag;
  public UnityEvent<GameObject> OnEnterEvent;

  private void onTriggerEnter(Collider collider)
  {
    if (collider.gameObject.tag == targetTag)
    {
      OnEnterEvent.Invoke(collider.gameObject);
    }
  }
}
