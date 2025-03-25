using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

using Command = TurtleCommand.Command;

/*
Notes:
- Blocks set to OffendingState will not be reset if they are removed from the stack.
- Because AssembleScopes() is run upon a function call, syntax errors in functions will only be detected when they are called.
- Jumps followed by another action do not cross control flow boundaries.
- This is hard :(
*/
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
        turtleMovement.FailEvent.AddListener(FailHandler);
        turtleMovement.SuccessEvent.AddListener(SuccessHandler);
    }

    public int GetMainListLength(){
        return mainBlockList.Count;
    }


    public void StartButtonPressed()
    {
        //blockList = mainBlockList;
        executionInterrupted = false;
        CallStack.Clear();
        FunctionData mainFunction = new FunctionData();
        mainFunction.blockList = mainBlockList;
        mainFunction.instructionPointer = 0;
        mainFunction.scopeDict = new Dictionary<int, ScopeData>();
        mainFunction.scopes = new Stack<ScopeData>();

        AssembleScopes(mainFunction);
        CallStack.Push(mainFunction);

        turtleMovement.EndOfMovementEvent.AddListener(MainLoop);
        ContinueLoopEvent.AddListener(MainLoop);

        StartCoroutine(RepeatLoop());
    }

    private UnityEvent ContinueLoopEvent = new UnityEvent();
    private IEnumerator RepeatLoop(){
        yield return new WaitForSeconds(1.0f);
        ContinueLoopEvent.Invoke();
    }

    private IEnumerator IlluminateBlock(GameObject block){
        Renderer r = block.GetComponent<Renderer>();
        r.material.SetColor("_EmissionColor", r.material.GetColor("_Color"));
        r.material.EnableKeyword("_EMISSION");

        yield return new WaitForSeconds(1.0f);

        r.material.DisableKeyword("_EMISSION");
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
        bool error(){
            throw new TurtleRuntimeError("Block is not a condition!", CallStack.Peek().blockList[CallStack.Peek().instructionPointer]);
        }
        return cmd switch {
            Command.ConditionFacingWall => turtleMovement.ConditionFacingWall(),
            Command.ConditionFacingCliff => turtleMovement.ConditionFacingCliff(),
            Command.ConditionFacingStepDown => turtleMovement.ConditionFacingStepDown(),
            Command.ConditionTrue => turtleMovement.ConditionTrue(),
            Command.ConditionFalse => turtleMovement.ConditionFalse(),
            _ => error()
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

    private struct FunctionData
    {
        public List<GameObject> blockList;
        public int instructionPointer;
        public Dictionary<int, ScopeData> scopeDict;
        public Stack<ScopeData> scopes;

    }

    private class TurtleSyntaxError : Exception{
        public TurtleSyntaxError(string message, GameObject offendingBlock) : base(message)
        {
            TurtleCommand tc;
            if(offendingBlock.TryGetComponent<TurtleCommand>(out tc))
            {
                tc.SetOffendingState(true);
            }
            Debug.LogError($"Syntax Error at {offendingBlock.name}");
        }
    };

    private class TurtleRuntimeError : Exception{
        public TurtleRuntimeError(string message, GameObject offendingBlock): base(message)
        {
            TurtleCommand tc;
            if(offendingBlock.TryGetComponent<TurtleCommand>(out tc))
            {
                tc.SetOffendingState(true);
            }
            Debug.LogError($"Runtime Error at Block: {offendingBlock.name}");
        }
    }

    private Stack<FunctionData> CallStack = new Stack<FunctionData>();
    private void AssembleScopes(FunctionData function)
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

        function.scopeDict.Clear();
        function.instructionPointer = 0;
        function.scopes.Clear();

        var blockList = function.blockList;

        Stack<ScopeData> scopeBuilding = new Stack<ScopeData>();
        int sb_index = 0;
        while(sb_index < blockList.Count)
        {
            TurtleCommand blockTurtleCommand;
            bool isCommandOrFlowControl = blockList[sb_index].TryGetComponent<TurtleCommand>(out blockTurtleCommand);
            if(isCommandOrFlowControl)
            {
                Command instruction = blockTurtleCommand.commandEnum;
                blockTurtleCommand.SetOffendingState(false);
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
                    if(scopeBuilding.Count == 0)
                        throw new TurtleSyntaxError("Cannot have Else or ElseIf blocks without a preceeding IfBegin", blockList[sb_index]);

                    ScopeData sd = scopeBuilding.Pop();
                    if(sd.ScopeType == Command.WhileBegin)
                        throw new TurtleSyntaxError("Cannot include Else or ElseIf blocks in a While loop", blockList[sb_index]);

                    sd.IntermediateBlocks.Add(sb_index);
                    scopeBuilding.Push(sd);
                }
                else if (instruction == Command.IfEnd)
                {
                    if(scopeBuilding.Count == 0)
                        throw new TurtleSyntaxError("Cannot have an IfEnd block without IfBegin", blockList[sb_index]);
                    ScopeData sd = scopeBuilding.Pop();
                    if(sd.ScopeType == Command.WhileBegin)
                        throw new TurtleSyntaxError("Cannot end a while loop with an IfEnd", blockList[sb_index]);
                    sd.EndBlock = sb_index;
                    sd.hasExecuted = false;
                    function.scopeDict.Add(sd.BeginBlock, sd);

                    Debug.Log($"ExecutionDirector.Execute() [assemble scopes]: begin {sd.BeginBlock}, end {sd.EndBlock}, intermediates: {sd.IntermediateBlocks.Count}");
                }
                else if (instruction == Command.WhileEnd)
                {
                    if(scopeBuilding.Count == 0)
                        throw new TurtleSyntaxError("Cannot have a WhileEnd block without WhileBegin", blockList[sb_index]);
                    ScopeData sd = scopeBuilding.Pop();
                    if(sd.ScopeType == Command.IfBegin)
                        throw new TurtleSyntaxError("Cannot end an if statement with a WhileEnd", blockList[sb_index]);

                    sd.EndBlock = sb_index;
                    sd.hasExecuted = false;
                    function.scopeDict.Add(sd.BeginBlock, sd);

                    Debug.Log($"ExecutionDirector.Execute() [assemble scopes]: while loop begin {sd.BeginBlock}, end {sd.EndBlock}");
                }
            }
            sb_index++;
        }

        if(scopeBuilding.Count > 0)
        {
            throw new TurtleSyntaxError("Incomplete conditional statement detected!", blockList[scopeBuilding.Peek().BeginBlock]);
        }

        // if anything is left in scopeBuilding, we have incomplete If Statements!
    }

    private bool executionInterrupted = false;
    public void SuccessHandler()
    {
        Debug.Log("ExecutionDirector: Success! Halting..");
        turtleMovement.EndOfMovementEvent.RemoveListener(MainLoop);
        ContinueLoopEvent.RemoveAllListeners();
        executionInterrupted = true;
    }
    public void FailHandler()
    {
        Debug.Log("ExecutionDirector: Failure! Halting..");
        turtleMovement.EndOfMovementEvent.RemoveListener(MainLoop);
        ContinueLoopEvent.RemoveAllListeners();
    }

    public void MainLoop()
    {
        if (executionInterrupted) return;

        var function = CallStack.Pop();

        if(function.instructionPointer >= function.blockList.Count)
        {
            if(CallStack.Count > 0)
            {
                // function return
                ContinueLoopEvent.Invoke();
            }
            else
            {
                // no more code
                turtleMovement.EndOfMovementEvent.RemoveListener(MainLoop);
                ContinueLoopEvent.RemoveAllListeners();
                turtleMovement.Fail();
            }
            return;
        }

        turtleMovement.canReset = true;
        turtleMovement.canFail = true;

        TurtleCommand blockTurtleCommand;
        bool isCommandOrFlowControl = function.blockList[function.instructionPointer].TryGetComponent<TurtleCommand>(out blockTurtleCommand);

        FunctionCallBlock functionCallBlock;
        bool isFunctionCall = function.blockList[function.instructionPointer].TryGetComponent<FunctionCallBlock>(out functionCallBlock);

        StartCoroutine(IlluminateBlock(function.blockList[function.instructionPointer]));
        if (isCommandOrFlowControl)
        {
            Command instruction = blockTurtleCommand.commandEnum;
            Debug.Log($"ExecutionDirector.Execute: {function.instructionPointer} {instruction}");


            if(instruction == Command.IfBegin)
            {
                TurtleCommand tc;
                if(function.blockList[function.instructionPointer+1].TryGetComponent<TurtleCommand>(out tc))
                {
                    bool condition = EvaluateCondition(tc.commandEnum);
                    Debug.Log($"ExecutionDirector.Execute(): IfBegin at {function.instructionPointer}: {tc.commandEnum} is {condition}.");
                    ScopeData sd = function.scopeDict[function.instructionPointer];
                    if(condition)
                    {
                        sd.hasExecuted = true;
                        function.instructionPointer += 1; // skip condition block
                        function.scopes.Push(sd);
                    }
                    else
                    {
                        if(sd.IntermediateBlocks.Count > 0)
                        {
                            Debug.Log($"ExecutionDirector.Execute(): IfBegin(False): Skipping to intermediate block at {sd.IntermediateBlocks[0]}");

                            function.instructionPointer = sd.IntermediateBlocks[0];
                            function.scopes.Push(sd);

                            StartCoroutine(RepeatLoop());
                            goto skip; // do not increment function.instructionPointer!
                        }
                        else
                        {
                            Debug.Log($"ExecutionDirector.Execute(): IfBegin(False): Skipping to EndBlock at {sd.EndBlock}");

                            function.instructionPointer = sd.EndBlock;
                            function.scopes.Push(sd);

                            goto skip;
                        }
                    }
                }
                StartCoroutine(RepeatLoop());
            }

            else if(instruction == Command.IfEnd)
            {
                function.scopes.Pop();
                StartCoroutine(RepeatLoop());
            }

            else if(instruction == Command.Else)
            {
                ScopeData sd = function.scopes.Pop();
                if(sd.hasExecuted)
                {
                    function.scopes.Push(sd);
                    function.instructionPointer = sd.EndBlock;

                    goto skip;
                }
                else
                {
                    sd.hasExecuted = true;
                    function.scopes.Push(sd);
                }
                StartCoroutine(RepeatLoop());
            }

            else if(instruction == Command.ElseIf)
            {
                TurtleCommand tc = null;
                if(function.blockList[function.instructionPointer+1].TryGetComponent<TurtleCommand>(out tc))
                {
                    bool condition = EvaluateCondition(tc.commandEnum);
                    Debug.Log($"ExecutionDirector.Execute(): ElseIf {tc.commandEnum} is {condition}.");
                    ScopeData sd = function.scopes.Pop();
                    if(condition)
                    {
                        if(sd.hasExecuted)
                        {
                            function.scopes.Push(sd);
                            function.instructionPointer = sd.EndBlock;

                            StartCoroutine(RepeatLoop());
                            
                            goto skip;
                        }
                        else
                        {
                            sd.hasExecuted = true;
                            function.scopes.Push(sd);
                        }
                    }
                    else
                    {
                        int nextIntermediate = -1;
                        for(int i = 0; i < sd.IntermediateBlocks.Count; i++)
                        {
                            if(sd.IntermediateBlocks[i] == function.instructionPointer)
                            {
                                if(i+1 < sd.IntermediateBlocks.Count)
                                {
                                    nextIntermediate = sd.IntermediateBlocks[i+1];
                                }
                            }
                        }
                        if(nextIntermediate != -1)
                        {
                            function.instructionPointer = nextIntermediate;
                            function.scopes.Push(sd);

                            goto skip;
                        }
                        else
                        {
                            function.instructionPointer = sd.EndBlock;
                            function.scopes.Push(sd);

                            goto skip;
                        }
                    }
                }
                StartCoroutine(RepeatLoop());
            }

            else if(instruction == Command.WhileBegin)
            {
                TurtleCommand tc;
                if(function.blockList[function.instructionPointer+1].TryGetComponent<TurtleCommand>(out tc))
                {
                    bool condition = EvaluateCondition(tc.commandEnum);
                    Debug.Log($"ExecutionDirector.Execute(): WhileBegin at {function.instructionPointer}: {tc.commandEnum} is {condition}.");
                    ScopeData sd = function.scopeDict[function.instructionPointer];
                    sd.hasExecuted = true;
                    if(!condition)
                    {
                        sd.hasExecuted = false;
                        function.instructionPointer = sd.EndBlock;
                        function.scopes.Push(sd);

                        goto skip;
                    }
                    function.instructionPointer += 1; // skip condition block
                    function.scopes.Push(sd);
                }
                StartCoroutine(RepeatLoop());
            }

            else if(instruction == Command.WhileEnd)
            {
                ScopeData sd = function.scopes.Pop();
                if(sd.hasExecuted)
                {
                    function.instructionPointer = sd.BeginBlock;

                    goto skip;
                }

                StartCoroutine(RepeatLoop());
            }

            else if(
                instruction == Command.MoveForward ||
                instruction == Command.RotateRight ||
                instruction == Command.RotateLeft ||
                instruction == Command.Jump
            )
            {
                if(instruction == Command.Jump)
                {
                    TurtleCommand tc;
                    if(function.instructionPointer + 1 < function.blockList.Count &&
                        function.blockList[function.instructionPointer+1].TryGetComponent<TurtleCommand>(out tc) &&
                        (tc.commandEnum == Command.MoveForward || tc.commandEnum == Command.RotateRight || tc.commandEnum == Command.RotateLeft)
                        )
                        {
                            CombinedJump(tc.commandEnum);
                            function.instructionPointer += 1;
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

            else{
                // must be a condition check
                InstructTurtle(instruction);
                StartCoroutine(RepeatLoop());
            }
        }

        else if (isFunctionCall)
        {
            GameObject fd = functionCallBlock.functionDefinition;
            FunctionData nextFunctionData = new FunctionData();
            nextFunctionData.blockList = functionBlockLists[functionCallBlock.FunctionID];
            nextFunctionData.instructionPointer = 0;
            nextFunctionData.scopeDict = new Dictionary<int, ScopeData>();
            nextFunctionData.scopes = new Stack<ScopeData>();
            
            AssembleScopes(nextFunctionData);

            function.instructionPointer++;
            CallStack.Push(function);
            CallStack.Push(nextFunctionData);
            StartCoroutine(RepeatLoop());
            return;
        }


        function.instructionPointer++;
        CallStack.Push(function);
        return;

        skip:
        StartCoroutine(RepeatLoop());
        CallStack.Push(function);
        return;
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
