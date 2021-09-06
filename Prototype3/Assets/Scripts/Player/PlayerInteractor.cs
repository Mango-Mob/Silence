using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    private PlayerCamera playerCamera;
    public GameObject currentInteractable;
    public float m_interactRange = 3.0f;
    public LayerMask m_layerMask;

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = GetComponent<PlayerCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit[] rayCast = Physics.RaycastAll(playerCamera.m_camera.transform.position, playerCamera.m_camera.transform.forward, m_interactRange, m_layerMask);

        currentInteractable = null;
        float closestDistance = m_interactRange + 1.0f;
        foreach (var ray in rayCast)
        {
            float distance = Vector3.Distance(ray.point, playerCamera.m_camera.transform.position);
            if (distance < closestDistance)
            {
                currentInteractable = ray.collider.gameObject;
                closestDistance = distance;
            }
        }

        if (InputManager.instance.IsKeyDown(KeyType.E))
        {
            if (currentInteractable != null && currentInteractable.GetComponent<Interactable>())
            {
                currentInteractable.GetComponent<Interactable>().Interact();
            }
        }
    }
}
