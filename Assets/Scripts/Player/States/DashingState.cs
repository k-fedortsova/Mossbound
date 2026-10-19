using Mossbound.Player.StateMachine;
using UnityEngine;

namespace Mossbound.Player.States
{
    public class DashingState : PlayerState
    {
        private Vector2 direction;
        private float timer;
        private float verticalDrift;
        private float velocityXBeforeDash;
        private bool startedOnGround;

        public DashingState(PlayerController player) : base(player)
        {
        }

        public override PlayerStateId Id => PlayerStateId.Dashing;

        private int HorizontalSign => direction.x > 0.01f ? 1 : direction.x < -0.01f ? -1 : 0;

        public override void Enter()
        {
            Player.Input.ConsumeDash();
            Player.Input.ConsumeJump();

            startedOnGround = Player.StateMachine.PreviousState == Player.GroundedState;
            if (!startedOnGround)
                Player.AirDashesLeft--;

            direction = ReadDirection();
            Player.SetFacing(HorizontalSign);
            velocityXBeforeDash = Player.Velocity.x;
            verticalDrift = 0f;
            timer = Settings.dashDuration;
            Player.Velocity = direction * Settings.dashSpeed;
        }

        public override void Exit()
        {
            int horizontal = HorizontalSign;
            if (horizontal != 0)
            {
                Player.Velocity = new Vector2(horizontal * Settings.dashExitSpeed, 0f);
                Player.StartDashExit(horizontal);
            }
            else
            {
                Player.Velocity = new Vector2(velocityXBeforeDash, 0f);
            }

            if (startedOnGround)
                Player.StartGroundDashCooldown();
        }

        public override void FixedTick(float deltaTime)
        {
            timer -= deltaTime;
            if (timer <= deltaTime * 0.5f)
            {
                ChangeState(Player.Sensor.IsGrounded ? Player.GroundedState : Player.FallingState);
                return;
            }

            if (HitWallAhead())
            {
                ChangeState(Player.WallSlidingState);
                return;
            }

            verticalDrift -= Settings.Gravity * Settings.dashGravityScale * deltaTime;
            Player.Velocity = direction * Settings.dashSpeed + Vector2.up * verticalDrift;
        }

        private Vector2 ReadDirection()
        {
            var input = new Vector2(Player.Input.Horizontal, Player.Input.Vertical);
            if (startedOnGround && input.y < 0f)
                input.y = 0f;
            if (input == Vector2.zero)
                input = new Vector2(Player.FacingDirection, 0f);
            return input.normalized;
        }

        private bool HitWallAhead()
        {
            if (HorizontalSign == 0 || Player.Sensor.IsGrounded)
                return false;
            if (!Player.Abilities.Has(PlayerAbility.WallJump))
                return false;
            return Player.Sensor.WallSide == HorizontalSign;
        }
    }
}
