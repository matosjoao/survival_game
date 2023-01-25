using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, PlayerControls.IMainActions
{
    public Vector2 MovementValue {get; private set;}
    public bool IsAttaking { get; private set;}
    public Vector2 MouseDelta { get; private set;}

    public event Action JumpEvent;
    public event Action InteractEvent;
    public event Action InventoryEvent;

    private PlayerControls controls;

    private void Start()
    {
        // Instantiate PlayerControls
        controls = new PlayerControls();
        controls.Main.SetCallbacks(this);

        // Enable PlayerControls
        controls.Main.Enable();
    }

    private void OnDestroy() {
        // Disable PlayerControls
        controls.Main.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // Read value from move keys
        MovementValue = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context) 
    {
        MouseDelta = context.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            IsAttaking = true;
        }
        else if(context.canceled)
        {
            IsAttaking = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        // If not press return
        if(!context.performed){return;}
        // Invoke Jump event
        JumpEvent?.Invoke();
    }

    public void OnInventory(InputAction.CallbackContext context)
    { 
        // If not press return
        if(!context.performed){return;}
        // Invoke Jump event
        InventoryEvent?.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    { 
        // If not press return
        if(!context.performed){return;}
        // Invoke Interact event
        InteractEvent?.Invoke();
    }
}
