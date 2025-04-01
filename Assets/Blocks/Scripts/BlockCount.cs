using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BlockCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI? blockCountText; // Attach TMP text in BlockCount prefab
    public void SetBlockCount(int count)
    {
        blockCountText.text = $"Block Count: {count}";
    }
}
