using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IfStatement : MonoBehaviour
{

    public bool testCondition = false;
    public TextMeshProUGUI conditionLabel;
    // Start is called before the first frame update
    void Start()
    {
        if(conditionLabel){
            conditionLabel.text = "testCondition";
        }
    }
}
