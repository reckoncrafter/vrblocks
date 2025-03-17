using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConditionSelectorBlock : MonoBehaviour
{

    string[] dropdownOptions =
    {
        "Is Facing Wall",
        "Literal True",
        "Literal False",
    };

    public Dropdown dropdown;
    TurtleCommand turtleCommand;
    TurtleMovement turtleMovement;

    void Start()
    {
        turtleCommand = GetComponent<TurtleCommand>();
        dropdown.AddOptions(new List<string>(dropdownOptions) );
        dropdown.onValueChanged.AddListener(delegate {DropdownValueChanged(dropdown);});
    }

    void DropdownValueChanged(Dropdown dd)
    {
        Debug.Log($"Block (Condition): Dropdown Value Changed ({dd.value})");
        turtleMovement = turtleCommand.turtleMovement;
        turtleCommand.onMove.RemoveAllListeners();
        turtleCommand.onMove.AddListener(dd.value switch {
            0 => turtleMovement.ConditionFacingWall,
            1 => turtleMovement.setConditionTrue,
            2 => turtleMovement.setConditionFalse,
            _ => () => {}
        });
    }

}
