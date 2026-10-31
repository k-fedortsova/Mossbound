using Mossbound.Player.StateMachine;
using UnityEngine;

namespace Mossbound.Player.States
{
    public class JumpingState : AirborneState
    {
        public JumpingState(PlayerController player) : base(player)
        {
        }

        public override PlayerStateId Id => PlayerStateId.Jumping;

        public override void Enter()
        {
            Player.Input.ConsumeJump();
            Player.Velocity = new Vector2(Player.Velocity.x, Settings.JumpVelocity);
        }

        protected override float GetGravityMultiplier()
        {
            if (Player.Velocity.y > 0f && !Player.Input.IsJumpHeld)
                return Settings.jumpCutGravityMultiplier;
            return base.GetGravityMultiplier();
        }
    }
}
