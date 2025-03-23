using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIAudioManager : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioClip hoverSound;
    public AudioClip clickSound;
    private AudioSource audioSource;

    void Start()
    {
        // Get or add an AudioSource component
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }
}
