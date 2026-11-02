using Mossbound.Player.StateMachine;

namespace Mossbound.Player.States
{
    public class FallingState : AirborneState
    {
        private float coyoteTimer;

        public FallingState(PlayerController player) : base(player)
        {
        }

        public override PlayerStateId Id => PlayerStateId.Falling;

        public override void Enter()
        {
            bool walkedOffLedge = Player.StateMachine.PreviousState == Player.GroundedState;
            coyoteTimer = walkedOffLedge ? Settings.coyoteTime : 0f;
        }

        public override void FixedTick(float deltaTime)
        {
            coyoteTimer -= deltaTime;
            base.FixedTick(deltaTime);
        }

        protected override bool TryJump()
        {
            if (coyoteTimer > 0f && Player.IsJumpBuffered)
            {
                ChangeState(Player.JumpingState);
                return true;
            }

            return base.TryJump();
        }

        protected override void OnAfterMovement()
        {
        }
    }
}
