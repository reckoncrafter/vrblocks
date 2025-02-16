using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NewFunctionButton : MonoBehaviour
{
    public Vector3 functionDefinitionBlockOffset = Vector3.zero;
    public Vector3 functionCallBlockOffset = Vector3.zero;
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
        functionDefinitionBlockOffset += transform.position;
        functionCallBlockOffset += transform.position;

        GameObject newFunctionDefinitionInstance = Instantiate(functionDefinitionBlockPrefab, functionDefinitionBlockOffset, Quaternion.identity, transform);
        GameObject newFunctionCallInstance = Instantiate(functionCallBlockPrefab, functionCallBlockOffset, Quaternion.identity, transform);

        newFunctionDefinitionInstance.AddComponent(typeof(FunctionBlock));
        newFunctionDefinitionInstance.AddComponent(typeof(QueueReading));
        FunctionCallBlock fcb = newFunctionCallInstance.AddComponent(typeof(FunctionCallBlock)) as FunctionCallBlock;
        fcb.functionDefinition = newFunctionDefinitionInstance;

        // BROKEN
        var FCLabel = newFunctionCallInstance.GetComponent<Transform>().Find("BlockLabel/LabelText").gameObject.GetComponent<TextMeshPro>();
        var FDLabel = newFunctionDefinitionInstance.GetComponent<Transform>().Find("BlockLabel/LabelText").gameObject.GetComponent<TextMeshPro>();

        FCLabel.text = "Function " + nextFunctionNumber.ToString();
        FDLabel.text = "Call " + (string)nextFunctionNumber.ToString();

        nextFunctionNumber++;

        //newFunctionCallInstance.transform.localScale = blockScale;
        //newFunctionDefinitionInstance.transform.localScale = blockScale;
    }

    public void SetGlowEffect(bool isGlow){
        Renderer renderer = GetComponent<Renderer>();
        if(renderer != null){
            Material material = renderer.material;

            if(isGlow){
                material.SetColor("_EmissionColor", Color.green);
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
