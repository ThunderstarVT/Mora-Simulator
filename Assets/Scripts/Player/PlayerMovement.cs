using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputManager)), RequireComponent(typeof(Ragdoll)), RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerInputManager inputManager;
    [SerializeField] private Ragdoll ragdoll;
    [SerializeField] private Rigidbody rb;
    private Camera mainCamera;

    [Space]
    [SerializeField, Min(0f)] private float walkSpeed = 1.5f;
    [SerializeField, Min(0f)] private float sprintSpeed = 10f;

    [SerializeField, Min(0f)] private float groundedVelocitySmoothing = 10f;
    [SerializeField, Min(0f)] private float airborneVelocitySmoothing = 0.2f;
    
    [SerializeField, Min(0f)] private float turnSpeed = 5.0f;

    [SerializeField, Min(0f)] private float ragdollAcceleration = 1.0f;

    [SerializeField, Min(0f)] private float jumpVelocity = 10f;

    [SerializeField] private LayerMask groundLayer;
    
    [SerializeField, Min(0f)] private float groundCheckRadius = 0.5f;
    [SerializeField] private float groundCheckOffset = 0.0f;
    
    private Vector2 inputDirection;
    
    private bool sprinting;

    public bool IsGrounded => Physics.CheckSphere(
            ragdoll.isRagdolling ? ragdoll.GetCenter() : transform.position + Vector3.up * groundCheckOffset, 
            groundCheckRadius, groundLayer);
    
    public Vector2 AnimVelocity // velocity for use in the animator
    {
        get
        {
            Vector3 vel = transform.InverseTransformDirection(
                Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0)
                * new Vector3(inputDirection.x, 0, inputDirection.y) 
                * (sprinting && IsGrounded && inputDirection.y > Mathf.Abs(inputDirection.x) ? 2 : 1));
            
            return new Vector2(vel.x, vel.z);
        }
    }

    public Vector3 GroundNormal
    {
        get
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, groundLayer))
            {
                return hit.normal;
            }

            return Vector3.up;
        }
    }


    private void Awake()
    {
        mainCamera = Camera.main;
    }

    void Start()
    {
        inputManager.OnMovement += OnMovement;
        inputManager.OnSprint += OnSprint;
        inputManager.OnJump += OnJump;
        inputManager.OnRagdoll += OnRagdoll;
    }

    private void FixedUpdate()
    {
        if (sprinting)
        {
            sprinting = inputDirection.sqrMagnitude > 0.05f;
        }
        
        if (ragdoll.isRagdolling) // ragdoll movement
        {
            ragdoll.AddAcceleration(Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0) 
                                    * new Vector3(inputDirection.x, 0, inputDirection.y) * ragdollAcceleration);
        }
        else
        {
            if (IsGrounded) // ground movement
            {
                Vector3 flatTargetVelocity = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0) 
                                             * new Vector3(inputDirection.x, 0, inputDirection.y) 
                                             * (sprinting && inputDirection.y > Mathf.Abs(inputDirection.x) ? sprintSpeed : walkSpeed);
                
                Vector3 targetVelocity = Vector3.ProjectOnPlane(flatTargetVelocity, GroundNormal);
                
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, groundedVelocitySmoothing * Time.deltaTime);
            }
            else // air movement
            {
                Vector3 targetVelocity = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0) 
                                         * new Vector3(inputDirection.x, 0, inputDirection.y) 
                                         * walkSpeed;
                
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, airborneVelocitySmoothing * Time.deltaTime);
            }
            
            // turn the player
            rb.rotation = Quaternion.Lerp(rb.rotation, Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0), 
                turnSpeed * inputDirection.magnitude * Time.deltaTime);
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        
        if (ragdoll.isRagdolling)
        {
            Gizmos.DrawWireSphere(ragdoll.GetCenter(), groundCheckRadius);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position + Vector3.up * groundCheckOffset, groundCheckRadius);
        }
    }


    private void OnMovement(InputAction.CallbackContext context)
    {
        inputDirection = context.ReadValue<Vector2>();
    }

    private void OnSprint(InputAction.CallbackContext obj)
    {
        sprinting = inputDirection.sqrMagnitude > 0.05f && !sprinting;
    }

    private void OnJump(InputAction.CallbackContext obj)
    {
        if (IsGrounded && !ragdoll.isRagdolling)
        {
            rb.linearVelocity += Vector3.up * jumpVelocity;
        }
    }
    
    private void OnRagdoll(InputAction.CallbackContext context)
    {
        if (ragdoll.isRagdolling)
        {
            if (IsGrounded) ragdoll.SetInactive();
        }
        else ragdoll.SetActive();
    }
}
