using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    private PlayerCamera playerCamera;
    private PlayerMovement playerMovement;
    public GameObject currentInteractable;
    public float m_interactRange = 3.0f;
    public LayerMask m_layerMask;
    public bool m_hasKnife = true;

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = GetComponent<PlayerCamera>();
        playerMovement = GetComponent<PlayerMovement>();
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

        if (InputManager.instance.IsKeyDown(KeyType.E) && !playerMovement.m_dead)
        {
            if (currentInteractable != null && currentInteractable.GetComponent<Interactable>())
            {
                playerMovement.m_animator.SetTrigger("Grab");
                NoiseManager.instance.CreateNoise(transform.position, 4.0f, playerMovement.m_noiseMask, Time.deltaTime);
                currentInteractable.GetComponent<Interactable>().Interact();
            }
        }

        if (InputManager.instance.GetMouseDown(MouseButton.LEFT) && m_hasKnife && !playerMovement.m_dead)
        {
            playerMovement.m_animator.SetTrigger("Stab");
            playerMovement.audioAgent.Play("Stab");
        }
    }

    public void PlayPickupSound(bool isLoot)
    {
        if(isLoot)
        {
            playerMovement.audioAgent.Play("Loot");
        }
        else
        {
            playerMovement.audioAgent.Play("Pickup");
        }
    }

    public void Stab()
    {
        if (currentInteractable != null && currentInteractable.GetComponent<AI_Brain>())
        {
            if (currentInteractable.GetComponent<AI_Brain>().KillGuard(transform.position))
            {
                m_hasKnife = false;
                NoiseManager.instance.CreateNoise(transform.position, 16.0f, playerMovement.m_noiseMask, Time.deltaTime);
            }
        }
    }
}
