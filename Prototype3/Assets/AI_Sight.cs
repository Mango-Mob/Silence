using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AI_Sight : MonoBehaviour
{
    private struct Interest
    {
        public Interest(Collider _reference, Vector3 _location)
        {
            reference = _reference;
            lastKnownLocation = _location;
            m_lastSeen = DateTime.Now;
        }

        public Collider reference;
        public Vector3 lastKnownLocation;
        public DateTime m_lastSeen;

        public double GetAge()
        {
            return (DateTime.Now - m_lastSeen).TotalSeconds;
        }
    }

    [Header("Sight variables")]
    public float m_sightDegrees = 45f;
    public float m_sightRange = 5f;
    public float m_castRadius = 1f;
    public float m_memoryDuration = 5.0f;
    public LayerMask m_sightLayer;

    private List<Collider> m_collidersWithin;
    private List<Interest> m_interests = new List<Interest>();

    private GUIStyle m_debugStyle;
    // Start is called before the first frame update
    void Start()
    {
        m_debugStyle = new GUIStyle();
        m_debugStyle.fontSize = 18;

    }

    // Update is called once per frame
    void Update()
    {
        UpdateDetection();
        UpdateInterest();
    }

    public bool IsWithinSight(Vector3 position)
    {
        Quaternion lookTo = Quaternion.LookRotation((position - transform.position).normalized);

        return Mathf.Abs(Quaternion.Angle(transform.rotation, lookTo)) <= m_sightDegrees;
    }

    public bool CanRaycastTo(Collider other)
    {
        Vector3 direction = (other.transform.position - transform.position).normalized;

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, m_castRadius, direction, m_sightRange);
        float dist = float.MaxValue;
        if(hits.Length == 0)
        {
            return false;
        }

        RaycastHit closestHit = hits[0];
        if(hits.Length > 1)
        {
            foreach (var hit in hits)
            {   
                if(hit.collider.gameObject.layer != LayerMask.NameToLayer("Environment"))
                {
                    float curr = Vector3.Distance(hit.point, transform.position);
                    if (curr < dist)
                    {
                        dist = curr;
                        closestHit = hit;
                    }
                }
            }
        }

        return closestHit.collider == other;
    }

    private void UpdateInterest()
    {
        //Remove from the start at the list, because they are added in age order.
        while (m_interests.Count > 0 && m_interests[0].GetAge() >= m_memoryDuration)
        {
            m_interests.RemoveAt(0);
        }
    }

    private void UpdateDetection()
    {
        List<Collider> newList = new List<Collider>(Physics.OverlapSphere(transform.position, m_sightRange, m_sightLayer));

        //Search the whole list for any within sight
        for (int i = newList.Count - 1; i >= 0; i--)
        {
            if (IsWithinSight(newList[i].transform.position) && CanRaycastTo(newList[i]))
            {
                //Remove it from the last know locations
                RemoveFromInterest(newList[i]);
            }
            else
            {
                //Not in sight, remove
                newList.RemoveAt(i);
            }
        }

        if(m_collidersWithin != null)
        {
            //Check if there is a differnce between frames
            foreach (var item in m_collidersWithin)
            {
                //If the new list doesn't contain the item
                if (!newList.Contains(item))
                {
                    //Log it as an interest
                    m_interests.Add(new Interest(item, item.transform.position));
                }
            }
        }
        
        m_collidersWithin = newList;
    }

    private void RemoveFromInterest(Collider reference)
    {
        for (int i = m_interests.Count - 1; i >= 0; i--)
        {
            if (m_interests[i].reference == reference)
            {
                m_interests.RemoveAt(i);
            }
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * m_sightRange);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, m_sightDegrees, 0) * transform.forward) * m_sightRange);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, -m_sightDegrees, 0) * transform.forward) * m_sightRange);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(m_sightDegrees, 0, 0) * transform.forward) * m_sightRange);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(-m_sightDegrees, 0, 0) * transform.forward) * m_sightRange);

        if(m_collidersWithin != null)
        {
            foreach (var item in m_collidersWithin)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, item.transform.position);
                Gizmos.DrawWireSphere(item.transform.position, m_castRadius);
#if UNITY_EDITOR
                m_debugStyle.normal.textColor = Color.green;
                Handles.Label(item.transform.position, $"{0.000}s", m_debugStyle);
#endif
            }
        }

        foreach (var item in m_interests)
        { 
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, item.lastKnownLocation);
            Gizmos.DrawWireSphere(item.lastKnownLocation, m_castRadius);
#if UNITY_EDITOR
            m_debugStyle.normal.textColor = Color.red;
            Handles.Label(item.lastKnownLocation, $"{item.GetAge().ToString("F3")}s", m_debugStyle);
#endif
        }

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, m_sightRange);
    }
}
