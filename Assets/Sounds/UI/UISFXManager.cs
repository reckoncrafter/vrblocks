using UnityEngine;
using UnityEngine.UI;

public class UISFXManager : MonoBehaviour
{
    
    public AudioClip? menuUIButtonSFX;
    public AudioClip? menuUIButtonErrorSFX;
    private AudioSource? uiSFXAudioSource;

    void Start()
    {
        uiSFXAudioSource = GetComponent<AudioSource>();

        Button[] foundUIInteractables = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(Button uiInteractable in foundUIInteractables)
        {
            if(uiInteractable.gameObject.CompareTag("MenuUIButton"))
            {
                uiInteractable.onClick.AddListener(() => {
                    uiSFXAudioSource.clip = menuUIButtonSFX;
                    uiSFXAudioSource.pitch = Random.Range(0.98f,1.02f);
                    uiSFXAudioSource.Play();
                });
            }
            else if(uiInteractable.gameObject.CompareTag("MenuUIButtonDeny"))
            {
                uiInteractable.onClick.AddListener(() => {
                    uiSFXAudioSource.clip = menuUIButtonErrorSFX;
                    uiSFXAudioSource.pitch = Random.Range(0.98f,1.02f);
                    uiSFXAudioSource.Play();
                });
            }
            else if(uiInteractable.gameObject.CompareTag("MenuUIButtonReturn"))
            {
                uiInteractable.onClick.AddListener(() => {
                    uiSFXAudioSource.clip = menuUIButtonErrorSFX;
                    uiSFXAudioSource.pitch = Random.Range(0.98f,1.02f);
                    uiSFXAudioSource.Play();
                });
            }
            else if(uiInteractable.gameObject.CompareTag("MenuUIButtonError"))
            {
                uiInteractable.onClick.AddListener(() => {
                    uiSFXAudioSource.clip = menuUIButtonErrorSFX;
                    uiSFXAudioSource.pitch = Random.Range(0.98f,1.02f);
                    uiSFXAudioSource.Play();
                });
            }
        }
    }
}