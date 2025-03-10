using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoToBlock : MonoBehaviour
{
    public TextMeshProUGUI parameterLabel;
    public int JumpToLine;
    // Start is called before the first frame update
    void Start()
    {
        //parameterLabel = GameObject.Find("BlockLabelParameter/LabelText").GetComponent<TextMeshProUGUI>();

        parameterLabel.text = JumpToLine.ToString();
    }
}
