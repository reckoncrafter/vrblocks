using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HandBoundUIHandler : MonoBehaviour
{
    public Transform errorDialog;
    private TextMeshProUGUI errorDialogTextMesh;

    void Awake()
    {
        errorDialogTextMesh = errorDialog.Find("Canvas/Text").GetComponent<TextMeshProUGUI>();
    }

    public void SetErrorDialog(string content)
    {
        if(content != "")
        {
            errorDialog.gameObject.SetActive(true);
            errorDialogTextMesh.text = content;
        }
        else
        {
            errorDialog.gameObject.SetActive(false);
            errorDialogTextMesh.text = "";
        }
    }
}
