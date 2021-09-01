using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public PlayerCamera playerCamera { get; private set; }

    [Header("Movement Attributes")]
    public float m_speed = 8.0f;
    public float m_crouchSpeed = 4.0f;
    public float m_jumpSpeed = 5.0f;
    public float m_playerGravity = 9.81f;
    public float m_damping = 0.5f;
    public float m_airAcceleration = 1.0f;

    [Header("Head Collision")]
    public Transform m_headCollisionPosition;
    public LayerMask m_headCollisionMask;

    private bool m_isCrouching = false;
    private float m_cameraOffset = 0.0f;
    private float m_crouchLerp = 1.0f;

    private Vector3 m_velocity = Vector3.zero;
    private CharacterController charController;
    private bool m_grounded = false;

    // Start is called before the first frame update
    void Start()
    {
        charController = GetComponent<CharacterController>();
        playerCamera = GetComponent<PlayerCamera>();
        m_cameraOffset = playerCamera.m_camera.transform.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        bool leftGround = false;
        if (!charController.isGrounded && m_grounded)
        {
            leftGround = true;
        }
        m_grounded = charController.isGrounded;

        // Player movement
        Vector2 movementInput;
        movementInput = GetMovementInput();
        Vector3 moveDirection = transform.right * movementInput.x + transform.forward * movementInput.y;

        float currentSpeed = (m_crouchLerp < 0.5f) ? m_crouchSpeed : m_speed;

        if (leftGround)
        {
            m_velocity.x = moveDirection.x * currentSpeed;
            m_velocity.z = moveDirection.z * currentSpeed;
        }

        // Air Acceleration
        if (!m_grounded)
        {
            #region Terrible Code
            //if (horizontalVelocity.normalized.x > 0)
            //{
            //    if (horizontalVelocity.normalized.x > moveDirection.x)
            //    {
            //        m_velocity.x += moveDirection.x * m_airAcceleration * Time.deltaTime;
            //    }
            //}
            //else if(horizontalVelocity.normalized.x < 0)
            //{
            //    if (horizontalVelocity.normalized.x < moveDirection.x)
            //    {
            //        m_velocity.x += moveDirection.x * m_airAcceleration * Time.deltaTime;
            //    }
            //}

            //if (horizontalVelocity.normalized.z > 0)
            //{
            //    if (horizontalVelocity.normalized.z > moveDirection.z)
            //    {
            //        m_velocity.z += moveDirection.z * m_airAcceleration * Time.deltaTime;
            //    }
            //}
            //else if (horizontalVelocity.normalized.z < 0)
            //{
            //    if (horizontalVelocity.normalized.z < moveDirection.z)
            //    {
            //        m_velocity.z += moveDirection.y * m_airAcceleration * Time.deltaTime;
            //    }
            //}
            #endregion
            
            m_velocity += moveDirection * m_airAcceleration * Time.deltaTime;

            Vector3 horizontalVelocity = m_velocity;
            horizontalVelocity.y = 0.0f;

            if (horizontalVelocity.magnitude > m_speed)
            {
                horizontalVelocity = horizontalVelocity.normalized * m_speed;
            }

            m_velocity.x = horizontalVelocity.x;
            m_velocity.z = horizontalVelocity.z;

            moveDirection = Vector2.zero;
        }

        // Grounded checks
        if (m_grounded && m_velocity.y < 0.0f)
        {
            m_velocity = Vector3.zero;
            m_velocity.y = -1.0f;
        }
        else
        {
            m_velocity.y -= m_playerGravity * Time.deltaTime;

            // Velocity damping
            m_velocity.x -= m_velocity.x * m_damping * Time.deltaTime;
            m_velocity.z -= m_velocity.z * m_damping * Time.deltaTime;
        }

        // Jumping
        if (charController.isGrounded && InputManager.instance.IsKeyDown(KeyType.SPACE))
        {
            //m_velocity = moveDirection * m_speed;
            m_velocity.y = m_jumpSpeed;
        }

        // Crouching
        if (InputManager.instance.IsKeyDown(KeyType.L_CTRL))
        {
            m_isCrouching = !m_isCrouching;
        }

        if (m_isCrouching)
            m_crouchLerp -= Time.deltaTime * 10.0f;
        else if (!Physics.CheckSphere(playerCamera.m_camera.transform.position, charController.radius * 1.1f, m_headCollisionMask))
            m_crouchLerp += Time.deltaTime * 10.0f;

        m_crouchLerp = Mathf.Clamp(m_crouchLerp, 0.0f, 1.0f);

        playerCamera.m_camera.transform.localPosition = new Vector3(0, Mathf.Lerp(0.0f, m_cameraOffset, m_crouchLerp), 0);

        float newHeight = Mathf.Lerp(0.5f, 2.0f, m_crouchLerp);
        float deltaHeight = newHeight - charController.height;
        charController.height = newHeight;

        charController.Move(moveDirection * currentSpeed * Time.deltaTime + m_velocity * Time.deltaTime + 0.5f * deltaHeight * Vector3.up);
    }
    private Vector2 GetMovementInput()
    {
        Vector2 movementInput = Vector2.zero;
        movementInput.x += (InputManager.instance.IsKeyPressed(KeyType.D) ? 1.0f : 0.0f);
        movementInput.x -= (InputManager.instance.IsKeyPressed(KeyType.A) ? 1.0f : 0.0f);
        movementInput.y += (InputManager.instance.IsKeyPressed(KeyType.W) ? 1.0f : 0.0f);
        movementInput.y -= (InputManager.instance.IsKeyPressed(KeyType.S) ? 1.0f : 0.0f);

        if (movementInput.magnitude != 0.0f)
            movementInput.Normalize();

        return movementInput;
    }
}
