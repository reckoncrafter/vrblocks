using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConditionSelectorBlock : MonoBehaviour
{

    string[] dropdownOptions =
    {
        "Is Facing Wall",
        "Is Facing Cliff",
        "Is Facing Step Down",
        "Literal True",
        "Literal False",
    };

    public Dropdown dropdown;
    TurtleCommand turtleCommand;

    void Start()
    {
        turtleCommand = GetComponent<TurtleCommand>();
        dropdown.AddOptions(new List<string>(dropdownOptions) );
        dropdown.onValueChanged.AddListener(delegate {DropdownValueChanged(dropdown);});
    }

    void DropdownValueChanged(Dropdown dd)
    {
        Debug.Log($"Block (Condition): Dropdown Value Changed ({dd.value})");
        turtleCommand.commandEnum = dd.value switch {
            0 => TurtleCommand.Command.ConditionFacingWall,
            1 => TurtleCommand.Command.ConditionFacingCliff,
            2 => TurtleCommand.Command.ConditionFacingStepDown,
            3 => TurtleCommand.Command.ConditionTrue,
            4 => TurtleCommand.Command.ConditionFalse,
            _ => TurtleCommand.Command.CommandError
        };
    }

}
