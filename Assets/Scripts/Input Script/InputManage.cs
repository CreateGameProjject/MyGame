using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;


public class InputManage : MonoBehaviour
{
    [Header("Character Input Values")]
    public Vector2 move;

    [Header("Movement Settings")]
    public bool analogMovement;

    public bool sprint;

    public bool jump;

    PlayerInput _input;

    void Awake()
    {
        TryGetComponent(out _input);
    }

    void OnEnable()
    {
        if (_input != null)
        {
            _input.actions["Move"].performed += OnMove;
            _input.actions["Move"].canceled += OnMoveStop;
            _input.actions["Jump"].started += OnJump;
        }
    }

    void OnDisable()
    {
        if (_input != null)
        {
            _input.actions["Move"].performed -= OnMove;
            _input.actions["Move"].canceled -= OnMoveStop;
            _input.actions["Jump"].started -= OnJump;
        }
    }

    void OnMove(InputAction.CallbackContext obj)
    {
        var value = obj.ReadValue<Vector2>();

        MoveInput(value);
    }

    void OnJump(InputAction.CallbackContext obj)
    {
        JumpInput(obj.started);
    }

    void OnMoveStop(InputAction.CallbackContext obj)
    {
        move = Vector2.zero;
    }

    void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    }

    void JumpInput(bool newJumpState)
    {
        jump = newJumpState;
    }

}
