using Mossbound.Player.StateMachine;
using UnityEngine;

namespace Mossbound.Player.States
{
    public class WallSlidingState : PlayerState
    {
        private int wallSide;
        private float releaseTimer;

        public WallSlidingState(PlayerController player) : base(player)
        {
        }

        public override PlayerStateId Id => PlayerStateId.WallSliding;

        public override void Enter()
        {
            wallSide = Player.Sensor.WallSide;
            releaseTimer = 0f;
            Player.SetFacing(wallSide);
            Player.Velocity = new Vector2(0f, -Settings.wallSlideSpeed);
        }

        public override void FixedTick(float deltaTime)
        {
            if (Player.Sensor.IsGrounded)
            {
                ChangeState(Player.GroundedState);
                return;
            }

            if (Player.IsJumpBuffered)
            {
                if (Player.CanWallJumpFrom(wallSide))
                {
                    Player.StartWallJump(wallSide);
                    return;
                }

                if (Player.CanDoubleJump)
                {
                    ChangeState(Player.DoubleJumpingState);
                    return;
                }
            }

            if (Settings.allowDashFromWall && Player.IsDashBuffered && Player.CanAirDash)
            {
                Player.SetFacing(-wallSide);
                ChangeState(Player.DashingState);
                return;
            }

            if (Player.Sensor.WallSide != wallSide)
            {
                ChangeState(Player.FallingState);
                return;
            }

            bool holdingTowardWall = Player.Input.Horizontal == wallSide;
            releaseTimer = holdingTowardWall || !Settings.wallClingRequiresInput ? 0f : releaseTimer + deltaTime;
            if (releaseTimer > Settings.wallCoyoteTime)
            {
                ChangeState(Player.FallingState);
                return;
            }

            Player.Velocity = new Vector2(0f, -Settings.wallSlideSpeed);
        }
    }
}
