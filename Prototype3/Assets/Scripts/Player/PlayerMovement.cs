using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum HeadAbility
{
    none,
    invisibility,
}
public enum ArmAbility
{
    none,
    grapplingHook,
}
public enum LegsAbility
{
    none,
    wallrun,
}

public class PlayerMovement : MonoBehaviour
{
    public PlayerCamera playerCamera { get; private set; }
    public MultiAudioAgent audioAgent { get; private set; }
    public Animator m_animator { get; private set; }
    public LayerMask m_noiseMask;

    [Header("Player Death")]
    public string m_nextScreen = "MainMenu";
    public bool m_dead { get; private set; } = false;

    [Header("Movement Attributes")]
    public float m_speed = 8.0f;
    public float m_crouchSpeed = 4.0f;
    public float m_jumpSpeed = 5.0f;
    public float m_playerGravity = 9.81f;
    public float m_damping = 0.5f;
    public float m_airAcceleration = 1.0f;

    public float m_crouchVisibility = 0.5f;
    public float m_visibility { get; private set; } // Much requested visibility variable 

    [Header("Crouching")]
    public LayerMask m_headCollisionMask;

    private bool m_isCrouching = false;
    private float m_cameraOffset = 0.0f;
    private float m_crouchLerp = 1.0f;

    private Vector3 m_velocity = Vector3.zero;
    private CharacterController charController;
    private bool m_grounded = false;

    public Volume crouchVolume;

    [Header("Abilities")]
    public HeadAbility m_headAbility;
    public ArmAbility m_armAbility;
    public LegsAbility m_legsAbility;

    [Header("Grappling Hook")]
    public LineRenderer m_grappleSource;
    public float m_grappleRange = 15.0f;
    public float m_grappleAcceleration = 10.0f;
    public float m_maxGrappleSpeed = 20.0f;
    public float m_grappleProjectileSpeed = 5.0f;
    public Transform m_grappleEnd;

    public float m_grappleForgiveDistance = 0.5f;

    private Vector3 m_grappleHitPos;
    private HookMode m_hookMode = HookMode.idle;
    private float m_grappleShotLerp = 0.0f;

    public float m_grappleCD = 45.0f;
    public float m_grappleCDTimer = 0.0f;

    [Header("Wall Running")]
    public LayerMask m_wallCollisionMask;
    public Transform m_wallColliderL;
    public Transform m_wallColliderR;
    public float m_wallRunGravity = 3.0f;
    private bool m_isWallRunning = false;
    public float m_cameraTiltSpeed = 1.0f;

    private WallDir m_currentWall = WallDir.none;

    private float m_tiltVelocity = 0.0f;
    private float m_xTiltVelocity = 0.0f;
    private bool m_wallRunRefreshed = true;

    [Header("Invisibility")]
    public GameObject m_invisEffect;
    public float m_invisibilityDuration = 5.0f;
    public float m_invisibilityTimer = 0.0f;
    public float m_invisibilityCD = 45.0f;
    public float m_invisibilityCDTimer = 0.0f;

