using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Michael Jordan
/// </summary>
public class ListenerAgent : MonoBehaviour
{
    [Range(0, 100)]
    public float softRange = 100;
    [Range(0, 50)]
    public float hardRange = 50;

    // Start is called before the first frame update
    private void Awake()
    {
        AudioManager.instance.listeners.Add(this);
    }

    private void OnDestroy()
    {
        AudioManager.instance.listeners.Remove(this);
        if (AudioManager.instance.agents.Count == 0 && AudioManager.instance.listeners.Count == 0)
            AudioManager.DestroyInstance();
    }

    public float CalculateHearingVol(Vector3 audioPos)
    {
        float distance = Vector3.Distance(transform.position, audioPos);
        if (distance <= hardRange)
            return 1.0f;
        else
            return Mathf.Clamp(1.0f - (distance - hardRange) /(softRange - hardRange), 0.0f, 1.0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, hardRange);
        Gizmos.color = Color.red;

        if(softRange > hardRange)
            Gizmos.DrawWireSphere(transform.position, softRange);

        Gizmos.color = Color.white;
    }
}
