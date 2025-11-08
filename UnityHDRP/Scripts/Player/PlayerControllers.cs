using UnityEngine;
using UnityEngine.InputSystem;
using Soulvan.Physics;

namespace Soulvan.Player
{
    /// <summary>
    /// Driver controller for in-car gameplay.
    /// Integrates with VehiclePhysics for realistic handling.
    /// </summary>
    [RequireComponent(typeof(VehiclePhysics))]
    public class DriverController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private float throttleSensitivity = 1f;
        [SerializeField] private float steeringSensitivity = 1f;
        [SerializeField] private bool analogSteering = true;

        [Header("Camera")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 2f, -6f);
        [SerializeField] private float cameraSmoothing = 5f;

        private VehiclePhysics vehiclePhysics;
        private PlayerInput playerInput;
        private InputAction throttleAction;
        private InputAction brakeAction;
        private InputAction steerAction;
        private InputAction nitroAction;

        private void Awake()
        {
            vehiclePhysics = GetComponent<VehiclePhysics>();
            playerInput = GetComponent<PlayerInput>();

            if (playerInput != null)
            {
                throttleAction = playerInput.actions["Throttle"];
                brakeAction = playerInput.actions["Brake"];
                steerAction = playerInput.actions["Steer"];
                nitroAction = playerInput.actions["Nitro"];
            }
        }

        private void Update()
        {
            HandleInput();
            UpdateCamera();
        }

        private void HandleInput()
        {
            if (vehiclePhysics == null) return;

            // Throttle (triggers or W/S keys)
            float throttle = throttleAction?.ReadValue<float>() ?? Input.GetAxis("Vertical");
            throttle = Mathf.Clamp(throttle, 0f, 1f) * throttleSensitivity;
            vehiclePhysics.SetThrottle(throttle);

            // Brake (triggers or space)
            float brake = brakeAction?.ReadValue<float>() ?? (Input.GetKey(KeyCode.Space) ? 1f : 0f);
            vehiclePhysics.SetBrake(brake);

            // Steering (analog stick or A/D keys)
            float steering = steerAction?.ReadValue<float>() ?? Input.GetAxis("Horizontal");
            
            if (!analogSteering)
            {
                // Digital steering (snaps to -1, 0, 1)
                steering = Mathf.Sign(steering) * (Mathf.Abs(steering) > 0.1f ? 1f : 0f);
            }
            
            steering *= steeringSensitivity;
            vehiclePhysics.SetSteering(steering);

            // Nitro (button or shift key)
            bool nitro = nitroAction?.IsPressed() ?? Input.GetKey(KeyCode.LeftShift);
            vehiclePhysics.SetNitro(nitro);
        }

        private void UpdateCamera()
        {
            if (cameraTransform == null) return;

            // Follow vehicle with smooth lag
            Vector3 targetPosition = transform.position + transform.TransformDirection(cameraOffset);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, Time.deltaTime * cameraSmoothing);

            // Look at vehicle
            cameraTransform.LookAt(transform.position + Vector3.up * 1f);
        }

        public void SetTarget(Vector3 targetPosition)
        {
            // AI-controlled driving towards target (for missions)
            Vector3 direction = (targetPosition - transform.position).normalized;
            float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);

            // Auto-steer towards target
            float steering = Mathf.Clamp(angle / 45f, -1f, 1f);
            vehiclePhysics.SetSteering(steering);

            // Auto-throttle
            vehiclePhysics.SetThrottle(1f);
        }
    }

    /// <summary>
    /// On-foot controller for stealth infiltration and boss battles.
    /// Supports movement, crouching, interaction, and combat.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class OnFootController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 6f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float crouchSpeed = 3f;
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float gravity = -20f;

        [Header("Stealth")]
        [SerializeField] private bool isInStealthMode = false;
        [SerializeField] private float stealthSpeedMultiplier = 0.5f;
        [SerializeField] private GameObject stealthVFX;

        [Header("Combat")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackDamage = 50f;
        [SerializeField] private LayerMask enemyLayers;

        [Header("Camera")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 1.5f, -3f);
        [SerializeField] private float cameraSmoothing = 10f;

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private bool isCrouching = false;
        private bool isSprinting = false;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            HandleMovement();
            HandleCombat();
            UpdateCamera();
        }

        private void HandleMovement()
        {
            // Ground check
            isGrounded = controller.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            // Input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 move = new Vector3(horizontal, 0f, vertical);

            // Transform to camera space
            if (cameraTransform != null)
            {
                move = cameraTransform.TransformDirection(move);
                move.y = 0f;
            }

            // Calculate speed
            float speed = walkSpeed;
            
            if (isCrouching)
            {
                speed = crouchSpeed;
            }
            else if (isSprinting && !isInStealthMode)
            {
                speed = sprintSpeed;
            }

            if (isInStealthMode)
            {
                speed *= stealthSpeedMultiplier;
            }

            // Apply movement
            controller.Move(move * speed * Time.deltaTime);

            // Jump
            if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Crouch toggle
            if (Input.GetKeyDown(KeyCode.C))
            {
                isCrouching = !isCrouching;
            }

            // Sprint
            isSprinting = Input.GetKey(KeyCode.LeftShift);

            // Apply gravity
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            // Rotate to face movement direction
            if (move.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }

        private void HandleCombat()
        {
            // Attack (mouse click or button)
            if (Input.GetMouseButtonDown(0))
            {
                PerformAttack();
            }

            // Dodge/Roll (space while moving)
            if (Input.GetKeyDown(KeyCode.Space) && !isGrounded && velocity.magnitude > 0.1f)
            {
                PerformDodge();
            }
        }

        private void PerformAttack()
        {
            Debug.Log("[OnFootController] Attacking");

            // Detect enemies in attack range
            Collider[] hitEnemies = UnityEngine.Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
            
            foreach (Collider enemy in hitEnemies)
            {
                // Apply damage to boss or enemy
                var bossBattle = FindObjectOfType<Missions.BossBattle>();
                if (bossBattle != null)
                {
                    bossBattle.ApplyDamage(attackDamage);
                }

                Debug.Log($"[OnFootController] Hit {enemy.name}");
            }

            // Trigger attack animation/VFX
            EventBus.EmitPlayerAttack();
        }

        private void PerformDodge()
        {
            Debug.Log("[OnFootController] Dodging");
            
            // Apply dodge velocity
            Vector3 dodgeDirection = transform.forward;
            velocity = dodgeDirection * 10f;

            // Trigger dodge animation/VFX
            EventBus.EmitPlayerDodge();
        }

        public void EnterStealthMode()
        {
            isInStealthMode = true;
            
            if (stealthVFX != null)
            {
                stealthVFX.SetActive(true);
            }

            // Apply calm motif overlay
            FindObjectOfType<MotifAPI>()?.SetMotif(Motif.Calm, 0.7f);

            Debug.Log("[OnFootController] Entered stealth mode");
        }

        public void ExitStealthMode()
        {
            isInStealthMode = false;
            
            if (stealthVFX != null)
            {
                stealthVFX.SetActive(false);
            }

            Debug.Log("[OnFootController] Exited stealth mode");
        }

        private void UpdateCamera()
        {
            if (cameraTransform == null) return;

            // Third-person camera follow
            Vector3 targetPosition = transform.position + transform.TransformDirection(cameraOffset);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, Time.deltaTime * cameraSmoothing);

            // Look at player
            cameraTransform.LookAt(transform.position + Vector3.up * 1.5f);
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
