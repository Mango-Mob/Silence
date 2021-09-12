using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AI_Path : MonoBehaviour
{
    public List<Transform> m_points;
    public bool ShouldReset = false;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            m_points.Add(transform.GetChild(i));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(ShouldReset)
        {
            m_points.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                m_points.Add(transform.GetChild(i));
            }
            ShouldReset = false;
        }
    }

    public void OnDrawGizmos()
    {
        for (int i = 0; i < transform.childCount - 1; i++)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(m_points[i].position, m_points[i+1].position);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(m_points[i].position, m_points[i].forward);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_points[i].position, 0.25f);
        }

        if(m_points.Count > 2)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(m_points[0].position, m_points[m_points.Count - 1].position);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(m_points[m_points.Count - 1].position, m_points[m_points.Count - 1].forward);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_points[m_points.Count - 1].position, 0.25f);
        }
    }

    public bool IsOnPath(Vector3 P)
    {
        for (int i = 0; i < m_points.Count - 1; i++)
        {
            Vector3 A = m_points[i].position;
            Vector3 B = m_points[i + 1].position;

            Vector3 AP = P - A;
            Vector3 AB = B - A;

            //dotAP/dotAB
            float t = Vector3.Dot(AP, AB)/Vector3.Dot(AB,AB);
            Vector3 proj = A + t * AB;
            if((t >= 0f && t <= 1f) && Vector3.Distance(P, proj) < 0.5f)
            {
                return true;
            }
        }
        
        return false;
    }

    public int IncrementIndex(int index)
    {
        index += 1;
        if (index > m_points.Count)
            index -= m_points.Count;
        return index;
    }

    public Vector3 GetWaypoint(int index)
    {
        if (index < 0)
            return Vector3.zero;

        if (index >= m_points.Count)
            return m_points[0].position;

        return m_points[index].position;
    }

    public Vector3 GetNextWaypoint(Vector3 position)
    {
        float dist = float.MaxValue;
        int result = -1;
        for (int i = 0; i < m_points.Count; i++)
        {
            float curr = Vector3.Distance(m_points[i].position, position);
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

        return m_points[++result].position;
    }

    public Vector3 GetNextWaypoint(Vector3 position, out int resultIndex)
    {
        float dist = float.MaxValue;
        int result = -1;
        for (int i = 0; i < m_points.Count; i++)
        {
            float curr = Vector3.Distance(m_points[i].position, position);
            if (curr < dist)
            {
                dist = curr;
                result = i;
            }
        }

        resultIndex = result;

        if (result == -1)
            return position;

        if (result == m_points.Count - 1)
            result = -1;

        resultIndex = result;

        return m_points[++result].position;
    }

    public Quaternion GetLookDirection(Vector3 position)
    {
        if(m_points.Count > 1)
        {
            float dist = float.MaxValue;
            Transform result = m_points[0];
            foreach (var item in m_points)
            {
                float curr = Vector3.Distance(item.position, position);
                if (curr < dist)
                {
                    dist = curr;
                    result = item;
                }
            }
            Debug.DrawRay(result.position, result.forward * 5, Color.cyan, 0.5f);
            return Quaternion.LookRotation(result.forward, Vector3.up);
        }
        return Quaternion.identity;
    }

    public Vector3 GetClosestWaypoint(Vector3 position)
    {
        float dist = float.MaxValue;
        Vector3 result = position;
        foreach (var item in m_points)
        {
            float curr = Vector3.Distance(item.position, position);
            if(curr < dist)
            {
                dist = curr;
                result = item.position;
            }
        }
        return result;
    }

    public Vector3 GetClosestWaypoint(Vector3 position, out int index)
    {
        float dist = float.MaxValue;
        Vector3 result = position;
        int resultIndex = -1;
        for (int i = 0; i < m_points.Count; i++)
        {
            float curr = Vector3.Distance(m_points[i].position, position);
            if (curr < dist)
            {
                dist = curr;
                result = m_points[i].position;
                resultIndex = i;
            }
        }
        index = resultIndex;

        return result;
    }
}
