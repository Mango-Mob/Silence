using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_Legs : MonoBehaviour
{
    public float m_maxDegrees;
    
    private NavMeshAgent m_agent;

    private float m_targetDelay = 1.0f;
    private Quaternion m_targetOrientation;
    [SerializeField] private Vector3 m_targetLocation;
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
        if(m_targetDelay > 0)
            m_targetDelay -= Time.deltaTime;
    }

    public void SetTargetDestinaton(Vector3 location, float maxDist = float.MaxValue, bool canFlee = true)
    {
        if(location != m_targetLocation)
            m_targetDelay = 1.0f;

        m_agent.isStopped = false;

        Vector3 direction = (location - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, location);
        if (dist < maxDist && !canFlee)
        {
            m_targetLocation = transform.position;
        }
        else if(dist > maxDist)
        {
            m_targetLocation = transform.position + direction * maxDist;
        }
        else
        {
            m_targetLocation = location;
        }

        m_agent.destination = m_targetLocation;
    }

    public void Halt()
    {
        m_agent.isStopped = true;
    }

    public bool IsResting()
    {
        return (m_agent.velocity.magnitude < 0.15f || m_agent.isStopped) && m_targetDelay <= 0;
    }

    public void LookAtTarget()
    {
        Vector3 direct = m_agent.destination - transform.position;
        direct.y = 0;
        m_targetOrientation = Quaternion.LookRotation(direct.normalized, Vector3.up);
    }

    public void LookAtDirection(Vector3 direction)
    {
        m_targetOrientation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    public void SetTargetOrientation(Quaternion orient)
    {
        m_targetOrientation = orient;
    }

    public Vector3 GetRandomPointAround(Vector3 lastKnownLocation, float dist)
    {
        Vector3 randDirection = UnityEngine.Random.insideUnitSphere * dist;

        randDirection += transform.position;
        NavMeshHit hit;
        if(NavMesh.SamplePosition(randDirection, out hit, dist, 1))
        {
            return hit.position;
        }

        return transform.position;
    }
}
