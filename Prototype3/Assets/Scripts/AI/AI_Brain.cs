using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InterestType
{
    Unknown, Player,
}
public struct AI_Interest
{
    public AI_Interest(Vector3 _position, Collider _reference = null)
    {
        lastKnownLocation = _position;
        reference = _reference;
        lastSeen = DateTime.Now;

        if (reference == null)
        {
            interestType = InterestType.Unknown;
        }
        else if (reference.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            interestType = InterestType.Player;
        }
        else
        {
            interestType = InterestType.Unknown;
        }
    }

    public Collider reference;
    public DateTime lastSeen;
    public Vector3 lastKnownLocation;
    public InterestType interestType;

    public double GetAge()
    {
        return (DateTime.Now - lastSeen).TotalSeconds;
    }
    public void Refesh()
    {
        lastSeen = DateTime.Now;
    }
}

public class AI_Brain : MonoBehaviour
{
    public enum AI_State
    {
        Idle, ReturnToPatrol, Patrol, Alert, Investigating, Hunting, Engaging
    }
    [Header("AI Statistics")]
    public AI_State m_myState;
    public float m_attentionBuild;
    public float m_attentionDecay;
    public float m_aggressionBuild;
    public float m_aggressionDecay;

    private AI_Legs m_myLegs;
    [SerializeField] private AI_Path m_myRoute;
    private AI_Sight m_mySight;
    private AI_Hearing m_myHearing;

    private Vector3 m_targetWaypoint;
    [Header("Vision Variables")]
    public float m_visionSpeed = 5.0f;
    public Vector3 m_visionMinEuler;
    public Vector3 m_visionMaxEuler;
    public Transform m_neckTransform;
    public SpriteRenderer m_attentionBar;
    public SpriteRenderer m_agressionBar;
    private Coroutine m_neckRoutine;

