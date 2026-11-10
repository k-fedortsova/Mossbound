using UnityEngine;
using UnityEngine.Serialization;

namespace Mossbound.Player
{
    [CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "Mossbound/Player Movement Settings")]
    public class PlayerMovementSettings : ScriptableObject
    {
        [field: SerializeField, Header("Move"), Min(0f), FormerlySerializedAs("maxMoveSpeed")]
        public float MaxMoveSpeed { get; private set; } = 7f;
        [field: SerializeField, Min(0f), FormerlySerializedAs("groundAcceleration")]
        public float GroundAcceleration { get; private set; } = 45f;
        [field: SerializeField, Min(0f), FormerlySerializedAs("groundDeceleration")]
        public float GroundDeceleration { get; private set; } = 55f;
        [field: SerializeField, Min(0f), FormerlySerializedAs("airAcceleration")]
        public float AirAcceleration { get; private set; } = 30f;

        [field: SerializeField, Header("Jump"), Min(0.1f), FormerlySerializedAs("targetJumpHeight")]
        public float TargetJumpHeight { get; private set; } = 2.5f;
        [field: SerializeField, Min(0.05f), FormerlySerializedAs("targetTimeToApex")]
        public float TargetTimeToApex { get; private set; } = 0.35f;
        [field: SerializeField, Min(0f), FormerlySerializedAs("jumpBufferTime")]
        public float JumpBufferTime { get; private set; } = 0.1f;
        [field: SerializeField, Min(0f), FormerlySerializedAs("coyoteTime")]
        public float CoyoteTime { get; private set; } = 0.1f;
        [field: SerializeField, Min(1f), FormerlySerializedAs("fallGravityMultiplier")]
        public float FallGravityMultiplier { get; private set; } = 1.5f;
        [field: SerializeField, Min(0f), FormerlySerializedAs("maxFallSpeed")]
        public float MaxFallSpeed { get; private set; } = 16f;
        [field: SerializeField, Min(1f), FormerlySerializedAs("jumpCutGravityMultiplier")]
        public float JumpCutGravityMultiplier { get; private set; } = 3f;

        [field: SerializeField, Header("Double Jump"), Min(0f), FormerlySerializedAs("doubleJumpVerticalSpeed")]
        public float DoubleJumpVerticalSpeed { get; private set; } = 11.5f;
        [field: SerializeField, Min(0), FormerlySerializedAs("maxAirJumps")]
        public int MaxAirJumps { get; private set; } = 1;

        [field: SerializeField, Header("Dash"), Min(0.01f), FormerlySerializedAs("dashDuration")]
        public float DashDuration { get; private set; } = 0.16f;
        [field: SerializeField, Min(0f), FormerlySerializedAs("dashSpeed")]
        public float DashSpeed { get; private set; } = 22f;
        [field: SerializeField, Min(0), FormerlySerializedAs("maxAirDashes")]
        public int MaxAirDashes { get; private set; } = 1;
        [field: SerializeField, Tooltip("Applies to ground dashes only."), Min(0f), FormerlySerializedAs("dashCooldown")]
        public float DashCooldown { get; private set; } = 0.3f;
        [field: SerializeField, Min(0f), FormerlySerializedAs("dashGravityScale")]
        public float DashGravityScale { get; private set; } = 0f;
        [field: SerializeField, Min(0f), FormerlySerializedAs("dashExitSpeed")]
        public float DashExitSpeed { get; private set; } = 8f;
        [field: SerializeField, Min(0.01f), FormerlySerializedAs("dashExitDecayTime")]
        public float DashExitDecayTime { get; private set; } = 0.2f;
        [field: SerializeField, Min(0f), FormerlySerializedAs("dashBufferTime")]
        public float DashBufferTime { get; private set; } = 0.1f;

        [field: SerializeField, Header("Wall Jump"), Min(0f), FormerlySerializedAs("wallJumpHorizontalSpeed")]
        public float WallJumpHorizontalSpeed { get; private set; } = 8f;
        [field: SerializeField, Min(0f), FormerlySerializedAs("wallJumpVerticalSpeed")]
        public float WallJumpVerticalSpeed { get; private set; } = 13f;
        [field: SerializeField, Min(0f), FormerlySerializedAs("wallJumpControlLockTime")]
        public float WallJumpControlLockTime { get; private set; } = 0.08f;
        [field: SerializeField, Min(0.01f), FormerlySerializedAs("wallCheckDistance")]
        public float WallCheckDistance { get; private set; } = 0.06f;

        [field: SerializeField, Header("Wall Contact"), Min(0f), FormerlySerializedAs("wallSlideSpeed")]
        public float WallSlideSpeed { get; private set; } = 2f;
        [field: SerializeField, Min(0f), FormerlySerializedAs("wallCoyoteTime")]
        public float WallCoyoteTime { get; private set; } = 0.1f;
        [field: SerializeField, FormerlySerializedAs("wallClingRequiresInput")]
        public bool WallClingRequiresInput { get; private set; } = true;
        [field: SerializeField, FormerlySerializedAs("allowSameWallRepeat")]
        public bool AllowSameWallRepeat { get; private set; } = false;
        [field: SerializeField, FormerlySerializedAs("allowDashFromWall")]
        public bool AllowDashFromWall { get; private set; } = false;

        public float Gravity => 2f * TargetJumpHeight / (TargetTimeToApex * TargetTimeToApex);
        public float JumpVelocity => Gravity * TargetTimeToApex;
    }
}
