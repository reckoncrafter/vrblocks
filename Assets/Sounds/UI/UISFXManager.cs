using UnityEngine;
using UnityEngine.UI;

public enum MenuUISFX
{
    MenuUIButton,
    MenuUIButtonDeny,
    MenuUIButtonReturn,
    MenuUIButtonError,
    PlayerInputActionOpen,
    PlayerInputActionClose,
}

public class UISFXManager : MonoBehaviour
{
    
    public AudioClip? menuUIButtonSFX;
    public AudioClip? menuUIButtonErrorSFX;
    public AudioClip? playerInputActionOpenSFX;
    public AudioClip? playerInputActionCloseSFX;
    private AudioSource? uiSFXAudioSource;

    void Start()
    {
        uiSFXAudioSource = GetComponent<AudioSource>();

        Button[] foundUIInteractables = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Toggle[] foundUIToggles = FindObjectsByType<Toggle>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(Button uiInteractable in foundUIInteractables)
        {
            if(uiInteractable.gameObject.CompareTag("MenuUIButton"))
            {
                uiInteractable.onClick.AddListener(() => { PlayUISFX(MenuUISFX.MenuUIButton); });
            }
            else if(uiInteractable.gameObject.CompareTag("MenuUIButtonDeny"))
            {
                uiInteractable.onClick.AddListener(() => { PlayUISFX(MenuUISFX.MenuUIButtonDeny); });
            }
            else if(uiInteractable.gameObject.CompareTag("MenuUIButtonReturn"))
            {
                uiInteractable.onClick.AddListener(() => { PlayUISFX(MenuUISFX.MenuUIButtonReturn); });
            }
            else if(uiInteractable.gameObject.CompareTag("MenuUIButtonError"))
            {
                uiInteractable.onClick.AddListener(() => { PlayUISFX(MenuUISFX.MenuUIButtonError); });
            }
        }
        foreach(Toggle uiToggle in foundUIToggles)
        {
            if(uiToggle.gameObject.CompareTag("MenuUIToggle"))
            {
                uiToggle.onValueChanged.AddListener((bool val) => {if (val){ PlayUISFX(MenuUISFX.MenuUIButton); } else { PlayUISFX(MenuUISFX.MenuUIButtonReturn); }});
            }
        }
    }

    public void PlayUISFX(MenuUISFX sfxType)
    {
        if(sfxType.Equals(MenuUISFX.PlayerInputActionOpen))
        {
            uiSFXAudioSource.clip = playerInputActionOpenSFX;
            uiSFXAudioSource.pitch = Random.Range(0.98f,1.02f);
            uiSFXAudioSource.Play();
        }
        else if(sfxType.Equals(MenuUISFX.PlayerInputActionClose))
        {
            uiSFXAudioSource.clip = playerInputActionCloseSFX;
            uiSFXAudioSource.pitch = Random.Range(0.98f,1.02f);
            uiSFXAudioSource.Play();
        }
        else if(sfxType.Equals(MenuUISFX.MenuUIButton))
        {
            uiSFXAudioSource.clip = menuUIButtonSFX;
            uiSFXAudioSource.pitch = Random.Range(0.98f,1.02f);
            uiSFXAudioSource.Play();
        }
        else if(sfxType.Equals(MenuUISFX.MenuUIButtonReturn) || sfxType.Equals(MenuUISFX.MenuUIButtonDeny) || sfxType.Equals(MenuUISFX.MenuUIButtonError))
        {
            uiSFXAudioSource.clip = menuUIButtonErrorSFX;
            uiSFXAudioSource.pitch = Random.Range(0.98f,1.02f);
            uiSFXAudioSource.Play();
        }
    }
}