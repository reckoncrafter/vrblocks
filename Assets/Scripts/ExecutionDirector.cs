using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

using Command = TurtleCommand.Command;

public class ExecutionDirector : MonoBehaviour
{
    public TurtleMovement turtleMovement;
    public GameObject startBlock;
    public GameObject startButton;

    // maintains a list of the blocks under Block (StartQueue)
    private List<GameObject> mainBlockList = new List<GameObject>();

    // Function IDs map to separate lists of blocks under their respective Function Definition blocks.
    private Dictionary<int, List<GameObject>> functionBlockLists = new Dictionary<int, List<GameObject>>();


    void Start()
    {
        // The execution director must begin execution (StartButtonPressed), and maintain an internal representation of the list of blocks (OnSnapEvent)
        startButton.GetComponent<XRSimpleInteractable>().onSelectEntered.AddListener((XRBaseInteractor xbi) => StartButtonPressed());
        BlockSnapping.blockSnapEvent.AddListener(OnSnapEvent);

        GrabFunctionsInScene();
    }

    public int GetMainListLength(){
        return mainBlockList.Count;
    }


    public void StartButtonPressed()
    {
        Execute(mainBlockList);
    }



    public void Execute(List<GameObject> blockList)
    {
        /*
         * Proceed through the mainBlockList and take the following paths:
         * - If the block is a movement instruction, like Move Forward, Rotate Left, etc, send the command to the turtle.
         *
         * Control flow blocks must be dealt with carefully, fortunately with the list, the implementation is simpler
         * - If, no else. Proceed normally on true. | Move index to next IfStatementEnd on false. (If no IfStatementEnd, trigger warning)
         * - If, with else. On true, proceed until Else, then move index to IfStatementEnd. | On false, move index to Else, proceed normally.
         * - If, with else if(s), no else. On true, proceed until **first** ElseIf, then skip to IfStatementEnd. | On False, skip to **first** ElseIf.
         * - Else If. On true, proceed to next ElseIf, otherwise to IfStatementEnd. | On false, skip to next ElseIf, otherwise to Else, otherwise to IfStatementEnd.
         *
         * - While. Evaluate condition, proceed if true, then repeat. Otherwise, move index to WhileStatementEnd. (If no WhileStatementEnd, trigger warning)
         *
         * Function blocks also have special behavior. When a function call is encountered, recurse the execution function using the associated list in functionBlockLists.
         * This means that the Execution function should take a List<GameObject> as an argument, beginning with Execute(mainBlockList);
         *
         * !!!! Jumping requires special consideration as it can be combined with the next action (e.g. Move Forward) or stand alone.
         * If Jump is followed with another movement instruction, pass it as as an Action to TurtleMovement.PerformJump(Action), and then skip it.
         * Otherwise, pass it an empty function.
         */

        Action<Command> InstructTurtle = (Command cmd) =>
        {
            switch(cmd)
            {
                case Command.MoveForward:
                    turtleMovement.PerformWalkForward();
                    break;
                case Command.RotateRight:
                    turtleMovement.PerformRotateRight();
                    break;
                case Command.RotateLeft:
                    turtleMovement.PerformRotateLeft();
                    break;
                case Command.Jump:
                    turtleMovement.shouldJump = true;
                    turtleMovement.afterJumpAction = () => {}; // PUT SOMETHING HERE!
                    turtleMovement.FixedUpdate();
                    break;
            }
        };

        Func<Command, bool> EvaluateCondition = (Command cmd) =>
        {
            return cmd switch {
                Command.ConditionFacingWall => turtleMovement.ConditionFacingWall(),
                Command.ConditionFacingCliff => turtleMovement.ConditionFacingCliff(),
                Command.ConditionFacingStepDown => turtleMovement.ConditionFacingStepDown(),
                Command.ConditionTrue => turtleMovement.ConditionTrue(),
                Command.ConditionFalse => turtleMovement.ConditionFalse(),
                _ => false
            };
        };

        Func<int, int, Command, int> FindNextBlockOfType = (int StartIndex, int EndIndex, Command cmd) =>
        {
            int index = StartIndex;
            while(index <= EndIndex)
            {
                if(blockList[index].GetComponent<TurtleCommand>().commandEnum == cmd){
                    return index;
                }
                index++;
            }
            return -1;
        };

        int index = 0;

        bool statusInIf = false;
        bool statusInWhile = false;
        bool statusCurrentCondition = false;

        while(index <= blockList.Count)
        {
            TurtleCommand currentBlockTurtleCommand = blockList[index].GetComponent<TurtleCommand>();
            if(currentBlockTurtleCommand == null)
            {
                Debug.Log("ExecutionDirector.Execute(): TurtleCommand not found on block.");
            }
            Command instruction = currentBlockTurtleCommand.commandEnum;

            if(instruction == Command.IfBegin)
            {
                GameObject conditionBlock = blockList[index + 1];
                bool condition = EvaluateCondition(conditionBlock.GetComponent<TurtleCommand>().commandEnum);
                statusInIf = true;
                statusCurrentCondition = condition;

                int EndIfIndex = FindNextBlockOfType(index, blockList.Count, Command.IfEnd);
                if(EndIfIndex != -1)
                {
                    if(!condition)
                    {
                        int ElseIfIndex = FindNextBlockOfType(index, EndIfIndex, Command.ElseIf);
                        int ElseIndex = FindNextBlockOfType(index, EndIfIndex, Command.Else);
                        if (ElseIfIndex != -1)
                        {
                            index = ElseIfIndex;
                        }
                        else if (ElseIndex != -1)
                        {
                            index = ElseIfIndex;
                        }
                        else
                        {
                            index = EndIfIndex;
                        }
                    }
                }
                else
                {
                    Debug.Log("ExecutionDirector.Execute(): No EndIfStatement.");
                }


            }

            else if(instruction == Command.IfEnd)
            {
                statusInIf = false;
            }

            if(instruction == Command.Else)
            {
                if(statusCurrentCondition)
                {
                    index = FindNextBlockOfType(index, blockList.Count, Command.IfEnd);
                }
            }
        }


    }

