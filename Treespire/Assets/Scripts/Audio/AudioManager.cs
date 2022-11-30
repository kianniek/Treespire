using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    // definition of a sound for easy management
    [Serializable]
    internal struct Sound
    {
        public enum Type { None, ButtonClick, Error, StartUp}
        public AudioClip clip;
        public Type type;
    }

    // all defined sounds
    [SerializeField] private List<Sound> sounds;

    // audio sources
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource systemAudioSource;

    // current volume for all sources
    private float currentVolume;

    /// <summary>
    /// Initializes the manager, prepares for updates.
    /// </summary>
    internal void Initialize()
    {
        // set volume and force an update
        OnVolumeChanged(0.5f, true);
    }
    
    /// <summary>
    /// Called on volume change.
    /// </summary>
    /// <param name="newVolume">The new volume</param>
    /// <param name="forceUpdate">Whether it should always update, no regards</param>
    internal void OnVolumeChanged(float newVolume, bool forceUpdate = false)
    {
        // return if there is no change and it isn't a forced update
        if (!forceUpdate && newVolume == currentVolume)
            return;

        // set all sources to new volume
        // and keep track of current volume
        systemAudioSource.volume = musicAudioSource.volume = currentVolume = newVolume;
    }

    /// <summary>
    /// Call to play a sound one shot.
    /// </summary>
    /// <param name="soundType">The sound to play</param>
    internal void PlaySound(Sound.Type soundType)
    {
        // find the clip to play
        AudioClip clip = sounds.Find(s => s.type == soundType).clip;

        // if a clip was found, play it one shot
        if (clip != null)
            systemAudioSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Call to play or pause music.
    /// </summary>
    /// <param name="play">Whether to play or pause</param>
    internal void PlayMusic(bool play)
    {
        // either play or pause the music source
        if (play)
            musicAudioSource.Play();
        else
            musicAudioSource.Pause();
    }

    /// <summary>
    /// Is the music source currently playing?
    /// </summary>
    /// <returns></returns>
    internal bool GetMusicIsPlaying()
    {
        return musicAudioSource.isPlaying;
    }

    /// <summary>
    /// What is the current play time of the music source?
    /// </summary>
    /// <returns></returns>
    internal float GetMusicPlaytime()
    {
        return musicAudioSource.time;
    }

    /// <summary>
    /// Call to stop the music
    /// Please don't stop the music.
    /// </summary>
    internal void StopMusic()
    {
        musicAudioSource.Stop();
    }

    /// <summary>
    /// Set the clip of the music source.
    /// </summary>
    /// <param name="clip">The new clip</param>
    internal void SetMusicClip(AudioClip clip)
    {
        musicAudioSource.clip = clip;
    }
}
