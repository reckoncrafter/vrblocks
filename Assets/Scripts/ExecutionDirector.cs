using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

using Command = TurtleCommand.Command;

/*
Notes:
- Because AssembleScopes() is run upon a function call, syntax errors in functions will only be detected when they are called.
- Jumps followed by another action do not cross control flow boundaries.
- This is hard :(
*/
public class ExecutionDirector : MonoBehaviour
{
    public TurtleMovement turtleMovement;
    public GameObject startBlock;
    public GameObject startButton;
    public HandBoundUIHandler handBoundUI;

    // maintains a list of the blocks under Block (StartQueue)
    private List<GameObject> mainBlockList = new List<GameObject>();

    // Function IDs map to separate lists of blocks under their respective Function Definition blocks.
    private Dictionary<int, List<GameObject>> functionBlockLists = new Dictionary<int, List<GameObject>>();

    void Start()
    {
        // The execution director must begin execution (StartButtonPressed), and maintain an internal representation of the list of blocks (OnSnapEvent)
        startButton.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(StartButtonPressed);
        BlockSnapping.blockSnapEvent.AddListener(OnSnapEvent);

        GrabFunctionsInScene();
        turtleMovement.FailEvent.AddListener(FailHandler);
        turtleMovement.SuccessEvent.AddListener(SuccessHandler);
        turtleMovement.ResetEvent.AddListener(ResetStartButton);

        if(handBoundUI == null){ handBoundUI = FindObjectOfType<HandBoundUIHandler>(); }
    }

    public void StartButtonPressed(SelectEnterEventArgs selectEnter)
    {
        // initialize data structures
        executionInterrupted = false;
        CallStack.Clear();
        FunctionData mainFunction = new FunctionData();
        mainFunction.blockList = mainBlockList;
        mainFunction.instructionPointer = 0;
        mainFunction.scopeDict = new Dictionary<int, ScopeData>();
        mainFunction.scopes = new Stack<ScopeData>();

        // setup and begin execution
        AssembleScopes(mainFunction);
        CallStack.Push(mainFunction);

        ContinueLoopEvent.AddListener(MainLoop);
        StartCoroutine(RepeatLoop(0.5f));

        // disable start button
        if(mainBlockList.Count > 0){
            startButton.GetComponent<StartButton>().SetEnabled(false);
            startButton.GetComponent<XRSimpleInteractable>().selectEntered.RemoveAllListeners();
        }

        // clear old error messages
        GameObject failureDialog = GameObject.Find("/FailureDialog");
        if(failureDialog != null)
        {
            failureDialog.GetComponentInChildren<TextMeshProUGUI>().text = "";
        }

        // clear offending state material from all blocks in scene
        TurtleCommand[] turtleCommands = FindObjectsOfType<TurtleCommand>();
        foreach(TurtleCommand tc in turtleCommands)
        {
            tc.SetOffendingState(false);
        }

        // clear error dialog
        if(handBoundUI != null){ handBoundUI.SetErrorDialog(""); }
    }

    private void ResetStartButton()
    {
        startButton.GetComponent<StartButton>().SetEnabled(true);
        startButton.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(StartButtonPressed);
    }

    private UnityEvent ContinueLoopEvent = new UnityEvent();
    private IEnumerator RepeatLoop(float time){
        yield return new WaitForSeconds(time);
        ContinueLoopEvent.Invoke();
    }

    private IEnumerator IlluminateBlock(GameObject block, float time){
        Renderer r = block.GetComponent<Renderer>();
        r.material.SetColor("_EmissionColor", r.material.GetColor("_Color"));
        r.material.EnableKeyword("_EMISSION");

        yield return new WaitForSeconds(time);

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

    private bool EvaluateCondition(Command cmd, bool isInverted)
    {
        // Note: Should create error on non-conditional Command
        bool status = cmd switch {
            Command.ConditionFacingWall => turtleMovement.ConditionFacingWall(),
            Command.ConditionFacingCliff => turtleMovement.ConditionFacingCliff(),
            Command.ConditionFacingStepDown => turtleMovement.ConditionFacingStepDown(),
            Command.ConditionTrue => turtleMovement.ConditionTrue(),
            Command.ConditionFalse => turtleMovement.ConditionFalse(),
            _ => false
        };
        return isInverted ? !status : status;
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
            HandBoundUIHandler handBoundUI = FindObjectOfType<HandBoundUIHandler>();
            if(handBoundUI != null)
            {
                handBoundUI.SetErrorDialog(message);
            }
        
            Debug.LogError($"Syntax Error at {offendingBlock.name}");
        }
    };

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
                //blockTurtleCommand.SetOffendingState(false);
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
                else if (instruction == Command.WhileBreak)
                {
                    if(scopeBuilding.Count == 0)
                    {
                        throw new TurtleSyntaxError("Cannot have a break outside of a while loop.", blockList[sb_index]);
                    }
                    else
                    {
                        ScopeData sd = scopeBuilding.Peek();
                        if(sd.ScopeType == Command.IfBegin)
                        {
                            throw new TurtleSyntaxError("Cannot have a break inside an if statement", blockList[sb_index]);
                        }
                    }
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
        ContinueLoopEvent.RemoveAllListeners();
        executionInterrupted = true;
    }
    public void FailHandler()
    {
        Debug.Log("ExecutionDirector: Failure! Halting..");
        ContinueLoopEvent.RemoveAllListeners();
    }

    readonly float defaultWait = 1.0f;
    readonly float moveWait = 1.8f;
    readonly float jumpWait = 2.3f;

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

