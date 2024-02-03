using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static void PlayVariedAudio(AudioSource audioSource, AudioClipVarience[] audioClipVarience)
    {
        int clip = Random.Range(0, audioClipVarience.Length);
        float pitchVarience = audioClipVarience[clip].pitchVarience;

        audioSource.volume = audioClipVarience[clip].volume;
        audioSource.pitch = 1 + Random.Range(-pitchVarience, pitchVarience);

        audioSource.PlayOneShot(audioClipVarience[clip].audioClip);
    }
}

[System.Serializable]
public class AudioClipVarience
{
    public AudioClip audioClip;
    public float volume = 1;
    [Range(0, 1)]
    public float pitchVarience = 0.1f;
}

