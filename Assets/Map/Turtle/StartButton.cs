using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class StartButton : MonoBehaviour
{
    public Material disabledMaterial;
    private Material defaultMaterial;
    private new Renderer renderer;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        defaultMaterial = renderer.material;
    }
    public void SetEnabled(bool __enabled){ renderer.material = __enabled ? defaultMaterial : disabledMaterial; }
}
