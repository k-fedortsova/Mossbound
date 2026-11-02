using UnityEngine;

namespace Mossbound.Player
{
    [CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "Mossbound/Player Movement Settings")]
    public class PlayerMovementSettings : ScriptableObject
    {
        [Header("Move")]
        [Min(0f)] public float maxMoveSpeed = 7f;
        [Min(0f)] public float groundAcceleration = 45f;
        [Min(0f)] public float groundDeceleration = 55f;
        [Min(0f)] public float airAcceleration = 30f;

        [Header("Jump")]
        [Min(0.1f)] public float targetJumpHeight = 2.5f;
        [Min(0.05f)] public float targetTimeToApex = 0.35f;
        [Min(0f)] public float jumpBufferTime = 0.1f;
        [Min(0f)] public float coyoteTime = 0.1f;
        [Min(1f)] public float fallGravityMultiplier = 1.5f;
        [Min(0f)] public float maxFallSpeed = 16f;
        [Min(1f)] public float jumpCutGravityMultiplier = 3f;

        [Header("Double Jump")]
        [Min(0f)] public float doubleJumpVerticalSpeed = 11.5f;
        [Min(0)] public int maxAirJumps = 1;

        [Header("Dash")]
        [Min(0.01f)] public float dashDuration = 0.16f;
        [Min(0f)] public float dashSpeed = 22f;
        [Min(0)] public int maxAirDashes = 1;
        [Tooltip("Applies to ground dashes only.")]
        [Min(0f)] public float dashCooldown = 0.3f;
        [Min(0f)] public float dashGravityScale = 0f;
        [Min(0f)] public float dashExitSpeed = 8f;
        [Min(0.01f)] public float dashExitDecayTime = 0.2f;
        [Min(0f)] public float dashBufferTime = 0.1f;

        [Header("Wall Jump")]
        [Min(0f)] public float wallJumpHorizontalSpeed = 8f;
        [Min(0f)] public float wallJumpVerticalSpeed = 13f;
        [Min(0f)] public float wallJumpControlLockTime = 0.08f;
        [Min(0.01f)] public float wallCheckDistance = 0.06f;

        [Header("Wall Contact")]
        [Min(0f)] public float wallSlideSpeed = 2f;
        [Min(0f)] public float wallCoyoteTime = 0.1f;
        public bool wallClingRequiresInput = true;
        public bool allowSameWallRepeat = false;
        public bool allowDashFromWall = false;

        public float Gravity => 2f * targetJumpHeight / (targetTimeToApex * targetTimeToApex);
        public float JumpVelocity => Gravity * targetTimeToApex;
    }
}
