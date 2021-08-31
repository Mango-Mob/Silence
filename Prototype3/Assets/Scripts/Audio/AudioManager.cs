using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///
/// <author> Michael Jordan </author> 
/// <year> 2021 </year>
/// 
/// <summary>
/// A global singleton used to handle all listeners and audio agents within the current scene, by
/// containing the global volume settings.
/// 
/// Note: Agents/Listeners are incharge of being added/removed when they are awake/destroyed. 
/// </summary>
/// 
public class AudioManager
{
    #region Singleton
    private static AudioManager _instance;

    public static AudioManager instance 
    { 
        get 
        {
            if (_instance == null)
            {
                _instance = new AudioManager();
            }

            return _instance;
        } 
    }

    public static void DestroyInstance()
    {
        _instance.OnDestroy();
        _instance = null;
    }

    private AudioManager()
    {
        agents = new List<AudioAgent>();
        listeners = new List<ListenerAgent>();
        Awake();
    }
    #endregion

    //Agent and listener lists:
    public List<AudioAgent> agents { get; private set; }
    public List<ListenerAgent> listeners { get; private set; }

    //private array of volumes
    public float[] volumes;
    public float m_globalPitch = 1.0f;

    //Volume types: 
    //(Add more to dynamically expand the above array)
    public enum VolumeChannel
    {
        MASTER,
        SOUND_EFFECT,
        MUSIC,
    }

    /// <summary>
    /// Called imediately after creation in the constructor
    /// </summary>
    private void Awake()
    {
        volumes = new float[Enum.GetNames(typeof(AudioManager.VolumeChannel)).Length];
        for (int i = 0; i < volumes.Length; i++)
        {
            volumes[i] = PlayerPrefs.GetFloat($"volume{i}", 1.0f);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < volumes.Length; i++)
        {
            PlayerPrefs.SetFloat($"volume{i}", volumes[i]);
        }
    }

    /// <summary>
    /// Gets the volume of the type additionally based on the agent's location
    /// </summary>
    /// <param name="type">Volume type to accurately base calculate the volume on.</param>
    /// <param name="agent">The agent to base the 3d volume on. Use NULL instead for gobal volume.</param>
    /// <returns> volume data between 0.0f and 1.0f </returns>
    public float GetVolume(VolumeChannel type, AudioAgent agent)
    {
        if (type == VolumeChannel.MASTER)
            return volumes[(int)VolumeChannel.MASTER] * CalculateHearingVolume(agent);

        return volumes[(int)VolumeChannel.MASTER] * volumes[(int)type] * CalculateHearingVolume(agent);
    }

    /// <summary>
    /// Makes the agent the only one playing with volume, the others will be muted. 
    /// </summary>
    /// <param name="_agent">Agent to prioritise.</param>
    public void MakeSolo(AudioAgent _agent)
    {
        if (_agent == null) //Edge case
        {
            Debug.LogError($"Agent attempting to be solo is null, ignored function call");
            return;
        }
        
        //Mute all other agents
        foreach (var agent in agents)
        {
            agent.SetMute(true);
        }

        //Unmute param agent
        _agent.SetMute(false);
    }

    /// <summary>
    /// Unmutes all agents within the scene.
    /// </summary>
    public void UnMuteAll()
    {
        foreach (var agent in agents)
        {
            agent.SetMute(false);
        }
    }

    /// <summary>
    /// Calculates the 3D volume based on the distance from all listeners.
    /// </summary>
    /// <param name="agent">Agent to get the volume of. use NULL for global volume.</param>
    /// <returns>Largest volume returned from all listener calculations.</returns>
    private float CalculateHearingVolume(AudioAgent agent)
    {
        if (listeners.Count == 0) //Edge case
            return 1.0f;

        if (agent == null) //Edge case
            return 1.0f;

        float max = 0.0f;
        foreach (var listener in listeners)
        {
            max = Mathf.Max(listener.CalculateHearingVol(agent.transform.position), max);
        }
        return max;
    }
}
