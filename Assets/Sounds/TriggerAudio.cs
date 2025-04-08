using UnityEngine;

public class TriggerAudio : MonoBehaviour
{
    private AudioSource audio;
    private DeletionBoundary deletionBoundary;
    void Start()
    {
        audio = GetComponent<AudioSource>();
        if(TryGetComponent(out deletionBoundary))
        {
            deletionBoundary.objectDestroyedEvent.AddListener(OnObjectDestroyed);
        }
    }

    void OnObjectDestroyed()
    {
        audio.Play();
    }
}
