using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private PlayerMovement playerMovement;
    [Header("Sensitivity")]
    [Range(0.0f, 100.0f)]
    public float m_mouseSentivityX = 50.0f;
    [Range(0.0f, 100.0f)]
    public float m_mouseSentivityY = 50.0f;

    public float m_xRotation = 0.0f;
    public float m_zRotation = 0.0f;

    public Camera m_camera { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        m_camera = GetComponentInChildren<Camera>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = 0.0f;
        float mouseY = 0.0f;
        if (!playerMovement.m_dead)
        {
            Vector2 mouseMovement = InputManager.instance.GetMouseDelta();
            mouseX = mouseMovement.x * m_mouseSentivityX * Time.deltaTime;
            mouseY = mouseMovement.y * m_mouseSentivityY * Time.deltaTime;
        }

        m_xRotation -= mouseY;
        m_xRotation = Mathf.Clamp(m_xRotation, -90.0f, 90.0f);

        m_camera.transform.localRotation = Quaternion.Euler(m_xRotation, 0.0f, m_zRotation);

        if (!playerMovement.m_dead)
            transform.Rotate(Vector3.up * mouseX);
    }
}