    public void OnSnapEvent()
    {
        ReadMainBlocks();
        GrabFunctionsInScene();
    }

    void GrabFunctionsInScene(){
        FunctionBlock[] functionBlocks = FindObjectsOfType(typeof(FunctionBlock)) as FunctionBlock[];
        foreach(FunctionBlock fb in functionBlocks)
        {
            Debug.Log($"ExecutionDirector.GrabFunctionsInScene(): Added Function ID {fb.FunctionID} to list of functions");
            functionBlockLists[fb.FunctionID] = ReadFunctionBlocks(fb.gameObject);
        }
    }


    void ReadMainBlocks()
    {
        Debug.Log("ExecutionDirector.ReadMainBlocks()");
        SnappedForwarding snappedForwarding = startBlock.GetComponentInChildren<SnappedForwarding>();
        if(snappedForwarding == null)
        {
            Debug.Log("ExecutionDirector.ReadMainBlocks(): no SnappedForwarding");
            return;
        }
        else if(snappedForwarding.ConnectedBlock == null)
        {
            Debug.Log("ExecutionDirector.ReadMainBlocks(): no ConnectedBlock");
            return;
        }
        GameObject currentBlock = snappedForwarding.ConnectedBlock;
        while(currentBlock != null)
        {
            mainBlockList.Add(currentBlock);
            currentBlock = currentBlock.GetComponentInChildren<SnappedForwarding>().ConnectedBlock;
        }

        foreach(GameObject block in mainBlockList)
        {
            Debug.Log($"ExecutionDirector.ReadMainBlocks: {block.name}");
        }
    }

    List<GameObject> ReadFunctionBlocks(GameObject functionDefinition)
    {
        List<GameObject> functionList = new List<GameObject>();
        SnappedForwarding snappedForwarding = functionDefinition.GetComponentInChildren<SnappedForwarding>();
        GameObject currentBlock = snappedForwarding.ConnectedBlock;
        while(currentBlock != null)
        {
            functionList.Add(currentBlock);
            currentBlock = currentBlock.GetComponentInChildren<SnappedForwarding>().ConnectedBlock;
        }

        foreach(GameObject block in functionList)
        {
            Debug.Log($"ExecutionDirector.ReadFunctionBlocks: {block.name}");
        }

        return functionList;
    }




}
