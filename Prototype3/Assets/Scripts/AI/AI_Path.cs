using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AI_Path : MonoBehaviour
{
    public List<GameObject> m_points;
    public bool m_showPoints = false;
    public LineRenderer m_renderer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        int i = 0;
        m_renderer.positionCount = m_points.Count + 1;
        m_renderer.enabled = m_showPoints;
        foreach (var item in m_points)
        {
            m_renderer.SetPosition(i++, item.transform.position);
            item.SetActive(m_showPoints);
        }
        m_renderer.SetPosition(i, m_points[0].transform.position);
#endif
    }
    public bool IsOnPath(Vector3 P)
    {
        for (int i = 0; i < m_points.Count - 1; i++)
        {
            Vector3 A = m_points[i].transform.position;
            Vector3 B = m_points[i + 1].transform.position;

            Vector3 AP = P - A;
            Vector3 AB = B - A;

            //dotAP/dotAB
            float t = Vector3.Dot(AP, AB)/Vector3.Dot(AB,AB);
            Vector3 proj = A + t * AB;
            if((t >= 0f || t <= 1f) && Vector3.Distance(P, proj) < 2f)
            {
                return true;
            }
        }
        
        return false;
    }
    public Vector3 GetNextWaypoint(Vector3 position)
    {
        float dist = float.MaxValue;
        int result = -1;
        for (int i = 0; i < m_points.Count; i++)
        {
            float curr = Vector3.Distance(m_points[i].transform.position, position);
            if (curr < dist)
            {
                dist = curr;
                result = i;
            }
        }

        if(result == -1)
            return position;

        if(result == m_points.Count - 1)
            result = -1;

        return m_points[++result].transform.position;
    }

    public Vector3 GetClosestWaypoint(Vector3 position)
    {
        float dist = float.MaxValue;
        Vector3 result = position;
        foreach (var item in m_points)
        {
            float curr = Vector3.Distance(item.transform.position, position);
            if(curr < dist)
            {
                dist = curr;
                result = item.transform.position;
            }
        }
        return result;
    }
}
