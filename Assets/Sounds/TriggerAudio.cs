using UnityEngine;

public class TriggerAudio : MonoBehaviour
{
    private AudioSource audio;
    void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    void OnTriggerEnter()
    {
        audio.Play();
    }
}
