namespace Mossbound.Player.StateMachine
{
    public enum PlayerStateId
    {
        //Явне вказування індексів енамів - може врятувати життя в подальшому, особливо при роботі з інспектором
        Grounded = 0,
        Jumping = 1,
        DoubleJumping,
        Falling,
        Dashing,
        WallSliding,
        WallJumping,
    }
}
