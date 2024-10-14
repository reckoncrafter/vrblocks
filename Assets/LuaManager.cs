using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MoonSharp.Interpreter;

public class LuaManager : MonoBehaviour
{

    private Script luaScript;
    void Start()
    {
        // Load StateTransitions.lua into the MoonSharp interpreter.
        string luaPath = Path.Combine(Application.dataPath, "StateTransitions.lua");
        string luaCode = File.ReadAllText(luaPath);

        luaScript = new Script();
        luaScript.DoString(luaCode);

        // exposing Unity functions to Lua
        luaScript.Globals["UnityFunction"] = (System.Action<string>)UnityFunction;
    }

    // An object should call this function when it wants to trigger a 
    // state transition in the Lua script.
    public void TransitionToState(string stateName){
        luaScript.Call(luaScript.Globals["onTransition"], stateName);
    }

    // This is how Lua will interact with Unity
    private void UnityFunction(string stateName){
        Debug.Log($"UnityFunction called with state: {stateName}");
    }
}
