using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIAudioManager : MonoBehaviour
{
    public AudioClip hoverSound;
    public AudioClip clickSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    public void PlayHoverSound()
    {
        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }


    public void PlayClickSound()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}
