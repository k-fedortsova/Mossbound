using System;

namespace Mossbound.Player.StateMachine
{
    public class PlayerStateMachine
    {
        public PlayerState CurrentState { get; private set; }
        public PlayerState PreviousState { get; private set; }

        public event Action<PlayerState> StateChanged;

        public void Initialize(PlayerState startingState)
        {
            CurrentState = startingState;
            CurrentState.Enter();
            StateChanged?.Invoke(CurrentState);
        }

        public void ChangeState(PlayerState nextState)
        {
            CurrentState.Exit();
            PreviousState = CurrentState;
            CurrentState = nextState;
            CurrentState.Enter();
            StateChanged?.Invoke(CurrentState);
        }

        public void FixedTick(float deltaTime)
        {
            CurrentState.FixedTick(deltaTime);
        }
    }
}
