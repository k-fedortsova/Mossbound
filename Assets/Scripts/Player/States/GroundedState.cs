using Mossbound.Player.StateMachine;

namespace Mossbound.Player.States
{
    public class GroundedState : PlayerState
    {
        public GroundedState(PlayerController player) : base(player)
        {
        }

        public override PlayerStateId Id => PlayerStateId.Grounded;

        public override void Enter()
        {
            Player.RestoreAirCharges();
            Player.ClearWallCoyote();
        }

        public override void FixedTick(float deltaTime)
        {
            if (!Player.Sensor.IsGrounded)
            {
                ChangeState(Player.FallingState);
                return;
            }

            if (Player.IsJumpBuffered)
            {
                ChangeState(Player.JumpingState);
                return;
            }

            if (Player.IsDashBuffered && Player.CanGroundDash)
            {
                ChangeState(Player.DashingState);
                return;
            }

            Player.ApplyHorizontalMovement(Settings.groundAcceleration, Settings.groundDeceleration, deltaTime);
            Player.ApplyGravity(1f, deltaTime);
        }
    }
}