    private AI_Interest? m_currentInterest;
    private float m_idleTimer = 0f;
    private int m_visionDir = 1;
    private float m_attention = 0.0f;
    private float m_agression = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_myLegs = GetComponentInChildren<AI_Legs>();
        m_mySight = GetComponentInChildren<AI_Sight>();
        m_myHearing = GetComponentInChildren<AI_Hearing>();
        m_idleTimer += 3.5f;
    }

    // Update is called once per frame
    void Update()
    {
        VisionUpdate();
        BehaviorUpdate();

        m_agressionBar.transform.localScale = new Vector3(m_agression, 1, 1);
        m_attentionBar.transform.localScale = new Vector3(m_attention, 1, 1);
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
                m_idleTimer -= Time.deltaTime;
                if(m_idleTimer <= 0)
                {
                    if(m_myRoute.IsOnPath(transform.position))
                    {
                        TransitionBehaviorTo(AI_State.Patrol);
                    }
                    else
                    {
                        TransitionBehaviorTo(AI_State.ReturnToPatrol);
                    }
                }
                SensorCheck();
                HearingCheck();
                break;
            case AI_State.ReturnToPatrol:
                if(m_myLegs.IsResting())
                {
                    TransitionBehaviorTo(AI_State.Idle);
                }
                SensorCheck();
                HearingCheck();
                break;
            case AI_State.Patrol:
                if (m_myLegs.IsResting())
                {
                    TransitionBehaviorTo(AI_State.Idle);
                }
                SensorCheck();
                HearingCheck();
                break;
            case AI_State.Investigating:
                Vector3 direction = (m_currentInterest.Value.lastKnownLocation - transform.position).normalized;
                m_myLegs.SetTargetOrientation(Quaternion.LookRotation(direction, Vector3.up));

                if(m_currentInterest == null)
                {
                    m_targetWaypoint = m_myLegs.GetRandomPointAround(transform.position, 5.0f);
                    m_myLegs.SetTargetDestinaton(m_targetWaypoint);
                    m_myLegs.LookAtTarget();
                }
                else if(m_mySight.IsWithinSight(m_currentInterest.Value.lastKnownLocation) 
                    && m_mySight.CanRaycastToPosition(m_currentInterest.Value.lastKnownLocation))
                {
                    //Found Location
                    m_myLegs.Halt();
                }
                else
                {
                    m_myLegs.SetTargetDestinaton(m_currentInterest.Value.lastKnownLocation);
                }
                SensorCheck();
                HearingCheck();
                Decay();
                break;
            case AI_State.Hunting:
                if(m_currentInterest != null)
                {
                    if(m_myLegs.IsResting())
                    {
                        m_targetWaypoint = m_myLegs.GetRandomPointAround(m_currentInterest.Value.lastKnownLocation, 5.0f);
                        m_myLegs.SetTargetDestinaton(m_targetWaypoint);
                        m_myLegs.LookAtTarget();
                    }
                }
                else
                {
                    if (m_myLegs.IsResting())
                    {
                        m_targetWaypoint = m_myLegs.GetRandomPointAround(transform.position, 5.0f);
                        m_myLegs.SetTargetDestinaton(m_targetWaypoint);
                        m_myLegs.LookAtTarget();
                    }
                }
                SensorCheck();
                HearingCheck();
                Decay();
                break;
            case AI_State.Alert:
                m_myLegs.SetTargetDestinaton(m_targetWaypoint, m_mySight.m_sightRange / 2.0f, false);
                m_myLegs.LookAtDirection(m_targetWaypoint - transform.position);
                SensorCheck();
                HearingCheck();
                break;
            case AI_State.Engaging:
                m_myLegs.SetTargetDestinaton(m_targetWaypoint, m_mySight.m_sightRange / 2.0f, false);
                m_myLegs.LookAtDirection(m_targetWaypoint - transform.position);
                SensorCheck();
                HearingCheck();
                break;
            default:
                break;
        }
    }

    private void SensorCheck()
    {
        foreach (var item in m_mySight.m_collidersWithinSight)
        {
            if(item.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                //Found player!
                float distMod = 1.0f - Vector3.Distance(item.transform.position, transform.position) / m_mySight.m_sightRange;
                distMod = Mathf.Clamp(distMod, 0.0f, 1.0f);

                if (m_attention < 1.0f)
                {
                    m_attention += distMod * m_attentionBuild * Time.deltaTime;

                    if (m_attention > 0.0f)
                    {
                        m_targetWaypoint = item.transform.position;
                        TransitionBehaviorTo(AI_State.Alert);
                    }

                    if (m_attention > 1.0f)
                    {
                        m_agression += m_attention - 1.0f;
                        m_attention = 1.0f;
                    }
                }
                else
                {
                    m_targetWaypoint = item.transform.position;
                    m_agression = Mathf.Clamp(m_agression + distMod * m_aggressionBuild * Time.deltaTime, 0.0f, 1.0f);
                }

                if (m_agression == 1.0f)
                {
                    TransitionBehaviorTo(AI_State.Engaging);
                }
                return;
            }
        }

        if(m_mySight.m_interests.Count > 0)
        {
            if(m_currentInterest == null)
            {
                m_currentInterest = m_mySight.m_interests[m_mySight.m_interests.Count - 1];
                return;
            }

            if(m_currentInterest.Value.GetAge() > m_mySight.m_interests[m_mySight.m_interests.Count - 1].GetAge())
            {
                m_currentInterest = m_mySight.m_interests[m_mySight.m_interests.Count - 1];
            }
            else if(m_mySight.m_interests[m_mySight.m_interests.Count - 1].interestType == InterestType.Player)
            {
                m_currentInterest = m_mySight.m_interests[m_mySight.m_interests.Count - 1];
            }
        }

        if (m_agression > 0.0f)
        {
            TransitionBehaviorTo(AI_State.Hunting);
        }
        else if (m_attention > 1.0f)
        {
            TransitionBehaviorTo(AI_State.Investigating);
        }
    }

    private void HearingCheck()
    {
        if (m_myHearing.m_interests.Count > 0)
        {
            if (m_currentInterest == null)
            {
                m_currentInterest = m_myHearing.m_interests[m_myHearing.m_interests.Count - 1];
                return;
            }

            if (m_currentInterest.Value.GetAge() > m_myHearing.m_interests[m_myHearing.m_interests.Count - 1].GetAge())
            {
                m_currentInterest = m_myHearing.m_interests[m_myHearing.m_interests.Count - 1];
            }
            else if (m_myHearing.m_interests[m_myHearing.m_interests.Count - 1].interestType == InterestType.Player)
            {
                m_currentInterest = m_myHearing.m_interests[m_myHearing.m_interests.Count - 1];
            }

            if (m_attention >= 0)
            {
                m_attention = 1.0f;
            }

            switch (m_myState)
            {
                case AI_State.Idle:
                case AI_State.ReturnToPatrol:
                case AI_State.Patrol:
                    TransitionBehaviorTo(AI_State.Investigating);
                    break;
                default:
                case AI_State.Alert:
                case AI_State.Investigating:
                case AI_State.Hunting:
                case AI_State.Engaging:
                    break;
            }
        }
    }

    private void TransitionBehaviorTo(AI_State state)
    {
        if (m_myState == state)
            return;

        switch (state)
        {
            case AI_State.Idle:
                m_targetWaypoint = transform.position;
                m_idleTimer += 3.5f;
                m_myLegs.Halt();
                m_myLegs.LookAtTarget();
                break;
            case AI_State.ReturnToPatrol:
                m_targetWaypoint = m_myRoute.GetClosestWaypoint(transform.position);
                m_myLegs.SetTargetDestinaton(m_targetWaypoint);
                m_myLegs.LookAtTarget();
                break;
            case AI_State.Patrol:
                m_targetWaypoint = m_myRoute.GetNextWaypoint(transform.position);
                m_myLegs.SetTargetDestinaton(m_targetWaypoint);
                m_myLegs.LookAtTarget();
                break;
            case AI_State.Alert:
                break;
            case AI_State.Investigating:
                m_idleTimer += 3.5f;
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

    private void Decay()
    {
        if(m_agression > 0.0f)
        {
            m_agression -= Time.deltaTime * m_aggressionDecay;
            if (m_agression < 0.0f)
            {
                m_attention += m_agression;
                m_agression = 0;
            }
        }
        else if(m_attention > 0.0f)
        {
            m_attention -= Time.deltaTime * m_attentionDecay;
            m_attention = Mathf.Clamp(m_attention, 0.0f, 1.0f);
        }
        else
        {
            TransitionBehaviorTo(AI_State.ReturnToPatrol);
        }
    }
    public bool Kill(Vector3 killerPosition)
    {
        return false;
    }

    public IEnumerator NeckTowardsAngle(Vector3 euler, IEnumerator routineAfterwards = null)
    {
        while(m_neckTransform.localRotation != Quaternion.Euler(euler.x, euler.y, euler.z))
        {
            m_neckTransform.localRotation = Quaternion.RotateTowards(m_neckTransform.localRotation, Quaternion.Euler(euler.x, euler.y, euler.z), m_visionSpeed);
            yield return new WaitForEndOfFrame();
        }

        if (routineAfterwards != null)
        {
            StartCoroutine(routineAfterwards);
        }
        else
        {
            m_neckRoutine = null;
        }
        
        yield return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
        Gizmos.DrawSphere(m_targetWaypoint, 0.5f);
    }
}
