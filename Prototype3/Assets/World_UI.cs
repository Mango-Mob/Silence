using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World_UI : MonoBehaviour
{
    public Transform m_worldTransform;
    public LayerMask m_filters;
    private float m_maxDist = 20;

    public GameObject m_toRender;
    // Update is called once per frame
    void Update()
    {
        m_toRender.transform.position = Camera.main.WorldToScreenPoint(m_worldTransform.position);
        float dist = Vector3.Distance(m_worldTransform.position, Camera.main.transform.position);
        Ray ray = Camera.main.ScreenPointToRay(m_toRender.transform.position);

        if(!Physics.Raycast(ray, dist, m_filters) && dist <= m_maxDist)
        {
            m_toRender.SetActive(true);
        }
        else
        {
            m_toRender.SetActive(false);
        }
    }
}
