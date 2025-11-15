using Framework.Toolkit.Movement;
using UnityEngine;

namespace Prpr.Player
{
    public class PlayerMovement
    {
        private MovementProfileSO _profile;
        private Rigidbody2D _rb;
        private Transform _groundCheckTransform;

        private Vector2 _inputs;
        private float coyoteTimer;
        private bool wantJumpBuffered;
        private float jumpBufferTimer;
        private bool _enabled;
        private bool _isJumpPressed;

        public void Initialization(MovementProfileSO profile, Transform transform, Rigidbody2D rb)
        {
            _profile = profile;
            _groundCheckTransform = transform;
            _rb = rb;
            _enabled = true;
        }

        public void SetInputs(Vector2 inputs, bool jumpPressed)
        {
            _inputs = inputs;
            _isJumpPressed = jumpPressed;
            if (_isJumpPressed == true)
                Debug.Log("trying jump");
        }

        /// <summary>
        /// 在 Update 中调用, 处理土狼跳逻辑
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateDrive(float deltaTime)
        {
            CoyoteJump(deltaTime);
            Debug.Log(CheckGrounded());
        }

        public void EnableMovement(bool enabled) => _enabled = enabled;

        /// <summary>
        /// 进行物理计算
        /// </summary>
        public void FixedUpdateDrive()
        {
            if (!_enabled)
                return;

            // 处理移动
            float targetSpeed = _inputs.x * _profile.maxSpeed;
            float deltaSpeed = targetSpeed - _rb.linearVelocityX;
            float accelerationRate = Mathf.Abs(targetSpeed) > 0.01f ? _profile.acceleration : _profile.deceleration;
            float movemntSpeed = deltaSpeed * accelerationRate * Time.fixedDeltaTime;
            _rb.linearVelocityX += movemntSpeed;
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

        public bool IsOnGround => CheckGrounded();
    }
}

