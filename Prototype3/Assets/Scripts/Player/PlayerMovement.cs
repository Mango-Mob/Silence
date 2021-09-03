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

    private Vector3 m_lastPosition;
    private Vector3 m_calculatedVelocity = Vector3.zero;

    [Header("Head Collision")]
    public Transform m_headCollisionPosition;
    public LayerMask m_headCollisionMask;

    private bool m_isCrouching = false;
    private float m_cameraOffset = 0.0f;
    private float m_crouchLerp = 1.0f;

    private Vector3 m_velocity = Vector3.zero;
    private CharacterController charController;
    private bool m_grounded = false;

    [Header("Grappling Hook")]
    public LineRenderer m_grappleSource;
    public float m_grappleRange = 15.0f;
    public float m_grappleAcceleration = 10.0f;
    public float m_maxGrappleSpeed = 20.0f;
    public float m_grappleProjectileSpeed = 5.0f;
    public Transform m_grappleEnd;

    private Vector3 m_grappleHitPos;
    private HookMode m_hookMode = HookMode.idle;
    private float m_grappleShotLerp = 0.0f;

    [Header("Wall Running")]
    public Transform m_wallColliderL;
    public Transform m_wallColliderR;
    public float m_wallRunGravity = 3.0f;
    private bool m_isWallRunning = false;
    public float m_cameraTiltSpeed = 1.0f;

    private WallDir m_currentWall = WallDir.none;


    private float m_tiltVelocity = 0.0f;
    private bool m_wallRunRefreshed = true;

    enum HookMode
    {
        idle,
        firing,
        firing_missed,
        retracting,
        pulling,
    }

    enum WallDir
    {
        left,
        right,
        none,
    }

    // Start is called before the first frame update
    void Start()
    {
        m_lastPosition = transform.position;
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
        Vector2 movementInput = Vector2.zero;
        if (!m_isWallRunning)
            movementInput = GetMovementInput();
        Vector3 moveDirection = transform.right * movementInput.x + transform.forward * movementInput.y;

        float currentSpeed = (m_crouchLerp < 0.5f) ? m_crouchSpeed : m_speed;

        if (leftGround)
        {
            m_velocity.x += moveDirection.x * currentSpeed;
            m_velocity.z += moveDirection.z * currentSpeed;
        }

        // Air Acceleration
        if (!m_grounded && !m_isWallRunning)
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

            if (m_hookMode != HookMode.pulling)
            m_velocity += moveDirection * m_airAcceleration * Time.deltaTime;

            Vector3 horizontalVelocity = m_velocity;
            horizontalVelocity.y = 0.0f;

            if (horizontalVelocity.magnitude > m_speed)
            {
                horizontalVelocity -= horizontalVelocity.normalized * m_speed * Time.deltaTime;
            }

            m_velocity.x = horizontalVelocity.x;
            m_velocity.z = horizontalVelocity.z;

            moveDirection = Vector2.zero;

            //m_calculatedVelocity = (transform.position - m_lastPosition) / Time.deltaTime;
            //m_lastPosition = transform.position;
            //Debug.Log((m_velocity - m_calculatedVelocity).magnitude);
            //m_velocity = Vector3.Lerp(m_velocity, m_calculatedVelocity, 0.1f);
        }


        // Grounded checks
        if (m_grounded && m_velocity.y < 0.0f && m_hookMode != HookMode.pulling)
        {
            m_velocity = Vector3.zero;
            m_velocity.y = -1.0f;
        }
        else
        {

            if (Vector3.Distance(playerCamera.m_camera.transform.position, m_grappleHitPos) > 5.0f || m_hookMode != HookMode.pulling)
            m_velocity.y -= ((!m_isWallRunning) ? m_playerGravity : m_wallRunGravity) * Time.deltaTime;

            // Velocity damping
            m_velocity.x -= m_velocity.x * m_damping * Time.deltaTime;
            m_velocity.z -= m_velocity.z * m_damping * Time.deltaTime;
        }

        // Jumping
        if (charController.isGrounded && InputManager.instance.IsKeyDown(KeyType.SPACE))
        {
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

        GrapplingHook();
        WallRunning();

        charController.Move(moveDirection * currentSpeed * Time.deltaTime + m_velocity * Time.deltaTime + 0.5f * deltaHeight * Vector3.up);
    }

    private void GrapplingHook()
    {
        m_grappleSource.SetPosition(0, m_grappleSource.transform.position);
        m_grappleSource.SetPosition(1, m_grappleEnd.position);

        if (InputManager.instance.GetMouseButtonDown(MouseButton.RIGHT))
        {
            RaycastHit rayHit;

            if (m_hookMode != HookMode.idle && m_hookMode != HookMode.retracting)
                m_hookMode = HookMode.retracting;

            if (m_hookMode == HookMode.idle)
            {
                if (Physics.Raycast(playerCamera.m_camera.transform.position, playerCamera.m_camera.transform.forward, out rayHit, m_grappleRange, m_headCollisionMask))
                {
                    m_grappleHitPos = rayHit.point;
                    m_hookMode = HookMode.firing;
                    m_grappleSource.enabled = true;
                }
                else
                {
                    m_hookMode = HookMode.firing_missed;
                    m_grappleHitPos = playerCamera.m_camera.transform.position + playerCamera.m_camera.transform.forward * m_grappleRange;
                    m_grappleSource.enabled = true;
                }
            }
        }


        switch (m_hookMode)
        {
            case HookMode.firing:
                m_grappleShotLerp += Time.deltaTime * m_grappleProjectileSpeed;
                if (m_grappleShotLerp >= 1.0f)
                {
                    m_hookMode = HookMode.pulling;
                    m_isWallRunning = false;
                    m_velocity = Vector3.zero;
                }
                break;
            case HookMode.firing_missed:
                m_grappleShotLerp += Time.deltaTime * m_grappleProjectileSpeed;
                if (m_grappleShotLerp >= 1.0f)
                {
                    m_hookMode = HookMode.retracting;
                }
                break;
            case HookMode.pulling:
                float distance = Vector3.Distance(playerCamera.m_camera.transform.position, m_grappleHitPos);
                m_velocity += (m_grappleSource.GetPosition(1) - transform.position).normalized * m_grappleAcceleration * Time.deltaTime;
                if (m_velocity.magnitude > m_maxGrappleSpeed)
                {
                    float mult = 1.0f;
                    if (distance < 6.5f)
                    {
                        mult = 1 - 0.5f * (distance / 6.5f);
                    }
                    m_velocity = m_velocity.normalized * m_maxGrappleSpeed * mult;
                }
                if (distance < 1.5f /*((charController.collisionFlags & CollisionFlags.CollidedAbove) != 0 || (charController.collisionFlags & CollisionFlags.CollidedSides) != 0)*/)
                {
                    m_hookMode = HookMode.retracting;
                    m_velocity /= 20.0f;
                }
                break;
            case HookMode.retracting:
                m_grappleShotLerp -= Time.deltaTime * m_grappleProjectileSpeed;
                if (m_grappleShotLerp <= 0.0f)
                {
                    m_hookMode = HookMode.idle;
                    m_grappleSource.enabled = false;
                }
                break;
            default:
                m_grappleShotLerp = 0.0f;
                break;
        }

        m_grappleEnd.position = Vector3.Lerp(m_grappleSource.transform.position, m_grappleHitPos, m_grappleShotLerp);

    }

    private Vector3 DEBUGDIRECTION;

    private void WallRunning()
    {
        Vector3 direction = transform.forward;

        if (m_grounded)
            m_wallRunRefreshed = true;

        if (((InputManager.instance.IsKeyDown(KeyType.SPACE) && m_wallRunRefreshed) || m_isWallRunning) && !m_grounded)
        {
            Collider closestCollider = null;
            Collider[] colliders = Physics.OverlapSphere(transform.position, 0.75f, m_headCollisionMask);

            float smallestDistance = 20.0f;

            foreach (var collider in colliders)
            {
                float distance = Vector3.Distance(transform.position, collider.ClosestPointOnBounds(transform.position));
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    closestCollider = collider;
                }
            }

            if (InputManager.instance.IsKeyDown(KeyType.SPACE) && m_isWallRunning)
            {
                m_isWallRunning = false;
                m_currentWall = WallDir.none;

                Vector3 jumpDir = transform.forward;
                m_velocity = transform.forward * m_speed;
                m_velocity.y = m_jumpSpeed;

                return;
            }

            if (closestCollider != null)
            {
                direction = (closestCollider.ClosestPoint(transform.position) - transform.position);
                direction.y = 0;
                direction.Normalize();

                // REMOVE LATER
                DEBUGDIRECTION = direction;
                

                if (!m_isWallRunning)
                {
                    Vector3 localDirection = direction.x * transform.forward + direction.z * transform.right;
                    if (localDirection.z < 0)
                    {
                        m_currentWall = WallDir.left;
                    }
                    else if (localDirection.z > 0)
                    {
                        m_currentWall = WallDir.right;
                    }
                    else
                    {
                        return;
                    }
                }


                m_isWallRunning = true;
                m_wallRunRefreshed = false;

                Vector2 perp = ((m_currentWall == WallDir.right) ? 1.0f : -1.0f) * Vector2.Perpendicular(new Vector2(direction.x, direction.z)) * m_speed;

                m_velocity.x = perp.x;
                m_velocity.z = perp.y;
                m_velocity.y = 0.0f;

                m_velocity += direction * 1.0f;
            }
            else
            {
                m_isWallRunning = false;
                m_currentWall = WallDir.none;
            }
        }
        else
        {
            m_isWallRunning = false;
            m_currentWall = WallDir.none;
        }

        //float targetLerp = 0.5f; 
        //switch (m_currentWall)
        //{
        //    case WallDir.left:
        //        targetLerp = 0.0f;
        //        break;
        //    case WallDir.right:
        //        targetLerp = 1.0f;
        //        break;
        //}


        float targetLerp = (Mathf.Sin(Vector3.SignedAngle(transform.forward, direction, Vector3.up) * Mathf.Deg2Rad) * 0.5f) + 0.5f;
        Debug.Log(targetLerp);
        playerCamera.m_zRotation = Mathf.SmoothDampAngle(playerCamera.m_zRotation, Mathf.LerpAngle(-30.0f, 30.0f, targetLerp), ref m_tiltVelocity, 0.1f);

        //playerCamera.m_zRotation = Mathf.LerpAngle(-30.0f, 30.0f, targetLerp);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        if (m_isWallRunning)
            Gizmos.DrawLine(transform.position, transform.position + DEBUGDIRECTION * 4.0f);
    }
}
