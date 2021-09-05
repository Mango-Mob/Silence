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
}
