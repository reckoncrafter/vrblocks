using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

public class BlockWalker : MonoBehaviour
{
    BlockSnapping blockSnapping;

    public List<UnityEvent> ActionList;

    void Start(){
        if(blockSnapping == null){
            blockSnapping = this.gameObject.GetComponent<BlockSnapping>();
        }
    }

    public void onActivation(){
        ActionList.Clear();

        GameObject nextBlock = blockSnapping.NextBlockToExecute;
        TurtleCommand turtleCommand = nextBlock.GetComponent<TurtleCommand>();
        UnityEvent onMoveMethod = turtleCommand.onMove;
        ActionList.Add(onMoveMethod);

        while(nextBlock != null){
            nextBlock = nextBlock.GetComponent<BlockSnapping>().NextBlockToExecute;
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
