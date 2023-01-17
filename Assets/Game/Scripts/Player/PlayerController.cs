using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Rigidbody playerRigidbody;


    [Header("Movement")]
    [SerializeField] private float freeLookMovementSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float groundedRadius;
    [SerializeField] private float groundedOffset;
    [SerializeField] private float rotationDamping;

    [HideInInspector]
    public bool canLook = true;

    private Transform mainCameraTransform;

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

    private void OnDrawGizmos() 
    {
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z), groundedRadius);
    }
}
