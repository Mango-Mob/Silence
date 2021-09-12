using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    public float m_range;
    public LayerMask m_openers;

    private MultiAudioAgent m_agent;
    private Animator m_animator;
    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_agent = GetComponent<MultiAudioAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        m_animator.SetBool("IsOpen", Physics.OverlapSphere(transform.position, m_range, m_openers).Length > 0);
    }

    public void PlayOpenSound()
    {
        m_agent.Play("DoorOpen");
    }
    public void PlayCloseSound()
    {
        m_agent.Play("DoorClose");
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, m_range);
    }
}
