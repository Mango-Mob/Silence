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
        Idle, ReturnToPatrol, Patrol, Meeting, Alert, Investigating, Hunting, Engaging, Dead
    }
    [Header("AI Statistics")]
    public AI_State m_myState;
    public float m_attentionBuild;
    public float m_attentionDecay;
    public float m_aggressionBuild;
    public float m_aggressionDecay;
    public float m_immuneRange = 160;
    public float m_maxKillDist = 2.5f;
    public Vector3 m_meetingLoc;
    public float m_meetingTime;
    public GameObject m_targetTransform;
    public float m_timeDelayBetweenShots;
    public Transform m_shotOrigin;

    public Collider m_aliveCollider;
    public Collider m_deathCollider;

    private float m_shotDelay = 0f;
    public int m_routeWaypointID = -1;

    private AI_Legs m_myLegs;
    [SerializeField] private AI_Path m_myRoute;
    private AI_Sight m_mySight;
    private AI_Hearing m_myHearing;
    private AI_Animator m_animator;
    private MultiAudioAgent m_agent;

    private Vector3 m_targetWaypoint;
    [Header("Vision Variables")]
    public float m_visionSpeed = 5.0f;
    public Vector3 m_visionMinEuler;
    public Vector3 m_visionMaxEuler;
    public Transform m_neckTransform;
    public SpriteRenderer m_attentionBar;
    public SpriteRenderer m_agressionBar;

    public struct AlliedInfo
    {
        public AlliedInfo(AI_Brain _brain)
        {
            brain = _brain;
            foundDead = false;
            timeOfMeeting = DateTime.Now;
        }

        public AlliedInfo(AlliedInfo oldInfo)
        {
            brain = oldInfo.brain;
            foundDead = oldInfo.foundDead;
            timeOfMeeting = DateTime.Now;
        }

        public AI_Brain brain;
        public bool foundDead;
        public DateTime timeOfMeeting;

        public double GetTimeSinceLastMeeting()
        {
            return (DateTime.Now - timeOfMeeting).TotalSeconds;
        }
    }

    private Dictionary<Collider, AlliedInfo> m_myAllies = new Dictionary<Collider, AlliedInfo>();
    public AlliedInfo? m_currentMeetingPartner;
    private AI_Interest? m_currentInterest;
    private float m_idleTimer = 0f;
    private int m_visionDir = 1;
    private float m_attention = 0.0f;
    private float m_agression = 0.0f;

    private void Awake()
    {
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("EnemyProjectile"));
        m_aliveCollider.enabled = true;
        m_deathCollider.enabled = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        //Search the scene for all allies
        foreach (var item in FindObjectsOfType<AI_Brain>())
        {
            if (item == this)
                continue;

            m_myAllies.Add(item.GetComponent<Collider>(), new AlliedInfo(item));
        }

        m_myLegs = GetComponentInChildren<AI_Legs>();
        m_mySight = GetComponentInChildren<AI_Sight>();
        m_myHearing = GetComponentInChildren<AI_Hearing>();
        m_animator = GetComponentInChildren<AI_Animator>();
        m_agent = GetComponent<MultiAudioAgent>();
        m_idleTimer += 3.5f;
    }

    // Update is called once per frame
    void Update()
    {
        VisionUpdate();
        BehaviorUpdate();

        m_animator.SetVelocity(m_myLegs.GetVelocity());

        m_agressionBar.transform.localScale = new Vector3(m_agression, 1, 1);
        m_attentionBar.transform.localScale = new Vector3(m_attention, 1, 1);
    }

    public bool KillGuard(Vector3 killerLoc)
    {
        Quaternion lookTo = Quaternion.LookRotation((killerLoc - transform.position).normalized);
        float dist = Vector3.Distance(killerLoc, transform.position);
        if(Mathf.Abs(Quaternion.Angle(transform.rotation, lookTo)) >= m_immuneRange && dist <= m_maxKillDist)
        {
            TransitionBehaviorTo(AI_State.Dead);
            return true;
        }
        else if (dist <= m_maxKillDist)
        {
            m_currentInterest = new AI_Interest(killerLoc);
            TransitionBehaviorTo(AI_State.Hunting);
            m_agression = 1.0f;
            m_attention = 1.0f;
            return false;
        }
        else
        {
            return false;
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
                m_myLegs.SetTargetDestinaton(m_targetWaypoint, m_mySight.m_sightRange * 0.25f, m_mySight.m_sightRange * 0.75f, false);
                m_myLegs.LookAtDirection(m_targetWaypoint - transform.position);
                SensorCheck();
                HearingCheck();
                break;
            case AI_State.Engaging:
                if (m_shotDelay > 0)
                {
                    m_shotDelay -= Time.deltaTime;
                }
                else 
                {
                    m_shotDelay += m_timeDelayBetweenShots;
                    m_animator.Shoot();
                }
                m_myLegs.SetTargetDestinaton(m_targetWaypoint, m_mySight.m_sightRange * 0.25f, m_mySight.m_sightRange * 0.75f, false);
                m_myLegs.LookAtTarget(60f);
                m_targetTransform.transform.position = m_targetWaypoint;
                SensorCheck();
                HearingCheck();
                break;
            case AI_State.Dead:
                m_aliveCollider.enabled = false;
                m_deathCollider.enabled = true;
                break;
            case AI_State.Meeting:
                if(m_myLegs.IsResting())
                {
                    if (m_meetingTime < 5.0f)
                    {
                        m_meetingTime += Time.deltaTime;
                        if (m_meetingTime < 4.0f && UnityEngine.Random.Range(0, 1000) < 200)
                        {
                            m_animator.Talk();
                        }
                    }
                    else
                    {
                        if (m_currentMeetingPartner.HasValue)
                        {
                            foreach (var item in m_myAllies)
                            {
                                if(item.Value.brain == m_currentMeetingPartner.Value.brain)
                                {
                                    m_myAllies[item.Key] = new AlliedInfo(m_currentMeetingPartner.Value);
                                    break;
                                }
                            }
                            m_currentMeetingPartner = null;
                        }
                        TransitionBehaviorTo(AI_State.Idle);
                        m_idleTimer = 0.5f;
                        m_meetingTime = 0.0f;
                    }
                }
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

                float visMod;
                if (item.GetComponent<PlayerMovement>())
                {
                    visMod = item.GetComponent<PlayerMovement>().m_visibility;
                }
                else
                {
                    visMod = 1f;
                }
                

                if (m_attention < 1.0f)
                {
                    m_attention += distMod * visMod * m_attentionBuild * Time.deltaTime;

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
            if(item.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                AlliedInfo info;
                if(m_myAllies.TryGetValue(item, out info))
                {
                    switch (info.brain.m_myState)
                    {
                        case AI_State.Idle:
                        case AI_State.ReturnToPatrol:
                        case AI_State.Patrol:
                            //Initiate talk
                            if(info.GetTimeSinceLastMeeting() > 30.0f && m_myState != AI_State.Meeting)
                            {
                                PingForMeeting(info);
                            }
                            break;
                        case AI_State.Alert:
                        case AI_State.Meeting:
                        case AI_State.Investigating:
                            //Ignore
                            break;
                        case AI_State.Hunting:
                        case AI_State.Engaging:
                            //Hunt with them
                            TransitionBehaviorTo(info.brain.m_myState);
                            m_agression = 1.0f;
                            m_attention = 1.0f;
                            m_currentInterest = info.brain.m_currentInterest;
                            break;
                        case AI_State.Dead:
                            if(!info.foundDead)
                            {
                                //Alert any nearby
                                TransitionBehaviorTo(AI_State.Hunting);
                                m_agression = 1.0f;
                                m_attention = 1.0f;
                                NoiseManager.instance.CreateNoise(transform.position, 25.0f, gameObject.layer, 2.0f, 0.0f);
                                info = new AlliedInfo(info.brain);
                                info.foundDead = true;
                                m_myAllies[item] = info;
                            }
                            break;
                        default:
                            break;
                    }
                }
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

    private void PingForMeeting(AlliedInfo other)
    {
        //Find meeting location
        Vector3 midPoint = (other.brain.transform.position + transform.position) / 2;
        float maxMeetingDist = 20f; //Too far, why bother?
        if(m_myLegs.GetPathDistTo(midPoint) <= maxMeetingDist && other.brain.PongForMeeting(midPoint, this))
        {
            //Start meeting
            m_meetingLoc = midPoint;
            TransitionBehaviorTo(AI_State.Meeting);
            CompareInformation(this, other.brain);
            m_currentMeetingPartner = other;
        }
        else
        {
            other.timeOfMeeting = DateTime.Now;
        }
    }

    public void CompareInformation(AI_Brain pinger, AI_Brain ponger)
    {
        foreach (var myData in pinger.m_myAllies)
        {
            foreach (var data in ponger.m_myAllies)
            {
                if(myData.Value.brain == data.Value.brain)
                {
                    if(myData.Value.foundDead)
                    {
                        ponger.m_myAllies[data.Key] = new AlliedInfo(myData.Value);
                    }
                    if (data.Value.foundDead)
                    {
                        pinger.m_myAllies[myData.Key] = new AlliedInfo(data.Value);
                    }
                    break;
                }
            }
        }
    }

    public bool PongForMeeting(Vector3 midPoint, AI_Brain pinger)
    {
        float maxMeetingDist = 20f; //Too far, why bother?
        if (m_myLegs.GetPathDistTo(midPoint) <= maxMeetingDist)
        {
            foreach (var item in m_myAllies)
            {
                if(item.Value.brain == pinger)
                {
                    m_currentMeetingPartner = item.Value;
                    m_meetingLoc = midPoint;
                    TransitionBehaviorTo(AI_State.Meeting);
                    return true;
                }
            }
        }
        return false;
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
                    m_agression = 1.0f;
                    break;
            }
        }
    }

    private void TransitionBehaviorTo(AI_State state)
    {
        if (m_myState == state)
            return;

        m_animator.Disengage();

        switch (state)
        {
            case AI_State.Idle:
                m_targetWaypoint = transform.position;
                m_idleTimer += 3.5f;
                m_myLegs.m_runMode = false;
                m_myLegs.SetTargetOrientation(m_myRoute.GetLookDirection(transform.position));
                m_myLegs.Halt();
                break;
            case AI_State.ReturnToPatrol:
                m_targetWaypoint = m_myRoute.GetWaypoint(m_routeWaypointID); //m_myRoute.GetClosestWaypoint(transform.position, out m_routeWaypointID);
                m_myLegs.m_runMode = false;
                m_myLegs.SetTargetDestinaton(m_targetWaypoint);
                m_myLegs.LookAtTarget();
                break;
            case AI_State.Patrol:
                if (m_routeWaypointID == -1)
                    m_targetWaypoint = m_myRoute.GetNextWaypoint(transform.position, out m_routeWaypointID);
                else
                    m_targetWaypoint = m_myRoute.GetWaypoint(m_routeWaypointID);

                m_routeWaypointID = m_myRoute.IncrementIndex(m_routeWaypointID);

                m_myLegs.m_runMode = false;
                m_myLegs.SetTargetDestinaton(m_targetWaypoint);
                m_myLegs.LookAtVelocity();
                break;
            case AI_State.Alert:
                m_myLegs.m_runMode = true;
                break;
            case AI_State.Investigating:
                m_myLegs.m_runMode = true;
                m_idleTimer += 3.5f;
                break;
            case AI_State.Hunting:
                m_myLegs.m_runMode = true; 
                break;
            case AI_State.Engaging:
                m_myLegs.m_runMode = true;
                m_myLegs.LookAtTarget();
                //m_myLegs.LookAtDirection(m_targetWaypoint - transform.position);
                //m_animator.Engage();
                break;
            case AI_State.Dead:
                m_myLegs.m_runMode = false;
                m_mySight.gameObject.SetActive(false);
                m_attentionBar.transform.parent.gameObject.SetActive(false);
                m_myLegs.Halt();
                m_animator.SetDead();
                break;
            case AI_State.Meeting:
                m_myLegs.SetTargetDestinaton(m_meetingLoc, 2.0f, 3.0f);
                m_myLegs.LookAtTarget();
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

    public void SpawnProjectile(GameObject prefab)
    {
        Vector3 direction = (m_targetTransform.transform.position - m_shotOrigin.transform.position).normalized;
        m_agent.Play("Gunshot");
        Rigidbody proj = Instantiate(prefab, m_shotOrigin.transform.position, Quaternion.LookRotation(direction, Vector3.up)).GetComponent<Rigidbody>();
        proj.AddForce(direction * 0.3f, ForceMode.Impulse);
        m_shotDelay += 0.3f;
    }

    public void PlayFootStep()
    {
        m_agent.PlayOnce("GuardFootstep", false, UnityEngine.Random.Range(0.85f, 1.25f));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if(m_myState != AI_State.Meeting)
        {
            Gizmos.DrawSphere(m_targetWaypoint, 0.4f);
        }
        else
        {
            Gizmos.DrawSphere(m_meetingLoc, 0.4f);
        }

        if(m_currentInterest.HasValue)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(m_currentInterest.Value.lastKnownLocation, 1.0f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, m_immuneRange, 0) * transform.forward) * m_maxKillDist);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, -m_immuneRange, 0) * transform.forward) * m_maxKillDist);
    }
}
