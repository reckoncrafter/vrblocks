using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ControlTutorialItem : MonoBehaviour
{
    public Gradient lineGradient;
    public Material lineMaterial;
    int joints = 0;
    Transform lineOrigin;
    LineRenderer line;
    void Start()
    {
        lineOrigin = transform.Find("LineOrigin");
        if(lineOrigin is not null)
        {
            joints = 1 + lineOrigin.childCount; // lineOrigin + children
            line = gameObject.AddComponent<LineRenderer>();
            line.alignment = LineAlignment.View;
            line.useWorldSpace = true;
            line.startWidth = 0.001f;
            line.endWidth = 0.001f;
            line.positionCount = joints;
            line.colorGradient = lineGradient;
            line.material = lineMaterial;

            lineOrigin.gameObject.SetActive(false);
            foreach(Transform child in lineOrigin)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if(line is not null)
        {
            line.SetPosition(0, lineOrigin.position);
            for(int i = 1; i < joints; i++)
            {
                line.SetPosition(i, lineOrigin.GetChild(i-1).position);
            }
        }
    }
}
