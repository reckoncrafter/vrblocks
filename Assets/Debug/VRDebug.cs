using UnityEngine;

public class VRDebug : MonoBehaviour
{
  public GameObject UI;
  public GameObject UIAnchor;
  public bool UIActive; // can be changed to button press in the future

  void Update()
  {
    if (UIActive)
    {
      UI.transform.position = UIAnchor.transform.position;
      UI.transform.eulerAngles = new Vector3(UIAnchor.transform.eulerAngles.x, UIAnchor.transform.eulerAngles.y, 0);
    }
  }
}
