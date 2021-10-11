using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public List<AudioClip> soundEfx;
    public List<AudioClip> music;
    public AudioMixer mixer;
    AudioSource audio;

    // Start is called before the first frame update
    void Awake()
    {
        audio = GetComponent<AudioSource>();
    }

    private void Start()
    {
        ChangeSnapshot("Normal");
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.anyKeyDown)
        //{
        //    print(audio.pitch);
        //    PlaySound(0);
        //}

    }

    public void ChangeSnapshot(string name)
    {
        mixer.FindSnapshot(name).TransitionTo(0);
    }

    public void PlaySound(int track)
    {
        audio.clip = soundEfx[track];
        audio.Play();
    }
}