        if (isCommandOrFlowControl)
        {
            Command instruction = blockTurtleCommand.commandEnum;
            Debug.Log($"ExecutionDirector.Execute: {function.instructionPointer} {instruction}");


            if(instruction == Command.IfBegin)
            {
                StartCoroutine(IlluminateBlock(function.blockList[function.instructionPointer], defaultWait));

                TurtleCommand tc;
                ConditionSelectorBlock csb;
                bool invertedStatus = false;

                if(function.blockList[function.instructionPointer+1].TryGetComponent<ConditionSelectorBlock>(out csb))
                {
                    invertedStatus = csb.isInverted;
                }

                if(function.blockList[function.instructionPointer+1].TryGetComponent<TurtleCommand>(out tc))
                {
                    StartCoroutine(IlluminateBlock(function.blockList[function.instructionPointer+1], defaultWait));
                    bool condition = EvaluateCondition(tc.commandEnum, invertedStatus);
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

                StartCoroutine(RepeatLoop(defaultWait));
            }

            else if(instruction == Command.IfEnd)
            {
                StartCoroutine(IlluminateBlock(function.blockList[function.instructionPointer], defaultWait));
                function.scopes.Pop();
                StartCoroutine(RepeatLoop(defaultWait));
            }

            else if(instruction == Command.Else)
            {
                StartCoroutine(IlluminateBlock(function.blockList[function.instructionPointer], defaultWait));
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
                StartCoroutine(RepeatLoop(defaultWait));
            }

            else if(instruction == Command.ElseIf)
            {
                StartCoroutine(IlluminateBlock(function.blockList[function.instructionPointer], defaultWait));

                TurtleCommand tc;
                ConditionSelectorBlock csb;
                bool invertedStatus = false;

                if(function.blockList[function.instructionPointer+1].TryGetComponent<ConditionSelectorBlock>(out csb))
                {
                    invertedStatus = csb.isInverted;
                }

                if(function.blockList[function.instructionPointer+1].TryGetComponent<TurtleCommand>(out tc))
                {
                    StartCoroutine(IlluminateBlock(function.blockList[function.instructionPointer+1], defaultWait));
                    bool condition = EvaluateCondition(tc.commandEnum, invertedStatus);
                    Debug.Log($"ExecutionDirector.Execute(): ElseIf {tc.commandEnum} is {(invertedStatus ? "not" : "")} {condition}.");
                    ScopeData sd = function.scopes.Pop();
                    if(condition)
                    {
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
                StartCoroutine(RepeatLoop(defaultWait));
            }

            else if(instruction == Command.WhileBegin)
            {
                StartCoroutine(IlluminateBlock(function.blockList[function.instructionPointer], defaultWait));

                TurtleCommand tc;
                ConditionSelectorBlock csb;
                bool invertedStatus = false;

                if(function.blockList[function.instructionPointer+1].TryGetComponent<ConditionSelectorBlock>(out csb))
                {
                    invertedStatus = csb.isInverted;
                }

                if(function.blockList[function.instructionPointer+1].TryGetComponent<TurtleCommand>(out tc))
                {
                    StartCoroutine(IlluminateBlock(function.blockList[function.instructionPointer+1], defaultWait));
                    bool condition = EvaluateCondition(tc.commandEnum, invertedStatus);
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
                StartCoroutine(RepeatLoop(defaultWait));
            }

            else if(instruction == Command.WhileBreak)
            {
                StartCoroutine(IlluminateBlock(function.blockList[function.instructionPointer], defaultWait));
                ScopeData sd = function.scopes.Pop();
                sd.hasExecuted = false;
                function.scopes.Push(sd);
                function.instructionPointer = sd.EndBlock;
                StartCoroutine(RepeatLoop(defaultWait));
                goto skip;
            }

            else if(instruction == Command.WhileEnd)
            {
                StartCoroutine(IlluminateBlock(function.blockList[function.instructionPointer], defaultWait));

                ScopeData sd = function.scopes.Pop();
                if(sd.hasExecuted)
                {
                    function.instructionPointer = sd.BeginBlock;

                    goto skip;
                }

                StartCoroutine(RepeatLoop(defaultWait));
            }

            else if(
                instruction == Command.MoveForward ||
                instruction == Command.RotateRight ||
                instruction == Command.RotateLeft ||
                instruction == Command.Jump
            )
            {
                StartCoroutine(IlluminateBlock(function.blockList[function.instructionPointer], (instruction == Command.Jump) ? jumpWait : moveWait));
                InstructTurtle(instruction);
                StartCoroutine(RepeatLoop((instruction == Command.Jump) ? jumpWait : moveWait));
            }

            else{
                // must be a lone condition check (this will do nothing)
                StartCoroutine(IlluminateBlock(function.blockList[function.instructionPointer], defaultWait));
                InstructTurtle(instruction);
                StartCoroutine(RepeatLoop(defaultWait));
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
            StartCoroutine(RepeatLoop(defaultWait));
            return;
        }


        function.instructionPointer++;
        CallStack.Push(function);
        return;

        skip:
        StartCoroutine(RepeatLoop(defaultWait));
        CallStack.Push(function);
        return;
    }

    public void OnSnapEvent()
    {
        ReadMainBlocks();
        GrabFunctionsInScene();

        int blocksAllQueues = mainBlockList.Count;
        foreach(var fbl in functionBlockLists)
        {
            blocksAllQueues += fbl.Value.Count;
        }
        handBoundUI.SetBlockCount(blocksAllQueues);
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
