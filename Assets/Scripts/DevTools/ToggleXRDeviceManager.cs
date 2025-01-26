using Unity.VisualScripting;
using UnityEngine;

public class ToggleXRDeviceManager: MonoBehaviour
{
    public void Awake()
    {
      if(ToggleXRDeviceEmulator.useXRDeviceEmulator){
        this.GameObject().SetActive(true);
      }
      else {
        this.GameObject().SetActive(false);
      }

    }
}