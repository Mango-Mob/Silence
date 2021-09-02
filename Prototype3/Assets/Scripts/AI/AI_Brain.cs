using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Brain : MonoBehaviour
{
    private AI_Legs m_myLegs;
    [SerializeField] private AI_Path m_myRoute;

    private Vector3 m_targetWaypoint;
    // Start is called before the first frame update
    void Start()
    {
        m_myLegs = GetComponentInChildren<AI_Legs>();
    }

    // Update is called once per frame
    void Update()
    {
        if(m_myRoute != null)
        {   
            if(!m_myRoute.IsOnPath(transform.position))
            {
                m_targetWaypoint = m_myRoute.GetClosestWaypoint(transform.position);
                if (m_myLegs.IsResting())
                {
                    m_myLegs.SetTargetDestinaton(m_targetWaypoint);
                }
            }
            else if(m_myLegs.IsResting())
            {
                m_targetWaypoint = m_myRoute.GetNextWaypoint(transform.position);
                m_myLegs.SetTargetDestinaton(m_targetWaypoint);
            }

            m_myLegs.LookAtTarget();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
        Gizmos.DrawSphere(m_targetWaypoint, 0.5f);
    }
}
