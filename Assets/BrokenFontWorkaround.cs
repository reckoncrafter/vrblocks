using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class BrokenFontWorkaround : MonoBehaviour
{
    public Font replacementFont;
    void Awake()
    {
        Text[] allTextComponents = GetComponentsInChildren<Text>(true);
        for(int i = 0; i < allTextComponents.Length; i++)
        {
            allTextComponents[i].font = replacementFont;
        }
    }
}
