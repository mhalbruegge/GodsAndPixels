using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

public class VFXPlayer : MonoBehaviour
{
    [SerializeField]
    AudioClip[] audioClips;
    [SerializeField]
    VisualEffect visualEffect;
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    Vector2 pitch;
    [SerializeField]
    float volume = .5f;
    [SerializeField]
    Vector2 audioCooldown;

    bool playing = false;
    float nextAudioTime = 0.0f;

    private void Awake()
    {
        visualEffect.Stop();
    }

    private void Update()
    {
        if (playing && ((nextAudioTime != 0.0f && Time.time > nextAudioTime) || 
                        (nextAudioTime == 0.0f && !audioSource.isPlaying)))
        {
            Debug.Log(this.name);
            audioSource.volume = volume;
            audioSource.pitch = Random.Range(pitch.x, pitch.y);
            audioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Length)]);
            if (audioCooldown.y > 0)
            {
                nextAudioTime = Time.time + Random.Range(audioCooldown.x, audioCooldown.y);
            }
            else
            {
                nextAudioTime = 0.0f;
            }
        }
    }

    public async void Play(float duration)
    {
        if (playing)
            return;

        playing = true;
        visualEffect.Play();

        await Task.Delay((int)(duration * 1000));

        playing = false;
        visualEffect.Stop();

        while(audioSource.isPlaying)
        {
            audioSource.volume = Mathf.Max(0.0f, audioSource.volume - Time.deltaTime);
            if (audioSource.volume == 0.0f)
                audioSource.Stop();

            await Task.Delay(50);
        }
    }
}
