using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///
/// <author> Michael Jordan </author> 
/// <year> 2021 </year>
/// 
/// <summary>
/// An abstract parent class for audio agents.
/// </summary>
/// 
public abstract class AudioAgent : MonoBehaviour
{
    [Header("Parent Settings:")] //Local settings
    [Range(0.0f, 1.0f)]
    public float localVolume = 1f;
    [Tooltip("Mutes this agent completely.")]
    public bool isMuted = false;

    protected virtual void Awake()
    {
        AudioManager.instance.agents.Add(this);

        if(isMuted)
            Debug.LogWarning($"Audio agent is muted on awake, location: {gameObject.name}.");
    }

    protected abstract void Update();

    public virtual void SetMute(bool status) { isMuted = status; }

    protected virtual void OnDestroy()
    {
        AudioManager.instance.agents.Remove(this);

        if (AudioManager.instance.agents.Count == 0 && AudioManager.instance.listeners.Count == 0)
            AudioManager.DestroyInstance();
    }
}
