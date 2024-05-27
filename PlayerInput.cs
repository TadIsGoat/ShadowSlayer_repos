// Tadeáš Vykopal, 3.B, PVA, Shadow Slayer

using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInput : MonoBehaviour
{
    public CharacterController2D characterController;
    PlayerInputActions playerInputActions;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    private void FixedUpdate()
    {
        characterController.Move(playerInputActions.Player.movement.ReadValue<float>());
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            characterController.Jump();
        }
        else if (context.canceled)
        {
            characterController.AdjustGravity("jumpFallIncrease");
        }
    }

    public void GoDown(InputAction.CallbackContext context)
    {
        characterController.GoDownFromPlatform();
    }

    public void SwordAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            StartCoroutine(characterController.SwordAttack());
        }
    }

    public void ChargedAttack(InputAction.CallbackContext context)
    {
        StartCoroutine(characterController.ChargedAttack(context));
    }
}