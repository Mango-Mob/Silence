using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BillboardScript : MonoBehaviour
{
    void Update()
    {
        if(Camera.main != null)
            transform.forward = -Camera.main.transform.forward;
    }
}
