using Mossbound.Player.StateMachine;
using UnityEngine;

namespace Mossbound.Player.States
{
    public class WallJumpingState : AirborneState
    {
        private int wallSide;

        public WallJumpingState(PlayerController player) : base(player)
        {
        }

        public override PlayerStateId Id => PlayerStateId.WallJumping;

        public void SetWallSide(int side)
        {
            wallSide = side;
        }

        public override void Enter()
        {
            Player.Input.ConsumeJump();
            Player.ClearWallCoyote();
            Player.LastWallJumpSide = wallSide;
            Player.SetFacing(-wallSide);
            Player.LockControl(wallSide, Settings.wallJumpControlLockTime);
            Player.Velocity = new Vector2(-wallSide * Settings.wallJumpHorizontalSpeed, Settings.wallJumpVerticalSpeed);
        }
    }
}
