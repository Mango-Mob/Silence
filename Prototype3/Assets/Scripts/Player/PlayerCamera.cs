using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Sensitivity")]
    [Range(0.0f, 100.0f)]
    public float m_mouseSentivityX = 50.0f;
    [Range(0.0f, 100.0f)]
    public float m_mouseSentivityY = 50.0f;

    private float m_xRotation = 0.0f;

    public Camera m_camera { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        m_camera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mouseMovement = InputManager.instance.GetMouseDelta();
        float mouseX = mouseMovement.x * m_mouseSentivityX * Time.deltaTime;
        float mouseY = mouseMovement.y * m_mouseSentivityY * Time.deltaTime;

        m_xRotation -= mouseY;
        m_xRotation = Mathf.Clamp(m_xRotation, -90.0f, 90.0f);

        m_camera.transform.localRotation = Quaternion.Euler(m_xRotation, 0.0f, 0.0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
