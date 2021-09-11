using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProjectile : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //collision.rigidbody.gameObject.GetComponents<PlayerMovement>().Kill();
        }
        Destroy(gameObject);
    }
}
