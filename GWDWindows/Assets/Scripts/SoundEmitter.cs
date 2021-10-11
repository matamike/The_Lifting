using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

[RequireComponent( typeof(AudioSource) )]
public class SoundEmitter : MonoBehaviour
{
    // Public variables
    [Range(0, 1)]
    public float pitchDiff;
    public AudioClip[] sounds;

    // Private variables
    private AudioSource _as;
    private bool played = false;
    private float initialPitch;

    void Start()
    {
        _as = GetComponent<AudioSource>();
        initialPitch = _as.pitch;
    }

    private void Update()
    {
        // reset pitch when done playing
        if (!_as.isPlaying && played)
        {
            _as.pitch = initialPitch;
        }
    }

    public void PlayClickSound(int i)
    {
        // reset pitc, set played flag and change pitch
        _as.pitch = initialPitch;
        played = true;
        _as.pitch += (Random.Range(-pitchDiff, pitchDiff));

        // play random or specific
        if (i < 0)
            _as.PlayOneShot(GiveMeARandomSound());
        else
        {
            _as.PlayOneShot(sounds[i]);
        }
    }

    AudioClip GiveMeARandomSound()
    {
        //if (Random.value < 0.3) return clickSound_1;
        //if (Random.value < 0.6) return clickSound_2;
        //return clickSound_3;
        return sounds[Random.Range(0, sounds.Length - 1)];
    }
}
