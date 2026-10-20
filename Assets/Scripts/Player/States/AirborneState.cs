using Mossbound.Player.StateMachine;

namespace Mossbound.Player.States
{
    public abstract class AirborneState : PlayerState
    {
        protected AirborneState(PlayerController player) : base(player)
        {
        }

        public override void FixedTick(float deltaTime)
        {
            if (TryLand() || TryJump() || TryDash() || TryStartWallSlide())
                return;

            Player.ApplyHorizontalMovement(Settings.airAcceleration, Settings.airAcceleration, deltaTime);
            Player.ApplyGravity(GetGravityMultiplier(), deltaTime);
            OnAfterMovement();
        }

        protected virtual float GetGravityMultiplier()
        {
            return Player.Velocity.y < 0f ? Settings.fallGravityMultiplier : 1f;
        }

        protected virtual void OnAfterMovement()
        {
            if (Player.Velocity.y <= 0f)
                ChangeState(Player.FallingState);
        }

        protected virtual bool TryJump()
        {
            if (!Player.IsJumpBuffered)
                return false;

            int wallSide = Player.WallJumpSide;
            if (Player.CanWallJumpFrom(wallSide))
            {
                Player.StartWallJump(wallSide);
                return true;
            }

            if (Player.CanDoubleJump)
            {
                ChangeState(Player.DoubleJumpingState);
                return true;
            }

            return false;
        }

        private bool TryLand()
        {
            if (!Player.Sensor.IsGrounded || Player.Velocity.y > 0.01f)
                return false;

            ChangeState(Player.GroundedState);
            return true;
        }

        private bool TryDash()
        {
            if (!Player.IsDashBuffered || !Player.CanAirDash)
                return false;

            ChangeState(Player.DashingState);
            return true;
        }

        private bool TryStartWallSlide()
        {
            if (!Player.CanStartWallSlide())
                return false;

            ChangeState(Player.WallSlidingState);
            return true;
        }
    }
}
