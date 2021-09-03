using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AI_Hearing : MonoBehaviour
{
    private struct NoiseInterest
    {
        public NoiseInterest(Vector3 _location)
        {
            lastKnownLocation = _location;
            m_lastSeen = DateTime.Now;
        }

        public Vector3 lastKnownLocation;
        public DateTime m_lastSeen;

        public double GetAge()
        {
            return (DateTime.Now - m_lastSeen).TotalSeconds;
        }
        public void Refesh()
        {
            m_lastSeen = DateTime.Now;
        }
    }

    public float m_hearingRange;
    public GameObject debugGameObject;
    public LayerMask m_recieverLayer;
    public float m_memoryDuration;

    private AI_Sight m_sight;
    private NoiseListener m_myListener;
    private List<NoiseInterest> m_interests = new List<NoiseInterest>();
    private GUIStyle m_debugStyle;

    private void Awake()
    {
        m_debugStyle = new GUIStyle();
        m_debugStyle.fontSize = 18;
        m_myListener = new NoiseListener(gameObject, m_hearingRange, m_recieverLayer);
        m_sight = GetComponent<AI_Sight>();
    }

    // Start is called before the first frame update
    void Start()
    {
        NoiseManager.instance.Subscribe(m_myListener);
    }
    
    // Update is called once per frame
    void Update()
    {
        if(InputManager.instance.IsKeyDown(KeyType.J))
        {
            NoiseManager.instance.CreateNoise(debugGameObject.transform.position, 2.0f, debugGameObject.layer, 1.0f);
        }
        
        DetectionUpdate();
        UpdateInterest();
        SightCheck();
    }

    private void OnDestroy()
    {
        NoiseManager.instance.UnSubscribe(m_myListener);
    }

    private void DetectionUpdate()
    {
        foreach (var item in m_myListener.newLocations)
        {
            bool found = false;
            foreach (var interest in m_interests)
            {
                if (interest.lastKnownLocation == item)
                {
                    interest.Refesh();
                    found = true;
                }
            }
            if (!found)
            {
                m_interests.Add(new NoiseInterest(item));
            }
        }
        m_myListener.Clear();
    }

    private void SightCheck()
    {
        if(m_sight != null)
        {
            for (int i = m_interests.Count - 1; i >= 0; i--)
            { 
                if(m_sight.IsWithinSight(m_interests[i].lastKnownLocation) 
                    && m_sight.CanRaycastToPosition(m_interests[i].lastKnownLocation))
                {
                    m_interests.RemoveAt(i);
                }
            }
        }
    }
    private void UpdateInterest()
    {
        //Remove from the start at the list, because they are added in age order.
        while (m_interests.Count > 0 && m_interests[0].GetAge() >= m_memoryDuration)
        {
            m_interests.RemoveAt(0);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, m_hearingRange);

        foreach (var item in m_interests)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, item.lastKnownLocation);
            Gizmos.DrawWireSphere(item.lastKnownLocation, 1.0f);
#if UNITY_EDITOR
            m_debugStyle.normal.textColor = Color.red;
            Handles.Label(item.lastKnownLocation, $"{item.GetAge().ToString("F3")}s", m_debugStyle);
#endif
        }
    }
}
