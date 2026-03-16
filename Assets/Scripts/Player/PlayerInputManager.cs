using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    public bool SprintKeyHeld => inputActions.Player.sprint.IsPressed();

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        
        inputActions.Player.Enable();
    }

    private void Start()
    {
        inputActions.Player.movement.started += e => OnMovement?.Invoke(e);
        inputActions.Player.movement.performed += e => OnMovement?.Invoke(e);
        inputActions.Player.movement.canceled += e => OnMovement?.Invoke(e);
        
        inputActions.Player.sprint.started += e => OnSprint?.Invoke(e);
        
        inputActions.Player.jump.started += e => OnJump?.Invoke(e);
        
        inputActions.Player.ragdoll.started += e => OnRagdoll?.Invoke(e);
        
        inputActions.Player.kick.started += e => OnKick?.Invoke(e);
        
        inputActions.Player.eat.started += e => OnEat?.Invoke(e);
        
        inputActions.Player.breatheFire.started += e => OnBreatheFire?.Invoke(e);
        inputActions.Player.breatheFire.performed += e => OnBreatheFire?.Invoke(e);
        inputActions.Player.breatheFire.canceled += e => OnBreatheFire?.Invoke(e);
        
        inputActions.Player.makeSound.started += e => OnMakeSound?.Invoke(e);
    }

    private void OnDestroy()
    {
        inputActions.Player.Disable();
    }
    
    
    public void setInputActive(bool active)
    {
        if (active) inputActions.Player.Enable();
        else inputActions.Player.Disable();
    }


    public event Action<InputAction.CallbackContext> OnMovement;
    public event Action<InputAction.CallbackContext> OnSprint;
    public event Action<InputAction.CallbackContext> OnJump;
    public event Action<InputAction.CallbackContext> OnRagdoll;
    
    public event Action<InputAction.CallbackContext> OnKick;
    public event Action<InputAction.CallbackContext> OnEat;
    public event Action<InputAction.CallbackContext> OnBreatheFire;
    public event Action<InputAction.CallbackContext> OnMakeSound;
}
