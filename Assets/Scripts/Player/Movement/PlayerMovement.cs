using System;
using Framework.Toolkit.Movement;
using UnityEngine;

namespace Prpr.Player
{
    public class PlayerMovement
    {
        [Header("Base Components")]
        private MovementProfileSO _profile;
        private Rigidbody2D _rb;
        private Transform _groundCheckTransform;
        private Transform _playerTransform;

        private Vector2 _inputs;
        private bool _enabled;

        [Header("Jump State")]
        private float coyoteTimer;
        private bool wantJumpBuffered;
        private float jumpBufferTimer;
        private bool _isJumpPressed;

        [Header("Dash State")]
        private bool _isDashPressed;
        private float _dashCooldownTimer;
        private float _dashTimer;
        private bool _isDashing;                // 在 Dash 的时候屏蔽其他物理
        private bool _canDash;
        private Vector2 _dashDirection = Vector2.zero;
        private Vector2 _dashSpeed;

        public void Initialization(MovementProfileSO profile, Transform transform, Rigidbody2D rb, Transform playerTransform)
        {
            _profile = profile;
            _groundCheckTransform = transform;
            _rb = rb;
            _playerTransform = playerTransform;
            _enabled = true;
        }

        public void SetInputs(Vector2 inputs, bool jumpPressed, bool dashPressed)
        {
            _inputs = inputs;
            _isJumpPressed = jumpPressed;
            _isDashPressed = dashPressed;
        }

        /// <summary>
        /// 在 Update 中调用, 处理土狼跳逻辑
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateDrive(float deltaTime)
        {
            CoyoteJump(deltaTime);
            Dash(deltaTime);
        }

        public void Dash(float deltaTime)
        {
            Debug.Log($"Dashing ... {_dashTimer:F2}; Speed {_rb.linearVelocity}");
            if (_isDashing)
            {
                _dashTimer -= deltaTime;
                return;
            }

            // 无法 Dash 的时候 更新冷却时间
            if (!_canDash)
            {
                _dashCooldownTimer -= deltaTime;
                // 冷却时间到了且在地面上的时候重置 Dash
                if (_dashCooldownTimer <= 0f && CheckGrounded())
                {
                    _canDash = true;
                }
            }

            // 在地面上且不在Dash的时候 允许 Dash
            if (!_isDashing && CheckGrounded())
            {
                _canDash = true;
            }

            if (_isDashPressed && !_isDashing && _canDash)
            {
                ExecuteDash();
            }
        }

        public void EnableMovement(bool enabled) => _enabled = enabled;

        /// <summary>
        /// 进行物理计算
        /// </summary>
        public void FixedUpdateDrive()
        {
            if (!_enabled)
                return;

            if (_isDashing)
            {
                _rb.linearVelocity = _dashSpeed;
                if (_dashTimer <= 0f)
                {
                    // dash 结束后恢复状态以及做一点小回调
                    EndDash();
                }

                // 屏蔽其他物理计算
                return;
            }

            // 处理移动
            float targetSpeed = _inputs.x * _profile.maxSpeed;
            float deltaSpeed = targetSpeed - _rb.linearVelocityX;
            float accelerationRate = Mathf.Abs(targetSpeed) > 0.01f ? _profile.acceleration : _profile.deceleration;
            float movemntSpeed = deltaSpeed * accelerationRate * Time.fixedDeltaTime;
            _rb.linearVelocityX += movemntSpeed;

            // 跳跃下落额外加速
            if (_rb.linearVelocityY < 0) // 正在下落
            {
                _rb.linearVelocityY += _profile.gravityScale * _profile.fallMultiplier * Time.fixedDeltaTime;
            }
            else
            {
                _rb.linearVelocityY += _profile.gravityScale * Time.fixedDeltaTime;
            }

            _rb.linearVelocityY = Mathf.Max(_rb.linearVelocityY, _profile.terminalVelocityY);
        }

        private void EndDash()
        {
            _isDashing = false;
            _dashTimer = 0f;

            // NOTE: 是否需要来个落地小速度？
            _rb.linearVelocity = _profile.dashRetainSpeed * _profile.dashVelocity * _dashDirection;
            // _rb.linearVelocityX = _dashDirection.x * _profile.dashVelocity * _profile.dashRetainSpeed;
            // _rb.linearVelocityY = Mathf.Min(_rb.linearVelocityY, 0f);
        }

        private void CoyoteJump(float deltaTime)
        {
            if (CheckGrounded())
                coyoteTimer = _profile.coyoteTime;
            else
                coyoteTimer -= deltaTime;

            if (_isJumpPressed)
            {
                if (CanPerformJump())
                {
                    ExecuteJump();
                    return;
                }
                else
                {
                    wantJumpBuffered = true;        // 入队 延迟执行
                    jumpBufferTimer = _profile.jumpBufferTime;
                }
            }

            // 队中有待消费的跳跃按键, 所以一直在 Timer <= 0f 前一直检查是否可以进行跳跃。
            // 若可, 则跳
            // 否则超过时间, 就清空状态 出队
            if (wantJumpBuffered)
            {
                jumpBufferTimer -= deltaTime;
                if (jumpBufferTimer <= 0f)
                {
                    ClearJumpBuffer();
                }
                else if (CanPerformJump())
                {
                    ExecuteJump();
                }
            }
        }

        private void ExecuteJump()
        {
            _rb.linearVelocityY = _profile.jumpVelocity;
            coyoteTimer = 0f;
        }

        void ClearJumpBuffer()
        {
            wantJumpBuffered = false;
            jumpBufferTimer = 0f;
        }

        private bool CheckGrounded()
        {
            // var origin = (Vector2)_transform.position + _profile.groundCheckOffset;
            // var hit = Physics2D.Raycast(origin, Vector2.down, _profile.groundCheckDistance, _profile.groundMask);
            // return hit.collider != null;
            return Physics2D.OverlapCircle((Vector2)_groundCheckTransform.position + _profile.groundCheckOffset, _profile.groundCheckDistance, _profile.groundMask);
        }

        private bool CanPerformJump()
        {
            return coyoteTimer > 0f || CheckGrounded();
        }

        private void ExecuteDash()
        {
            // 没有输入时
            if (_inputs.sqrMagnitude < 0.01f)
            {
                _dashDirection = new Vector2(Mathf.Sign(_playerTransform.localScale.x), 0f);
            }
            else
            {
                _dashDirection = _inputs;
            }
            _dashDirection = _dashDirection.normalized;
            _isDashing = true;
            _dashCooldownTimer = _profile.dashCooldownTime;
            _canDash = false;
            _dashTimer = _profile.dashDuration;
            _dashSpeed = new Vector2(_dashDirection.x * _profile.dashVelocity, _dashDirection.y * _profile.dashVelocity);
            _rb.linearVelocity = _dashSpeed;

        }

        public bool IsOnGround => CheckGrounded();
    }
}

