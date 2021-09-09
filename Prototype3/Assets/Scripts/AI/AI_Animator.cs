using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Animator : MonoBehaviour
{
    public string HorizVelocityName = "HorizontalVelocity";
    public string VertVelocityName = "VerticalVelocity";
    public string VelocityNetName = "VelocityNet";

    private Animator m_myAnimator;

    private void Awake()
    {
        m_myAnimator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetVelocity(Vector2 velocity)
    {
        float horiz = Mathf.Clamp(velocity.x, -1f, 1f);
        float vert = Mathf.Clamp(velocity.y, -1f, 1f);
        float net = Mathf.Max(Mathf.Abs(horiz), Mathf.Abs(vert));

        m_myAnimator.SetFloat(HorizVelocityName, horiz);
        m_myAnimator.SetFloat(VertVelocityName, vert);
        m_myAnimator.SetFloat(VelocityNetName, net);
    }
}
