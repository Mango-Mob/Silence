using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

///
/// <author> Michael Jordan </author> 
/// <year> 2021 </year>
/// 
/// <summary>
/// A wrapper for an Audio Source that handles external applications conducted by an audio agent.
/// 
/// </summary>
/// 
public class AudioPlayer
{
    public AudioClip currentClip { get { return source.clip; } }

    public bool isMutating { get; protected set; } = false; //If coroutine is active.

    protected AudioSource source; //Created in constructor.
    protected float m_localPitch = 1.0f;

    private bool isDelayed = false;

    //Constructor
    public AudioPlayer(GameObject owner, AudioClip clip)
    {
        source = owner.AddComponent<AudioSource>();
        source.pitch = AudioManager.instance.m_globalPitch * m_localPitch;
        source.playOnAwake = false;
        source.clip = clip;
    }

    #region AudioControls

    public void Play() { source.Play(); }
    public void Pause() { source.Pause(); }
    public void Stop() { source.Stop(); }
    public bool IsPlaying() { return source.isPlaying || isDelayed; }
    public void SetLooping(bool isLooping = true) { source.loop = isLooping; }
    public void SetPitch(float pitch) 
    {
        m_localPitch = pitch;
        source.pitch = AudioManager.instance.m_globalPitch * m_localPitch;
    }

    #endregion

    public void Update()
    {
        source.pitch = AudioManager.instance.m_globalPitch * m_localPitch;
    }

    /// <summary>
    /// Calculate the time remaining of the current clip.
    /// </summary>
    /// <returns>Time remaining in seconds, zero if the player isn't playing.</returns>
    public float TimeLeft()
    {
        if (!IsPlaying()) //Edge case
            return 0.0f;

        return source.clip.length - source.time;
    }

    /// <summary>
    /// Update the player's clip to a new one. Note: Will stop the current clip from playing.
    /// </summary>
    /// <param name="clip">New audio clip to change to.</param>
    public void SetClip(AudioClip clip)
    {
        //Stop the old clip.
        source.Stop();

        //Update clip
        source.clip = clip;
    }

    /// <summary>
    /// Update the player's local volume. Will not change if the current player is mutating.
    /// </summary>
    /// <param name="volume">New volume value</param>
    public void SetVolume(float volume)
    {
        if (!isMutating)
            source.volume = Mathf.Clamp(volume, 0.0f, 1.0f);
    }

    public float GetVolume()
    {
        return source.volume;
    }

    /// <summary>
    /// [Coroutine] Mutates the audio player to start fading in over some time. Will start 
    /// playing the audio from zero volume, to the original volume of this player. 
    /// </summary>
    /// <param name="time">Time in seconds to lerp the volume over.</param>
    public IEnumerator FadeIn(float time)
    {
        if (isMutating)
            yield return null;

        isMutating = true;

        float startVolume = source.volume;
        float startTime = time;

        while (time >= 0)
        {
            source.volume = (1.0f - time / startTime) * startVolume;
            yield return new WaitForEndOfFrame();
            time -= Time.deltaTime;
        }

        isMutating = false;
        yield return null;
    }

    /// <summary>
    /// [Coroutine] Mutates the audio player to start fading out over some time. Will pause 
    /// the audio once the volume hits zero. 
    /// </summary>
    /// <param name="time">Time in seconds to lerp the volume over.</param>
    public IEnumerator FadeOut(float time)
    {
        if (isMutating)
            yield return null;

        isMutating = true;

        float startVolume = source.volume;
        float startTime = time;

        while (time >= 0)
        {
            source.volume = (time / startTime) * startVolume;
            yield return new WaitForEndOfFrame();
            time -= Time.deltaTime;
        }
        Pause();
        isMutating = false;
        yield return null;
    }
    public IEnumerator PlayDelayed(float delay)
    {
        isDelayed = true;
        yield return new WaitForSecondsRealtime(delay);
        isDelayed = false;

        Play();
        yield return null;
    }
}
