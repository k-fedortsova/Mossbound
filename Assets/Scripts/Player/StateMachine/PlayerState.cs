namespace Mossbound.Player.StateMachine
{
    public abstract class PlayerState
    {
        protected readonly PlayerController Player;

        protected PlayerState(PlayerController player)
        {
            Player = player;
        }

        public abstract PlayerStateId Id { get; }

        protected PlayerMovementSettings Settings => Player.Settings;

        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
        }

        public abstract void FixedTick(float deltaTime);

        protected void ChangeState(PlayerState next)
        {
            Player.StateMachine.ChangeState(next);
        }
    }
}
