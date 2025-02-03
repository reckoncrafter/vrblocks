using UnityEngine;
using UnityEditor;

public class ToggleXRDeviceEmulator: MonoBehaviour
{
  const string useXRDeviceMenu = "DevTools/Toggle XR Device Emulator";
  public static bool useXRDeviceEmulator { get; set; }

  static ToggleXRDeviceEmulator()
  {
      useXRDeviceEmulator = EditorPrefs.GetBool(useXRDeviceMenu, false);
  }

  [MenuItem(useXRDeviceMenu)]
  static void ToggleEmulator() 
  {
      useXRDeviceEmulator = !useXRDeviceEmulator;
      EditorPrefs.SetBool(useXRDeviceMenu, useXRDeviceEmulator);
  }

  [MenuItem(useXRDeviceMenu, true)]
  static bool ToggleEmulatorValidate()
  {
      Menu.SetChecked("DevTools/Toggle XR Device Emulator", useXRDeviceEmulator);
      return true;
  }
}