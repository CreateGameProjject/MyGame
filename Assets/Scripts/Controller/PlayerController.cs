using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Parameters
    [Header("Player")]
    [Tooltip("キャラクターの移動速度設定[m/s]")]
    public float MoveSpeed = 2.0f;

    [Tooltip("キャラクタースプリント時移動速度[m/s]")]
    public float SprintSpeed = 5.335f;

    [Tooltip("加速と減速")]
    public float SpeedChangeRate = 10.0f;

    [Tooltip("キャラクターが移動方向に向きを帰る速さ")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("落下状態に遷移するまでの経過時間")]
    public float FallTimeout = 0.15f;

    [Space(10)]
    [Tooltip("キャラクターがジャンプ可能な高さ")]
    public float JumpHeight = 1.2f;

    [Tooltip("重力。Unityエンジンのデォルトは-9.81")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("再度ジャンプ可能になるまでの経過時間")]
    public float JumpTimeout = 0.50f;

    [Header("Player Grounded")]
    [Tooltip("キャラクター接地判定")]
    public bool Grounded = true;

    public float GroundedOffset = -0.14f;

    [Tooltip("接地判定の半径。キャラクターコントローラの半径と一致する必要がある")]
    public float GroundedRadius = 0.28f;

    [Tooltip("キャラクターが地面として使用するレイヤー")]
    public LayerMask GroundLayers;

    [Tooltip("")]
    public bool hasWeapon = false;
    #endregion

    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _verticalVelocity;
    private float _rotationVelocity;
    private float _terminalVelocity = 53.0f;

    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private Animator _animator;

    private GameObject _mainCamera;

    private CharacterController _controller;

    private InputManage _input;

    /// <summary>
    /// 起動時設定
    /// </summary>
    void Awake()
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        TryGetComponent(out _animator);
    }

    /// <summary>
    /// 毎フレーム更新
    /// </summary>
    void Update()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<InputManage>();

        JumpAndGravity();
        GroundedCheck();
        Move();

    }

    /// <summary>
    /// 着地判定
    /// </summary>
    private void GroundedCheck()
    {
        //キャラクターの位置を取得する
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);

        //Colliderにオブジェクトがヒットするか
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        _animator.SetBool("IsGrounded", Grounded);
    }

    /// <summary>
    /// キャラクターの移動
    /// </summary>
    private void Move()
    {

        //TODO：スプリントの実装
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

        //入力がない場合は速度を0に設定
        if(_input.move == Vector2.zero)
        {
            targetSpeed = 0.0f;
        }

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        float speedOffset = 0.1f;

        //移動設定（使ってない）
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

            _speed = Mathf.Round(_speed * 1000f) / 1000f;

            _speed = targetSpeed;
        }
        else
        {
            _speed = targetSpeed;
        }

        //アニメーション用（入力に対する反応よすぎて改善必要）
        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f)
        {
            _animationBlend = 0f;
        }

        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        if (_input.move != Vector2.zero)
        {
             _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;

            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        //アニメーションに値を設定
        _animator.SetFloat("Speed", _animationBlend);

    }

    /// <summary>
    /// ジャンプ
    /// 要修正
    /// </summary>
    private void JumpAndGravity()
    {
        if(Grounded)
        {
            _fallTimeoutDelta = FallTimeout;

            //アニメーション解除
            _animator.SetBool("IsJumpStart", false);
            _animator.SetBool("FreeFall", false);

            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            //ジャンプボタンが押された
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                //ジャンプアニメーション設定
                _animator.SetBool("IsJumpStart", true);
            }

            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        } else
        {
            //条件がいけてない気がする
            //いったんうごくからこのまま

            _jumpTimeoutDelta = JumpTimeout;

            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _animator.SetBool("FreeFall", true);
            }

            _input.jump = false;

        }

        //重力を加味する
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    public void Attack()
    {
        _animator.SetTrigger("Attack");
    }
}
