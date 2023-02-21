using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(InputReader))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float freeLookMovementSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float groundedRadius;
    [SerializeField] private float groundedOffset;
    [SerializeField] private float rotationDamping;

    [Header("Cinemachine")]
    [SerializeField] private GameObject cinemachineCameraTarget;
    [SerializeField] private float topClamp = 70.0f;
    [SerializeField] private float bottomClamp = -30.0f;
    [SerializeField] private float cameraAngleOverride = 0.0f;
    [SerializeField] private float baseSensitivity = .12f;
    [SerializeField] private float lookSensitivity = 1.0f;
    
    [Header("Components")]
    private InputReader inputReader;
    private Rigidbody playerRigidbody;
    
    [HideInInspector] public bool CanLook { get; private set;} = true;
    [HideInInspector] public bool IsInteracting { get; private set;} = false;
    
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;
    private const float threshold = 0.01f;
    private Transform mainCameraTransform;

    private void Awake() 
    {
        // Get components
        inputReader = GetComponent<InputReader>();
        playerRigidbody = GetComponent<Rigidbody>();
    }
    private void OnEnable() 
    {
        // Subscribe to events
        inputReader.JumpEvent += OnJump;
    }

    private void OnDisable() 
    {
        // Unsubscribe to events
        inputReader.JumpEvent -= OnJump;
    }

    private void Start() 
    {
        // Lock the cursor at the start of the game
        Cursor.lockState = CursorLockMode.Locked;

        // Get main camera
        mainCameraTransform = Camera.main.transform;
    }

    private void FixedUpdate() 
    {
        // Calculate movement based on camera 
        Vector3 currentMovement = CalculateMovement();
        currentMovement *= freeLookMovementSpeed;

        // Move
        Move(currentMovement);

        // If not moving return
        if(inputReader.MovementValue == Vector2.zero) return;
        
        // Face movement direction
        FaceMovementDirection(currentMovement);
    }

    private void LateUpdate() 
    {
        CameraRotation();
    }

    private Vector3 CalculateMovement()
    {
        Vector3 forward = mainCameraTransform.forward;
        Vector3 right = mainCameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        return (forward * inputReader.MovementValue.y) + (right * inputReader.MovementValue.x); 
    }

    private void Move(Vector3 motion)
    {
        // Set y velocity to rigidbody y velocity
        motion.y = playerRigidbody.velocity.y;

        // Set velocity to player rigidbody
        playerRigidbody.velocity = motion;
    }

    private void FaceMovementDirection(Vector3 movement)
    {
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.LookRotation(movement),
            rotationDamping
        );
    }

    private void CameraRotation() {
        // If there is an input and camera position is not fixed
        Vector2 lookVector = inputReader.MouseDelta;
        if (lookVector.sqrMagnitude >= threshold && CanLook) {
            cinemachineTargetYaw += lookVector.x * baseSensitivity * lookSensitivity;
            cinemachineTargetPitch += lookVector.y * baseSensitivity * lookSensitivity * -1;
        }

        // Clamp our rotations so our values are limited 360 degrees
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

        // Cinemachine will follow this target
        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + cameraAngleOverride, cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax) {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnJump()
    {
        if(IsGrounded())
        {
            playerRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        return Physics.CheckSphere(spherePosition, groundedRadius, groundLayerMask, QueryTriggerInteraction.Ignore);
    }

    public void ToggleCursor(bool toggle)
    {
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        CanLook = !toggle;
    }

    public void ToggleInteract(bool value = false)
    {
        IsInteracting = value;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z), groundedRadius);
    }
}
