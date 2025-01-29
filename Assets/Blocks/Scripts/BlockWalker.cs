using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

public class BlockWalker : MonoBehaviour
{
    private SnappedForwarding snappedForwarding;
    public List<UnityEvent> ActionList;

    void Start(){
        if(snappedForwarding == null){
            GameObject SnapTriggerBottom = this.transform.Find("SnapTriggerBottom").gameObject;
            snappedForwarding = SnapTriggerBottom.GetComponent<SnappedForwarding>();
            Debug.Log(snappedForwarding);
        }
    }

    public void onActivation(){
        ActionList.Clear();

        GameObject nextBlock = snappedForwarding.ConnectedBlock;
        TurtleCommand turtleCommand = nextBlock.GetComponent<TurtleCommand>();
        UnityEvent onMoveMethod = turtleCommand.onMove;
        ActionList.Add(onMoveMethod);

        while(nextBlock != null){
            nextBlock = nextBlock.transform.Find("SnapTriggerBottom").gameObject.GetComponent<SnappedForwarding>().ConnectedBlock;
            if(nextBlock != null){
                turtleCommand = nextBlock.GetComponent<TurtleCommand>();
                onMoveMethod = turtleCommand.onMove;
                ActionList.Add(onMoveMethod);
            }
        }
    }
}

[CustomEditor(typeof(BlockWalker))]
public class BlockWalkerMenu : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        BlockWalker blockWalker = (BlockWalker)target;
        if (GUILayout.Button("Perform Walk"))
        {
            blockWalker.onActivation();
        }
    }
}
