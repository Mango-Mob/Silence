using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastDebugger : MonoBehaviour
{
    public enum Direction { FORWARD, RIGHT, UP };
    public Direction m_direction;
    public float dist;
    public Color col;

    // Update is called once per frame
    void Update()
    {
        switch (m_direction)
        {
            case Direction.FORWARD:
                Debug.DrawRay(transform.position, transform.forward * dist, col);
                break;
            case Direction.RIGHT:
                Debug.DrawRay(transform.position, transform.right * dist, col);
                break;
            case Direction.UP:
                Debug.DrawRay(transform.position, transform.up * dist, col);
                break;
            default:
                break;
        }
        
    }
}
