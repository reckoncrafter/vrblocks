using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class FileHandler : MonoBehaviour
{
    public string filePath;
    public TextMeshPro textMeshPro;
    // Start is called before the first frame update
    void Start()
    {
        if(File.Exists(filePath)){
            string fileContents = File.ReadAllText(filePath);
            textMeshPro.text = fileContents;
        }
        else{
            Debug.LogError("File not found: " + filePath);
        }
    }
}
