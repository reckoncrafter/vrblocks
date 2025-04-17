using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HandBoundUIHandler : MonoBehaviour
{
    public Transform errorDialog;
    private TextMeshProUGUI errorDialogTextMesh;
    public Transform blockCount;
    private TextMeshProUGUI blockCountTextMesh;
    
    public float dialogTimeout = 4.0f;

    void Awake()
    {
        errorDialogTextMesh = errorDialog.Find("Canvas/Text").GetComponent<TextMeshProUGUI>();
        blockCountTextMesh = blockCount.Find("Text").GetComponent<TextMeshProUGUI>();
    }

    public void SetErrorDialog(string content)
    {
        IEnumerator reset()
        {
            yield return new WaitForSeconds(dialogTimeout);
            errorDialog.gameObject.SetActive(false);
            errorDialogTextMesh.text = "";
        }
        errorDialog.gameObject.SetActive(true);
        errorDialogTextMesh.text = content;
        StartCoroutine(reset());
    }

    public void SetBlockCount(int count)
    {
        blockCountTextMesh.text = count.ToString();
    }
}
