// Tade� Vykopal, 3.B, PVA, Shadow Slayer

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Timeline.TimelinePlaybackControls;


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
        if(context.started)
        {
            characterController.SwordAttack();
        }
    }
}