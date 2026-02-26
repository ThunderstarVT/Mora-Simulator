using System;
using UnityEngine;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(PlayerMovement)), RequireComponent(typeof(PlayerActions))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int OnJump = Animator.StringToHash("OnJump");
    private static readonly int OnKick = Animator.StringToHash("OnKick");
    private static readonly int OnEat = Animator.StringToHash("OnEat");
    private static readonly int OnScream = Animator.StringToHash("OnScream");
    private static readonly int CamX = Animator.StringToHash("CamX");
    private static readonly int CamY = Animator.StringToHash("CamY");
    private static readonly int VelocityX = Animator.StringToHash("VelocityX");
    private static readonly int VelocityY = Animator.StringToHash("VelocityY");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int BreathingFire = Animator.StringToHash("BreathingFire");

    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerActions playerActions;
    [SerializeField] private CameraController cameraController;

    private void Start()
    {
        playerMovement.OnJumpEvent += () => animator.SetTrigger(OnJump);
        
        playerActions.OnKickEvent += () => animator.SetTrigger(OnKick);
        playerActions.OnEatEvent += () => animator.SetTrigger(OnEat);
        playerActions.OnScreamEvent += () => animator.SetTrigger(OnScream);
    }

    private void FixedUpdate()
    {
        Vector2 cameraForward = playerMovement.transform.InverseTransformDirection(cameraController.transform.forward);
        
        animator.SetFloat(CamX, cameraForward.x);
        animator.SetFloat(CamY, cameraForward.y);

        Vector2 playerVelocity = playerMovement.AnimVelocity;
        
        animator.SetFloat(VelocityX, playerVelocity.x);
        animator.SetFloat(VelocityY, playerVelocity.y);
        
        animator.SetBool(Grounded, playerMovement.IsGrounded);
        
        animator.SetBool(BreathingFire, playerActions.BreathingFire);
    }
}
