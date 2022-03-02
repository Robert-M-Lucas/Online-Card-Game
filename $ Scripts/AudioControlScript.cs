// Author: Robert Lucas
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Audio Clip
[System.Serializable]
public struct NamedAudioClip
{
    public string name;
    public AudioClip audio;
    [Range(0, 2)]
    public float volume;
    [System.NonSerialized]
    public AudioSource source;
}

// Controls player audio

public class AudioControlScript : MonoBehaviour
{
    public NamedAudioClip[] NamedAudioClips;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Play a sound
    public void PlaySound(int index)
    {
        play_sound(index);
    }

    public void PlaySound(string name)
    {
        for (int i = 0; i < NamedAudioClips.Length; i++)
        {
            if (NamedAudioClips[i].name == name)
            {
                play_sound(i);
                return;
            }
        }
        Debug.LogError("Sound " + name + " not found");
    }

    private void play_sound(int index)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = NamedAudioClips[index].audio;
        source.volume = NamedAudioClips[index].volume;
        source.Play();

        //Invoke("end_sound", NamedAudioClips[index].audio.length);

        // Destroys audio source when sound ends
        StartCoroutine(end_sound(source, NamedAudioClips[index].audio.length + 0.5f));
    }

    // Starts sound loop (such as running)
    public void PlaySoundLoop(int index)
    {
        play_loop(index);
    }
    public void PlaySoundLoop(string name)
    {
        for (int i = 0; i < NamedAudioClips.Length; i++)
        {
            if (NamedAudioClips[i].name == name)
            {
                play_loop(i);
                return;
            }
        }
        Debug.LogError("Sound " + name + " not found");
    }

    private void play_loop(int index)
    {
        if (NamedAudioClips[index].source != null) { Destroy(NamedAudioClips[index].source); }
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.loop = true;
        source.clip = NamedAudioClips[index].audio;
        source.volume = NamedAudioClips[index].volume;
        source.Play();
        NamedAudioClips[index].source = source;
    }

    // Stops a sound loop
    public void StopSoundLoop(int index)
    {
        stop_loop(index);
    }
    public void StopSoundLoop(string name)
    {
        for (int i = 0; i < NamedAudioClips.Length; i++)
        {
            if (NamedAudioClips[i].name == name)
            {
                stop_loop(i);
                return;
            }
        }
        Debug.LogError("Sound " + name + " not found");
    }

    private void stop_loop(int index)
    {
        if (NamedAudioClips[index].source != null) { Destroy(NamedAudioClips[index].source); }
    }

    // Called after clip finishes playing
    IEnumerator end_sound(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(source);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
