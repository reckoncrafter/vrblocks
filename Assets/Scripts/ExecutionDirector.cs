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


    private void InstructTurtle(Command cmd)
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
                turtleMovement.FixedUpdate();
                break;
        }
    }

    private void CombinedJump(Command afterJump)
    {
        Debug.Log($"ExecutionDirector.CombinedJump: Jump -> {afterJump}");
        Action func = afterJump switch {
            Command.MoveForward => turtleMovement.PerformWalkForward,
            Command.RotateRight => turtleMovement.PerformRotateRight,
            Command.RotateLeft  => turtleMovement.PerformRotateLeft
        };
        turtleMovement.shouldJump = true;
        turtleMovement.afterJumpAction = func;
        turtleMovement.FixedUpdate();
    }

    private bool EvaluateCondition(Command cmd)
    {
        return cmd switch {
            Command.ConditionFacingWall => turtleMovement.ConditionFacingWall(),
            Command.ConditionFacingCliff => turtleMovement.ConditionFacingCliff(),
            Command.ConditionFacingStepDown => turtleMovement.ConditionFacingStepDown(),
            Command.ConditionTrue => turtleMovement.ConditionTrue(),
            Command.ConditionFalse => turtleMovement.ConditionFalse(),
            _ => false
        };
    }

    private int FindNextBlockOfType(List<GameObject> blockList, int StartIndex, int EndIndex, Command cmd)
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
    }

    private struct ScopeData
    {
        public Command ScopeType;
        public int BeginBlock;
        public List<int> IntermediateBlocks;
        public int EndBlock;
        public bool hasExecuted;
    };

    private class TurtleSyntaxError : Exception{
        public TurtleSyntaxError(string message, int offendingBlockIndex) : base(message)
        {
            Debug.LogError($"Syntax Error at Block: {offendingBlockIndex}");
        }
    };

    public void Execute(List<GameObject> blockList)
    {
        /*
         * Handling Control Flow:
         * I've been trying and iterating on different solutions, and here's what I believe is the answer:
         *
         * Before execution begins, all control flow scopes need to be identified and enclosed.
         * the logic is as follows:
         * We begin walking through the blockList, looking for control flow statements: IfBegin, Else, ElseIf, IfEnd, WhileBegin, WhileEnd.
         *
         * We will maintain a stack that keeps a data structure that represents the current scope.
         * * int BeginBlock;
         * * List<int> IntermediateBlocks; (Else and ElseIf)s
         * * int EndBlock;
         *
         * - Whenever a *Begin is encountered, a new data structure is created, and the block's index is referenced in BeginBlock.
         * - Whenever an Else or ElseIf is encountered, their indexes will be added to the IntermediateBlocks list.
         *      - If the BeginBlock is a WhileBegin, Raise an error.
         * - Whenever a *End is encountered, put its index in EndBlock, then Pop the struct from the stack, and append it to a Dictionary
         * that uses the BeginBlock's index as a key to get the full struct.
         * i.e. Dictionary<GameObject, ScopeStruct>;
         *
         *
         * This done, during normal execution, we can use the struct to implement the following behavior:
         * - Whenever a *Begin is encountered, we can fetch the struct, and get references to the other control blocks that make up its scope.
         * - bool hasExecuted
         * - Stack<ScopeStruct> currentScope
         * - IfBegin(True):
         *     - set hasExecuted true
         *     - proceed normally
         * - IfBegin(False):
         *     - skip index to first IntermediateBlock, or EndIf if none exists.
         * - ElseIf(true):
         *     - if hasExecuted
         *          - skip index to currentScope.EndBlock
         *     - if not hasExecuted
         *          - set hasExecuted true
         *          - proceed normally
         * - ElseIf(false):
         *     - find own index in currentScope.IntermediateBlocks
         *     - skip index to the following IntermediateBlock, or EndIf if none exists.
         * - Else:
         *     - if hasExecuted
         *          - skip index to currentScope.EndBlock
         *     - if not hasExecuted
         *          - set hasExecuted true
         *          - proceed normally
         * - IfEnd:
         *      - Pop currentScope
         */

        // assemble scopes.
        Stack<ScopeData> scopeBuilding = new Stack<ScopeData>();
        Dictionary<int, ScopeData> scopeDict = new Dictionary<int, ScopeData>();
        int sb_index = 0;
        while(sb_index < blockList.Count)
        {
            TurtleCommand blockTurtleCommand;
            bool isCommandOrFlowControl = blockList[sb_index].TryGetComponent<TurtleCommand>(out blockTurtleCommand);
            if(isCommandOrFlowControl)
            {
                Command instruction = blockTurtleCommand.commandEnum;
                if(instruction == Command.IfBegin)
                {
                    ScopeData sd = new ScopeData();
                    sd.IntermediateBlocks = new List<int>();
                    sd.BeginBlock = sb_index;
                    sd.ScopeType = Command.IfBegin;
                    scopeBuilding.Push(sd);
                }
                else if(instruction == Command.WhileBegin)
                {
                    ScopeData sd = new ScopeData();
                    sd.BeginBlock = sb_index;
                    sd.ScopeType = Command.WhileBegin;
                    scopeBuilding.Push(sd);
                }
                else if(instruction == Command.Else || instruction == Command.ElseIf)
                {
                    ScopeData sd = scopeBuilding.Pop();
                    if(sd.ScopeType == Command.WhileBegin)
                        throw new TurtleSyntaxError("Cannot include else or else if blocks in a While loop", sb_index);

                    sd.IntermediateBlocks.Add(sb_index);
                    scopeBuilding.Push(sd);
                }
                else if (instruction == Command.IfEnd)
                {
                    ScopeData sd = scopeBuilding.Pop();
                    if(sd.ScopeType == Command.WhileBegin)
                        throw new TurtleSyntaxError("Cannot end a while loop with an IfEnd", sb_index);
                    sd.EndBlock = sb_index;
                    sd.hasExecuted = false;
                    scopeDict.Add(sd.BeginBlock, sd);

                    Debug.Log($"ExecutionDirector.Execute() [assemble scopes]: begin {sd.BeginBlock}, end {sd.EndBlock}, intermediates: {sd.IntermediateBlocks.Count}");
                }
                else if (instruction == Command.WhileEnd)
                {
                    ScopeData sd = scopeBuilding.Pop();
                    if(sd.ScopeType == Command.IfBegin)
                        throw new TurtleSyntaxError("Cannot end an if statement with a WhileEnd", sb_index);

                    sd.EndBlock = sb_index;
                    sd.hasExecuted = false;
                    scopeDict.Add(sd.BeginBlock, sd);

                    Debug.Log($"ExecutionDirector.Execute() [assemble scopes]: while loop begin {sd.BeginBlock}, end {sd.EndBlock}");
                }
            }
            sb_index++;
        }

        // if anything is left in scopeBuilding, we have incomplete If Statements!


        int index = 0;
        turtleMovement.canReset = true;
        turtleMovement.canFail = true;

        Stack<ScopeData> scopes = new Stack<ScopeData>();

        while(index < blockList.Count)
        {
            TurtleCommand blockTurtleCommand;
            bool isCommandOrFlowControl = blockList[index].TryGetComponent<TurtleCommand>(out blockTurtleCommand);

            FunctionCallBlock functionCallBlock;
            bool isFunctionCall = blockList[index].TryGetComponent<FunctionCallBlock>(out functionCallBlock);

            if (isCommandOrFlowControl)
            {
                Command instruction = blockTurtleCommand.commandEnum;
                Debug.Log($"ExecutionDirector.Execute: instruction = {instruction}");


                if(instruction == Command.IfBegin)
                {
                    TurtleCommand tc;
                    if(blockList[index+1].TryGetComponent<TurtleCommand>(out tc))
                    {
                        bool condition = EvaluateCondition(tc.commandEnum);
                        Debug.Log($"ExecutionDirector.Execute(): IfBegin at {index}: {tc.commandEnum} is {condition}.");
                        ScopeData sd = scopeDict[index];
                        if(condition)
                        {
                            sd.hasExecuted = true;
                            scopes.Push(sd);
                        }
                        else
                        {
                            if(sd.IntermediateBlocks.Count > 0)
                            {
                                Debug.Log($"ExecutionDirector.Execute(): IfBegin(False): Skipping to intermediate block at {sd.IntermediateBlocks[0]}");

                                index = sd.IntermediateBlocks[0];
                                scopes.Push(sd);
                                continue; // do not increment index!
                            }
                            else
                            {
                                Debug.Log($"ExecutionDirector.Execute(): IfBegin(False): Skipping to EndBlock at {sd.EndBlock}");

                                index = sd.EndBlock;
                                scopes.Push(sd);
                                continue;
                            }
                        }
                    }
                }

                else if(instruction == Command.IfEnd)
                {
                    scopes.Pop();
                }

                else if(instruction == Command.Else)
                {
                    ScopeData sd = scopes.Pop();
                    if(sd.hasExecuted)
                    {
                        scopes.Push(sd);
                        index = sd.EndBlock;
                        continue;
                    }
                    else
                    {
                        sd.hasExecuted = true;
                        scopes.Push(sd);
                    }
                }

                else if(instruction == Command.ElseIf)
                {
                    TurtleCommand tc = null;
                    if(blockList[index+1].TryGetComponent<TurtleCommand>(out tc))
                    {
                        bool condition = EvaluateCondition(tc.commandEnum);
                        Debug.Log($"ExecutionDirector.Execute(): ElseIf {tc.commandEnum} is {condition}.");
                        ScopeData sd = scopes.Pop();
                        if(condition)
                        {
                            if(sd.hasExecuted)
                            {
                                scopes.Push(sd);
                                index = sd.EndBlock;
                                continue;
                            }
                            else
                            {
                                sd.hasExecuted = true;
                                scopes.Push(sd);
                            }
                        }
                        else
                        {
                            int nextIntermediate = -1;
                            for(int i = 0; i < sd.IntermediateBlocks.Count; i++)
                            {
                                if(sd.IntermediateBlocks[i] == index)
                                {
                                    if(i+1 < sd.IntermediateBlocks.Count)
                                    {
                                        nextIntermediate = sd.IntermediateBlocks[i+1];
                                    }
                                }
                            }
                            if(nextIntermediate != -1)
                            {
                                index = nextIntermediate;
                                scopes.Push(sd);
                                continue;
                            }
                            else
                            {
                                index = sd.EndBlock;
                                scopes.Push(sd);
                                continue;
                            }
                        }
                    }
                }

                else if(instruction == Command.WhileBegin)
                {
                    TurtleCommand tc;
                    if(blockList[index+1].TryGetComponent<TurtleCommand>(out tc))
                    {
                        bool condition = EvaluateCondition(tc.commandEnum);
                        Debug.Log($"ExecutionDirector.Execute(): WhileBegin at {index}: {tc.commandEnum} is {condition}.");
                        ScopeData sd = scopeDict[index];
                        sd.hasExecuted = true;
                        if(!condition)
                        {
                            sd.hasExecuted = false;
                            index = sd.EndBlock;
                            scopes.Push(sd);
                            continue;
                        }
                        scopes.Push(sd);
                    }
                }

                else if(instruction == Command.WhileEnd)
                {
                    ScopeData sd = scopes.Pop();
                    if(sd.hasExecuted)
                    {
                        index = sd.BeginBlock;
                        continue;
                    }
                }

                else
                {
                    // must be a move instruction
                    if(instruction == Command.Jump)
                    {
                        TurtleCommand tc;
                        if(index + 1 < blockList.Count &&
                           blockList[index+1].TryGetComponent<TurtleCommand>(out tc) &&
                           (tc.commandEnum == Command.MoveForward || tc.commandEnum == Command.RotateRight || tc.commandEnum == Command.RotateLeft)
                          )
                          {
                              CombinedJump(tc.commandEnum);
                              index += 1;
                          }
                          else
                          {
                              InstructTurtle(instruction);
                          }
                    }
                    else
                    {
                        InstructTurtle(instruction);
                    }
                }
            }

            else if (isFunctionCall)
            {

            }


            index++;
        }

        //turtleMovement.Fail();


    }

    public void OnSnapEvent()
    {
        ReadMainBlocks();
        GrabFunctionsInScene();
    }

    void GrabFunctionsInScene(){
        functionBlockLists.Clear();
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
        mainBlockList.Clear();
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
