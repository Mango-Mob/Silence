using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    #region Singleton

    private static NoiseManager _instance = null;
    public static NoiseManager instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject loader = new GameObject();
                _instance = loader.AddComponent<NoiseManager>();
                loader.name = "Noise Manager";
                return loader.GetComponent<NoiseManager>();
            }
            return _instance;
        }
    }

    public void CreateNoise(Vector3 position, float range, LayerMask layer, float duration, float delay = 0.0f)
    {
        m_noises.Add(new Noise(position, range, delay, duration, layer));
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        if (_instance == this)
        {
            InitialFunc();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Second Instance of NoiseManager was created, this instance was destroyed.");
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
    #endregion

    public class Noise
    {
        public Noise(Vector3 pos, float maxRange, float timeToMax, float age, LayerMask layer)
        {
            m_position = pos;
            m_maxRange = maxRange;
            m_timeToMax = timeToMax;
            m_range = 0.0f;
            m_rangeDelta = m_timeToMax / m_maxRange;
            m_age = age;
            m_layer = layer;
        }

        public Vector3 m_position;
        public float m_range;
        public float m_age;
        public LayerMask m_layer;

        private float m_timeToMax;
        private float m_rangeDelta;
        private float m_maxRange;
        
        public void Update()
        {
            if (m_timeToMax > 0)
            {
                m_range += m_rangeDelta * Time.deltaTime;
                m_timeToMax -= Time.deltaTime;
            }
            else
                m_range = m_maxRange;

            m_age -= Time.deltaTime;
        }
    }

    private List<NoiseListener> m_subscribers;
    private List<Noise> m_noises;
    private void InitialFunc()
    {
        m_noises = new List<Noise>();
        m_subscribers = new List<NoiseListener>();
    }

    // Update is called once per frame
    void Update()
    {
        int i = 0;
        while(m_noises.Count != 0 && i < m_noises.Count)
        {
            if(m_noises[i].m_age <= 0)
            {
                m_noises.RemoveAt(i);
            }
            else
            {
                m_noises[i].Update();
                i++;
            }
        }
        foreach (var subcriber in m_subscribers)
        {
            foreach (var noise in m_noises)
            {
                float dist = Vector3.Distance(subcriber.owner.transform.position, noise.m_position);
                if(dist < Mathf.Sqrt(Mathf.Pow(noise.m_range, 2) + Mathf.Pow(subcriber.range, 2)))
                {
                    subcriber.Notify(noise);
                }
            }
        }
    }

    public void Subscribe(NoiseListener newListener)
    {
        m_subscribers.Add(newListener);
    }

    public void UnSubscribe(NoiseListener newListener)
    {
        m_subscribers.Remove(newListener);
    }

    public void OnDrawGizmos()
    {
        if(m_noises != null)
            foreach (var item in m_noises)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(item.m_position, item.m_range);
            }
    }
}