    private bool m_isInvisible = false;

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
        m_animator = GetComponentInChildren<Animator>();
        charController = GetComponent<CharacterController>();
        playerCamera = GetComponent<PlayerCamera>();
        audioAgent = GetComponent<MultiAudioAgent>();
        m_cameraOffset = playerCamera.m_camera.transform.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.instance.IsKeyDown(KeyType.K))
            KillPlayer();
        if (m_dead)
        {
            playerCamera.m_xRotation = Mathf.SmoothDampAngle(playerCamera.m_xRotation, -85.0f, ref m_xTiltVelocity, 0.2f);
            playerCamera.m_zRotation = Mathf.SmoothDampAngle(playerCamera.m_zRotation, 85.0f, ref m_tiltVelocity, 0.3f);
        }

        bool leftGround = false;
        if (!charController.isGrounded && m_grounded)
        {
            leftGround = true;
        }
        m_grounded = charController.isGrounded;

        // Player movement
        Vector2 movementInput = Vector2.zero;
        if (!m_isWallRunning && !m_dead)
            movementInput = GetMovementInput();
        Vector3 moveDirection = transform.right * movementInput.x + transform.forward * movementInput.y;

        float currentSpeed = (m_crouchLerp < 0.5f) ? m_crouchSpeed : m_speed;

        m_animator.SetBool("IsRunning", movementInput.y > 0.0f && m_grounded);
        m_animator.SetBool("IsZip", m_hookMode != HookMode.idle);

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
            //if (Vector3.Distance(playerCamera.m_camera.transform.position, m_grappleHitPos) > 5.0f || m_hookMode != HookMode.pulling)
            if (m_hookMode != HookMode.pulling)
                m_velocity.y -= ((!m_isWallRunning) ? m_playerGravity : m_wallRunGravity) * Time.deltaTime;
            else

            // Velocity damping
            m_velocity.x -= m_velocity.x * m_damping * Time.deltaTime;
            m_velocity.z -= m_velocity.z * m_damping * Time.deltaTime;
        }

        if ((charController.collisionFlags & CollisionFlags.CollidedAbove) != 0 && m_velocity.y > 0 && m_hookMode != HookMode.pulling)
            m_velocity.y = 0;

        // Jumping
        if (!m_dead && charController.isGrounded && InputManager.instance.IsKeyDown(KeyType.SPACE))
        {
            m_velocity.y = m_jumpSpeed;
        }

        // Crouching
        if (InputManager.instance.IsKeyDown(KeyType.L_CTRL) && !m_dead)
        {
            m_isCrouching = !m_isCrouching;
        }

        if (m_isCrouching)
            m_crouchLerp -= Time.deltaTime * 10.0f;
        else if (!Physics.CheckSphere(playerCamera.m_camera.transform.position, charController.radius * 1.1f, m_headCollisionMask))
            m_crouchLerp += Time.deltaTime * 10.0f;

        m_animator.SetBool("IsSneaking", m_isCrouching);

        m_crouchLerp = Mathf.Clamp(m_crouchLerp, 0.0f, 1.0f);

        if (crouchVolume != null)
            crouchVolume.weight = 1.0f - m_crouchLerp;

        playerCamera.m_camera.transform.localPosition = new Vector3(0, Mathf.Lerp(0.0f, m_cameraOffset, m_crouchLerp), 0);

        float newHeight = Mathf.Lerp(0.5f, 2.0f, m_crouchLerp);
        float deltaHeight = newHeight - charController.height;
        charController.height = newHeight;

        Abilities();
        StealthDetection();

        charController.Move(moveDirection * currentSpeed * Time.deltaTime + m_velocity * Time.deltaTime + 0.5f * deltaHeight * Vector3.up);
    }
    public void KillPlayer()
    {
        if (m_dead)
            return;

        LevelLoader.instance.LoadNewLevel(m_nextScreen, LevelLoader.Transition.YOUDIED);
        gameObject.layer = 2;
        m_dead = true;
        m_isCrouching = false;
        m_hookMode = HookMode.retracting;
        m_isInvisible = false;
        m_isWallRunning = false;
        m_currentWall = WallDir.none;
    }
    private void StealthDetection()
    {
        m_visibility = (m_crouchVisibility + m_crouchLerp * (1.0f - m_crouchVisibility)) * (m_isInvisible ? 0.0f : 1.0f);
    }
    private void Abilities()
    {
        switch (m_headAbility)
        {
            case HeadAbility.invisibility:
                Invisibility();
                break;
            default:
                m_isInvisible = false;
                m_invisibilityTimer = 0.0f;
                break;
        }
        m_invisEffect.SetActive(m_isInvisible);

        switch (m_armAbility)
        {
            case ArmAbility.grapplingHook:
                GrapplingHook();
                break;
            default:
                break;
        }
        switch (m_legsAbility)
        {
            case LegsAbility.wallrun:
                WallRunning();
                break;
            default:
                break;
        }
    }
    public void Footstep()
    {
        if (!m_grounded)
            return;

        NoiseManager.instance.CreateNoise(transform.position, 8.0f, m_noiseMask, Time.deltaTime);
        audioAgent.Play("Footstep");
    }

    private void GrapplingHook()
    {
        m_grappleSource.SetPosition(0, m_grappleSource.transform.position);
        m_grappleSource.SetPosition(1, m_grappleEnd.position);

        if (m_grappleCDTimer > 0.0f)
            m_grappleCDTimer -= Time.deltaTime;

        if (InputManager.instance.GetMouseButtonDown(MouseButton.RIGHT) && !m_dead)
        {
            RaycastHit rayHit;

            if (m_hookMode != HookMode.idle && m_hookMode != HookMode.retracting)
            {
                m_hookMode = HookMode.retracting;
                m_animator.SetTrigger("ZipPullStart");
            }

            if (m_grappleCDTimer <= 0.0f && m_hookMode == HookMode.idle)
            {
                bool hit = false;
                if (Physics.Raycast(playerCamera.m_camera.transform.position, playerCamera.m_camera.transform.forward, out rayHit, m_grappleRange, m_headCollisionMask))
                {
                    hit = true;
                }
                else if (Physics.SphereCast(playerCamera.m_camera.transform.position + playerCamera.m_camera.transform.forward * m_grappleRange, m_grappleForgiveDistance, -playerCamera.m_camera.transform.forward, out rayHit, m_grappleRange, m_headCollisionMask))
                { // Now casts from target to player
                    hit = true;
                }

                if (hit)
                {
                    m_grappleHitPos = rayHit.point;
                    m_hookMode = HookMode.firing;
                    m_grappleSource.enabled = true;

                    m_grappleCDTimer = m_grappleCD;
                }
                else
                {
                    m_hookMode = HookMode.firing_missed;
                    m_grappleHitPos = playerCamera.m_camera.transform.position + playerCamera.m_camera.transform.forward * m_grappleRange;
                    m_grappleSource.enabled = true;
                }
                m_animator.SetTrigger("ZipFire");
                audioAgent.Play("HookLaunch");
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
                    m_animator.SetTrigger("ZipPullStart");
                }
                break;
            case HookMode.firing_missed:
                m_grappleShotLerp += Time.deltaTime * m_grappleProjectileSpeed;
                if (m_grappleShotLerp >= 1.0f)
                {
                    m_hookMode = HookMode.retracting;
                    m_animator.SetTrigger("ZipPullStart");
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
                    m_velocity /= 5.0f;
                    m_velocity.y += 8.0f;
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
    private void WallRunning()
    {
        Vector3 direction = transform.forward;

        if (m_grounded)
            m_wallRunRefreshed = true;

        if (((InputManager.instance.IsKeyDown(KeyType.SPACE) && m_wallRunRefreshed && !m_dead) || m_isWallRunning) && !m_grounded)
        {
            Collider closestCollider = null;
            Collider[] colliders = Physics.OverlapSphere(transform.position, 0.75f, m_wallCollisionMask);

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
                m_velocity.y = -m_wallRunGravity;

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

        float targetLerp = (Mathf.Sin(Vector3.SignedAngle(transform.forward, direction, Vector3.up) * Mathf.Deg2Rad) * 0.5f) + 0.5f;
        playerCamera.m_zRotation = Mathf.SmoothDampAngle(playerCamera.m_zRotation, Mathf.LerpAngle(-30.0f, 30.0f, targetLerp), ref m_tiltVelocity, 0.1f);
    }
    private void Invisibility()
    {
        if (m_isInvisible)
        {
            if (m_invisibilityTimer > 0.0f)
            {
                m_invisibilityTimer -= Time.deltaTime;
            }
            else
            {
                m_isInvisible = false;
            }
        }
        else
        {
            if (m_invisibilityCDTimer > 0.0f)
            {
                m_invisibilityCDTimer -= Time.deltaTime;
            }
            else
            {
                if (InputManager.instance.IsKeyDown(KeyType.Q) && !m_dead)
                { 
                    m_animator.SetTrigger("Snap");
                    m_invisibilityTimer = m_invisibilityDuration;
                }
            }
        }
    }

    public void StartInvis()
    {
        audioAgent.Play("InvisSnap");
        m_invisibilityCDTimer = m_invisibilityCD;
        m_isInvisible = true;
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
    public void SetHeadAbility(HeadAbility _ability)
    {
        m_headAbility = _ability;
    }
    public void SetArmAbility(ArmAbility _ability)
    {
        m_armAbility = _ability;
    }
    public void SetLegsAbility(LegsAbility _ability)
    {
        m_legsAbility = _ability;
    }
}
