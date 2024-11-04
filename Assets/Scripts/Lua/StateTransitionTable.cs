using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class StateTransitionTable : MonoBehaviour
{
    // This file takes the children of the object that it is attached to,
    // and presuming that each child has a "State" component, will generate
    // a template Lua file with a state transition table.
    void Start()
    {
        GenerateLuaFile();
    }

    private void GenerateLuaFile(){
        string luaContent = "StateTransitions = {\n";
        foreach (Transform child in transform)
        {
            State state = child.GetComponent<State>();
            if (state != null)
            {
                luaContent += $"  [{state.stateName}] = {{\n";
                luaContent += "    transitions = {\n";
                foreach (string transition in state.transitions)
                {
                    luaContent += $"      '{transition}',\n";
                }
                luaContent += "    },\n";
                luaContent += "  },\n";
            }
        }
        luaContent += "}\n";

        luaContent += "\nfunction onTransition(stateName)\nprint(\"Transition: \" .. stateName)\nUnityFunction(stateName)\nend";

        string path = Path.Combine(Application.dataPath, "StateTransitions.lua");
        File.WriteAllText(path, luaContent);
        Debug.Log("Lua file generated at: " + path);
    }
}
