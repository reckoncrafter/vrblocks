using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleToText : MonoBehaviour
{
  public Text debugText;
  string output = "";
  string stack = "";

  private int lineCount = 0;

  private void OnEnable()
  {
    Application.logMessageReceived += HandleLog;
    Debug.Log("Console Enabled");
  }

  private void OnDisable()
  {
    Application.logMessageReceived -= HandleLog;
    ClearLog();
  }

  void HandleLog(string logString, string stackTrace, LogType type)
  {
    if (lineCount > 2)
    {
      ClearLog();
      lineCount = 0;
    }
    output += logString + "\n" + output;
    stack = stackTrace;
    OnGUI();
  }

  private void OnGUI()
  {
    debugText.text = output;
  }

  public void ClearLog()
  {
    output = "";
    lineCount = 0;
    // debugText.Rebuild(CanvasUpdate.PreRender);
    debugText.text = output;
  }
}
