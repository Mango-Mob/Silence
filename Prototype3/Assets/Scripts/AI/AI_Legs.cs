using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_Legs : MonoBehaviour
{
    public float m_maxDegrees;
    private NavMeshAgent m_agent;

    private Quaternion m_targetOrientation;
    // Start is called before the first frame update
    void Start()
    {
        m_agent = GetComponentInChildren<NavMeshAgent>();
        m_agent.isStopped = true;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, m_targetOrientation, m_maxDegrees);
    }

    public void SetTargetDestinaton(Vector3 location)
    {
        m_agent.destination = location;
        m_agent.isStopped = false;
    }

    public bool IsResting()
    {
        return m_agent.velocity.magnitude < 0.15f || m_agent.isStopped;
    }

    public void LookAtTarget()
    {
        Vector3 direct = m_agent.destination - transform.position;
        direct.y = 0;
        m_targetOrientation = Quaternion.LookRotation(direct.normalized, Vector3.up);
    }

    public void SetTargetOrientation(Quaternion orient)
    {
        m_targetOrientation = orient;
    }
}
