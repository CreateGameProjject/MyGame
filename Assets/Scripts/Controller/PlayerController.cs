using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Parameters
    [Header("Player")]
    [Tooltip("�L�����N�^�[�̈ړ����x�ݒ�[m/s]")]
    public float MoveSpeed = 2.0f;

    [Tooltip("�L�����N�^�[�X�v�����g���ړ����x[m/s]")]
    public float SprintSpeed = 5.335f;

    [Tooltip("�����ƌ���")]
    public float SpeedChangeRate = 10.0f;

    [Tooltip("�L�����N�^�[���ړ������Ɍ������A�鑬��")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("������ԂɑJ�ڂ���܂ł̌o�ߎ���")]
    public float FallTimeout = 0.15f;

    [Space(10)]
    [Tooltip("�L�����N�^�[���W�����v�\�ȍ���")]
    public float JumpHeight = 1.2f;

    [Tooltip("�d�́BUnity�G���W���̃f�H���g��-9.81")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("�ēx�W�����v�\�ɂȂ�܂ł̌o�ߎ���")]
    public float JumpTimeout = 0.50f;

    [Header("Player Grounded")]
    [Tooltip("�L�����N�^�[�ڒn����")]
    public bool Grounded = true;

    public float GroundedOffset = -0.14f;

    [Tooltip("�ڒn����̔��a�B�L�����N�^�[�R���g���[���̔��a�ƈ�v����K�v������")]
    public float GroundedRadius = 0.28f;

    [Tooltip("�L�����N�^�[���n�ʂƂ��Ďg�p���郌�C���[")]
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
    /// �N�����ݒ�
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
    /// ���t���[���X�V
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
    /// ���n����
    /// </summary>
    private void GroundedCheck()
    {
        //�L�����N�^�[�̈ʒu���擾����
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);

        //Collider�ɃI�u�W�F�N�g���q�b�g���邩
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        _animator.SetBool("IsGrounded", Grounded);
    }

    /// <summary>
    /// �L�����N�^�[�̈ړ�
    /// </summary>
    private void Move()
    {

        //TODO�F�X�v�����g�̎���
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

        //���͂��Ȃ��ꍇ�͑��x��0�ɐݒ�
        if(_input.move == Vector2.zero)
        {
            targetSpeed = 0.0f;
        }

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        float speedOffset = 0.1f;

        //�ړ��ݒ�i�g���ĂȂ��j
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

        //�A�j���[�V�����p�i���͂ɑ΂��锽���悷���ĉ��P�K�v�j
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

        //�A�j���[�V�����ɒl��ݒ�
        _animator.SetFloat("Speed", _animationBlend);

    }

    /// <summary>
    /// �W�����v
    /// �v�C��
    /// </summary>
    private void JumpAndGravity()
    {
        if(Grounded)
        {
            _fallTimeoutDelta = FallTimeout;

            //�A�j���[�V��������
            _animator.SetBool("IsJumpStart", false);
            _animator.SetBool("FreeFall", false);

            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            //�W�����v�{�^���������ꂽ
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                //�W�����v�A�j���[�V�����ݒ�
                _animator.SetBool("IsJumpStart", true);
            }

            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        } else
        {
            //�����������ĂȂ��C������
            //�������񂤂������炱�̂܂�

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

        //�d�͂���������
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
