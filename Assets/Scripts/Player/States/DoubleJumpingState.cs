using Mossbound.Player.StateMachine;
using UnityEngine;

namespace Mossbound.Player.States
{
    public class DoubleJumpingState : AirborneState
    {
        public DoubleJumpingState(PlayerController player) : base(player)
        {
        }

        public override PlayerStateId Id => PlayerStateId.DoubleJumping;

        public override void Enter()
        {
            Player.Input.ConsumeJump();
            Player.AirJumpsLeft--;
            Player.Velocity = new Vector2(Player.Velocity.x, Settings.doubleJumpVerticalSpeed);
        }
    }
}
