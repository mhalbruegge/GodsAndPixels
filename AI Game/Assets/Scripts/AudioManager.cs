using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [Header("Assign Music Clips Here")]
    public AudioClip[] musicClips;     // Drag your music clips in via the Inspector

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Start the coroutine that handles looping random music
        StartCoroutine(PlayMusicLoop());
    }

    private IEnumerator PlayMusicLoop()
    {
        // Continue looping forever
        while (true)
        {
            // Pick a random clip from the array
            AudioClip selectedClip = musicClips[Random.Range(0, musicClips.Length)];

            audioSource.clip = selectedClip;
            audioSource.Play();

            // Wait until the clip finishes
            yield return new WaitForSeconds(selectedClip.length);
        }
    }
}