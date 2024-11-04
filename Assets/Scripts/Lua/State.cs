using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class State : MonoBehaviour
{
    [System.Serializable]
    public struct Transition{        
        public GameObject NextObject;
        public UnityEvent Action;
    }
    public string stateName;
    public Transition[] transitions;
}
