using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NewFunctionButton : MonoBehaviour
{
    public Vector3 functionDefinitionBlockOffset = Vector3.zero;
    public Vector3 functionCallBlockOffset = Vector3.zero;
    public Vector3 blockRotation = new Vector3(0, 180, 0);
    //public Vector3 blockScale = new Vector3(2.5f, 2.25f, 2.5f);
    public int nextFunctionNumber = 0;

    public GameObject functionDefinitionBlockPrefab;
    public GameObject functionCallBlockPrefab;

    public void spawnFunctionBlocks(){
        if(functionCallBlockPrefab == null || functionDefinitionBlockPrefab == null){
            Debug.Log("Function block prefabs are unreferenced.");
            return;
        }

        var transform = GetComponent<Transform>();
        var blockEntity = GameObject.Find("MoveableEntities/BlockEntity").GetComponent<Transform>();

        functionDefinitionBlockOffset += transform.position;
        functionCallBlockOffset += transform.position;

        GameObject newFunctionDefinitionInstance = Instantiate(functionDefinitionBlockPrefab, functionDefinitionBlockOffset, Quaternion.Euler(blockRotation), blockEntity);
        GameObject newFunctionCallInstance = Instantiate(functionCallBlockPrefab, functionCallBlockOffset, Quaternion.Euler(blockRotation), blockEntity);

        newFunctionDefinitionInstance.name = "Block (Function)";
        newFunctionCallInstance.name = "Block (FunctionCall)";

        newFunctionDefinitionInstance.AddComponent(typeof(FunctionBlock));
        newFunctionDefinitionInstance.AddComponent(typeof(QueueReading));
        FunctionCallBlock fcb = newFunctionCallInstance.AddComponent(typeof(FunctionCallBlock)) as FunctionCallBlock;
        fcb.functionDefinition = newFunctionDefinitionInstance;

        // BROKEN
        var FCLabel = newFunctionCallInstance.GetComponent<Transform>().Find("BlockLabel/LabelText").gameObject.GetComponent<TextMeshProUGUI>();
        var FDLabel = newFunctionDefinitionInstance.GetComponent<Transform>().Find("BlockLabel/LabelText").gameObject.GetComponent<TextMeshProUGUI>();

        FDLabel.text = "Function " + nextFunctionNumber.ToString();
        FCLabel.text = "Call " + (string)nextFunctionNumber.ToString();

        newFunctionDefinitionInstance.GetComponent<FunctionBlock>().FunctionID = nextFunctionNumber;

        nextFunctionNumber++;

        //newFunctionCallInstance.transform.localScale = blockScale;
        //newFunctionDefinitionInstance.transform.localScale = blockScale;
    }

    public void SetGlowEffect(bool isGlow){
        Renderer renderer = GetComponent<Renderer>();
        if(renderer != null){
            Material material = renderer.material;

            if(isGlow){
                material.SetColor("_EmissionColor", new Color32(162, 1, 230, 255));
                material.EnableKeyword("_EMISSION");
            }
            else{
                material.SetColor("_EmissionColor", Color.black);
                material.DisableKeyword("_EMISSION");
            }
        }
        else{
            Debug.LogWarning("Renderer not found on GameObject");
        }
    }
}
