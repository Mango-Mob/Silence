using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AI_Sight : MonoBehaviour
{
    private struct ViewCastInfo
    {
        public ViewCastInfo(bool _hit, Vector3 _point, float _dist, float _angle)
        {
            hit = _hit;
            point = _point;
            dist = _dist;
            angle = _angle;
        }

        public bool hit;
        public Vector3 point;
        public float dist;
        public float angle;
    }

    private struct EdgeInfo
    {
        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }

        public Vector3 pointA;
        public Vector3 pointB;
    }

    [Header("Sight variables")]
    public float m_sightDegrees = 45f;
    public float m_sightRange = 5f;
    public float m_castRadius = 1f;
    public float m_memoryDuration = 5.0f;
    public LayerMask m_sightLayer;

    [Header("Render variables")]
    [Range(0f, 1f)]
    public float m_renderResolution;
    public LayerMask m_obstacleMask;
    public MeshFilter m_viewMeshFilter;
    public int m_edgeResolveIteration;
    public float m_edgeDistThreshold;
    private Mesh m_viewMesh;
    private bool m_activeSight = true;

    public List<Collider> m_collidersWithinSight;
    public List<AI_Interest> m_interests = new List<AI_Interest>();

    private GUIStyle m_debugStyle;
    // Start is called before the first frame update
    void Start()
    {
        m_debugStyle = new GUIStyle();
        m_debugStyle.fontSize = 18;

        m_viewMesh = new Mesh();
        m_viewMesh.name = "View Mesh";
        m_viewMeshFilter.mesh = m_viewMesh;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDetection();
        UpdateInterest();
    }
        

    private void LateUpdate()
    {
        RaycastForRender();
    }

    #region Rendering
    private void RaycastForRender()
    {
        int stepCount = Mathf.RoundToInt(2*m_sightDegrees * m_renderResolution);
        float stepAngleSize = (2*m_sightDegrees) / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - m_sightDegrees + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);
            if(i > 0)
            {
                bool edgeDistThresholdExceeded = Mathf.Abs(oldViewCast.dist - newViewCast.dist) > m_edgeDistThreshold;
                if(oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDistThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if(edge.pointA != Vector3.zero)
                        viewPoints.Add(edge.pointA);
                    if (edge.pointB != Vector3.zero)
                        viewPoints.Add(edge.pointB);
                }
            }
            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2)*6];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if(i < vertexCount - 2)
            {
                triangles[i * 6] = 0;
                triangles[i * 6 + 1] = i + 1;
                triangles[i * 6 + 2] = i + 2;
                triangles[i * 6 + 3] = 0;
                triangles[i * 6 + 4] = i + 2;
                triangles[i * 6 + 5] = i + 1;
            }
        }

        m_viewMesh.Clear();
        m_viewMesh.vertices = vertices;
        m_viewMesh.triangles = triangles;
        m_viewMesh.RecalculateNormals();
    }

    private ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, m_sightRange, m_obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * m_sightRange, m_sightRange, globalAngle);
        }
    }
    private EdgeInfo FindEdge(ViewCastInfo minView, ViewCastInfo maxView)
    {
        float minAngle = minView.angle;
        float maxAngle = maxView.angle;

        Vector3 minPoint = minView.point;
        Vector3 maxPoint = maxView.point;

        for (int i = 0; i < m_edgeResolveIteration; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDistThresholdExceeded = Mathf.Abs(minView.dist - newViewCast.dist) > m_edgeDistThreshold;

            if (newViewCast.hit == minView.hit && !edgeDistThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }
        return new EdgeInfo(minPoint, maxPoint);
    }

    private Vector3 DirFromAngle(float angle, bool angleIsGlobal)
    {
        if (angleIsGlobal)
        {
            angle -= transform.rotation.eulerAngles.y;
        }
        return (Quaternion.AngleAxis(angle, Vector3.up)) * transform.forward;
    }
    #endregion

    public bool IsWithinSight(Vector3 position)
    {
        Quaternion lookTo = Quaternion.LookRotation((position - transform.position).normalized);

        return Mathf.Abs(Quaternion.Angle(transform.rotation, lookTo)) <= m_sightDegrees;
    }

    public bool CanRaycastToPosition(Vector3 position)
    {
        Vector3 direction = (position - transform.position).normalized;
        RaycastHit hit;

        return Physics.SphereCast(transform.position, m_castRadius, direction, out hit, m_sightRange);
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
                if(hit.collider.gameObject.layer != LayerMask.NameToLayer("Enemy"))
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
        if(newList.Count >= 2)
        {
            int j = 0;
        }
        //Search the whole list for any within sight
        for (int i = newList.Count - 1; i >= 0; i--)
        {
            if (IsWithinSight(newList[i].transform.position) && CanRaycastTo(newList[i]))
            {
                //Remove it from the last know locations
                if (newList[i].gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    if (newList[i].gameObject.GetComponent<PlayerMovement>().m_visibility == 0.0f)
                    {
                        newList.RemoveAt(i);
                    }
                    else
                    {
                        RemoveFromInterest(newList[i]);
                    }
                }
                else
                {
                    RemoveFromInterest(newList[i]);
                }
            }
            else
            {
                //Not in sight, remove
                newList.RemoveAt(i);
            }
        }

        if(m_collidersWithinSight != null)
        {
            //Check if there is a differnce between frames
            foreach (var item in m_collidersWithinSight)
            {
                //If the new list doesn't contain the item
                if (!newList.Contains(item))
                {
                    //Log it as an interest
                    m_interests.Add(new AI_Interest(item.transform.position, item));
                }
            }
        }

        m_collidersWithinSight = newList;
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

    public void OnDisable()
    {
        m_viewMesh.Clear();
    }
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * m_sightRange);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, m_sightDegrees, 0) * transform.forward) * m_sightRange);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, -m_sightDegrees, 0) * transform.forward) * m_sightRange);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(m_sightDegrees, 0, 0) * transform.forward) * m_sightRange);
        Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(-m_sightDegrees, 0, 0) * transform.forward) * m_sightRange);

        if(m_collidersWithinSight != null)
        {
            foreach (var item in m_collidersWithinSight)
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
