using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Brain : MonoBehaviour
{
    private AI_Legs m_myLegs;
    [SerializeField] private AI_Path m_myRoute;
    private AI_Sight m_mySight;

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
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
        Gizmos.DrawSphere(m_targetWaypoint, 0.5f);
    }
}
