using Unity.VisualScripting;
using UnityEngine;

public class ToggleXRDeviceManager: MonoBehaviour
{
    public void Awake()
    {
      //TODO: If production build, must default to false
      if(ToggleXRDeviceEmulator.useXRDeviceEmulator){
        this.GameObject().SetActive(true);
      }
      else {
        this.GameObject().SetActive(false);
      }

    }
}