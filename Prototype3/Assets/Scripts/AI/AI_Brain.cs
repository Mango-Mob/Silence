using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Brain : MonoBehaviour
{
    public enum AI_State
    {
        Idle, ReturnToPatrol, Patrol, Hunting, Engaging
    }
    [Header("AI Statistics")]
    public AI_State m_myState;

    private AI_Legs m_myLegs;
    [SerializeField] private AI_Path m_myRoute;
    private AI_Sight m_mySight;
    private AI_Hearing m_myHearing;

    private Vector3 m_targetWaypoint;
    [Header("Vision Variables")]
    public float m_visionSpeed = 5.0f;
    public Vector3 m_visionMinEuler;
    public Vector3 m_visionMaxEuler;
    
    private int m_visionDir = 1;

    // Start is called before the first frame update
    void Start()
    {
        m_myLegs = GetComponentInChildren<AI_Legs>();
        m_mySight = GetComponentInChildren<AI_Sight>();
        m_myHearing = GetComponentInChildren<AI_Hearing>();
    }

    // Update is called once per frame
    void Update()
    {
        VisionUpdate();

        if (m_myRoute != null)
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
    private void VisionUpdate()
    {
        Quaternion min = Quaternion.Euler(m_visionMinEuler.x, m_visionMinEuler.y, m_visionMinEuler.z);
        Quaternion max = Quaternion.Euler(m_visionMaxEuler.x, m_visionMaxEuler.y, m_visionMaxEuler.z);

        switch (m_visionDir)
        {
            default:
            case 1:
                m_mySight.transform.localRotation = Quaternion.RotateTowards(m_mySight.transform.localRotation, max, m_visionSpeed);
                if(m_mySight.transform.localRotation == max)
                {
                    m_visionDir *= -1;
                }
                break;
            case -1:
                m_mySight.transform.localRotation = Quaternion.RotateTowards(m_mySight.transform.localRotation, min, m_visionSpeed);
                if (m_mySight.transform.localRotation == min)
                {
                    m_visionDir *= -1;
                }
                break;
        }
    }

    private void BehaviorUpdate()
    {
        switch (m_myState)
        {
            case AI_State.Idle:
                SensorCheck();
                break;
            case AI_State.ReturnToPatrol:
                break;
            case AI_State.Patrol:
                break;
            case AI_State.Hunting:
                break;
            case AI_State.Engaging:
                break;
            default:
                break;
        }
    }

    private void SensorCheck()
    {
        if(m_mySight.GetInterestsCount() > 0 || m_myHearing.GetInterestsCount() > 0)
        {
            //Something of interest!
            m_myLegs.Halt();
            //m_myLegs.SetTargetOrientation(Quaternion.LookRotation());
        }

    }

    private void TransitionBehaviorTo(AI_State state)
    {
        if (m_myState == state)
            return;

        switch (state)
        {
            case AI_State.Idle:
                break;
            case AI_State.ReturnToPatrol:
                break;
            case AI_State.Patrol:
                break;
            case AI_State.Hunting:
                break;
            case AI_State.Engaging:
                break;
            default:
                break;
        }

        m_myState = state;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
        Gizmos.DrawSphere(m_targetWaypoint, 0.5f);
    }
}
