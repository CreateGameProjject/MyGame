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

    PlayerController _controller;

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
            _input.actions["Attack"].started += OnAttack;
        }
    }

    void OnDisable()
    {
        if (_input != null)
        {
            _input.actions["Move"].performed -= OnMove;
            _input.actions["Move"].canceled -= OnMoveStop;
            _input.actions["Jump"].started -= OnJump;
            _input.actions["Attack"].started -= OnAttack;
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

        Debug.Log("aaaaaaaaaaaa");
    }

    void OnMoveStop(InputAction.CallbackContext obj)
    {
        move = Vector2.zero;
    }

    void OnAttack(InputAction.CallbackContext obj)
    {
        Debug.Log("çUåÇÇ∑ÇÈÇ®ÅI");
        //_controller.Attack();
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
