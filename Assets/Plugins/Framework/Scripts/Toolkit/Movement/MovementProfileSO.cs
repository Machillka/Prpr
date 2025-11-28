using Framework.Profile.Scriptable;
using UnityEngine;

namespace Framework.Toolkit.Movement
{
    /// <summary>
    /// Movement-related parameters for a player or entity.
    /// Designed to be simple and designer-friendly.
    /// </summary>
    [CreateAssetMenu(fileName = "MovementProfile", menuName = "Framework/Profiles/MovementProfile")]
    public class MovementProfileSO : BaseProfileSO
    {
        [Header("Movement Parameters")]
        public float maxSpeed = 6f;
        public float moveSpeed = 6f;
        public float acceleration = 30f;
        public float deceleration = 40f;

        [Header("Jump")]
        public float jumpVelocity = 12f;        // 初速度
        public float gravityScale = 3f;         // 自行调整重力（可用于更好曲线）
        public float fallMultiplier = 2.5f;
        public float terminalVelocityY = -30f;  // 最大下落速度
        public LayerMask groundMask;
        public float groundCheckDistance = 0.1f;
        public Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
        public float coyoteTime = 0.12f;        // 离地后可继续跳的时间窗口
        public float jumpBufferTime = 0.12f;    // 提前按跳键后的有效时间窗口

        [Header("Dash")]
        public float dashVelocity = 20f;
        public float dashCooldownTime = 1f;
        public float dashDuration = 0.2f;
        public float dashRetainSpeed = 0.5f; // Dash 时间结束之后的水平速度分量比例

        public override string Validate()
        {
            var baseErr = base.Validate();
            if (!string.IsNullOrEmpty(baseErr)) return baseErr;
            if (maxSpeed <= 0f) return "maxSpeed must be > 0";
            if (acceleration <= 0f) return "acceleration must be > 0";
            return null;
        }
    }
}
