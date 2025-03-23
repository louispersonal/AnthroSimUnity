using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource; // Assign in Inspector or fetch in Awake()
    public AudioClip[] playlist; // Assign tracks in Inspector
    private int currentTrackIndex = 0;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (playlist.Length > 0)
        {
            PlayTrack(currentTrackIndex);
        }
    }

    void Update()
    {
        if (!audioSource.isPlaying) // When track ends
        {
            PlayNextTrack();
        }
    }

    public void PlayTrack(int index)
    {
        if (index >= 0 && index < playlist.Length)
        {
            currentTrackIndex = index;
            audioSource.clip = playlist[index];
            audioSource.Play();
        }
    }

    public void PlayNextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % playlist.Length; // Loop back to start
        PlayTrack(currentTrackIndex);
    }
}