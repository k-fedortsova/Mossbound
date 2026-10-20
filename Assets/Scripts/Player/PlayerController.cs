using Mossbound.Player.StateMachine;
using Mossbound.Player.States;
using UnityEngine;

namespace Mossbound.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputReader), typeof(PlayerCollisionSensor))]
    [RequireComponent(typeof(PlayerAbilities))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerMovementSettings settings;

        private Rigidbody2D body;
        private bool isFrozen;

        private float controlLockTimer;
        private int controlLockDirection;
        private float dashExitTimer;
        private int dashExitDirection;
        private float groundDashCooldownTimer;
        private float wallCoyoteTimer;
        private int wallCoyoteSide;

        public PlayerMovementSettings Settings => settings;
        public PlayerInputReader Input { get; private set; }
        public PlayerCollisionSensor Sensor { get; private set; }
        public PlayerAbilities Abilities { get; private set; }
        public PlayerStateMachine StateMachine { get; private set; }

        public GroundedState GroundedState { get; private set; }
        public JumpingState JumpingState { get; private set; }
        public DoubleJumpingState DoubleJumpingState { get; private set; }
        public FallingState FallingState { get; private set; }
        public DashingState DashingState { get; private set; }
        public WallSlidingState WallSlidingState { get; private set; }
        public WallJumpingState WallJumpingState { get; private set; }

        public int FacingDirection { get; private set; } = 1;
        public int AirJumpsLeft { get; set; }
        public int AirDashesLeft { get; set; }
        public int LastWallJumpSide { get; set; }

        public Vector2 Velocity
        {
            get => body.linearVelocity;
            set => body.linearVelocity = value;
        }

        public bool CanDoubleJump => Abilities.Has(PlayerAbility.DoubleJump) && AirJumpsLeft > 0;
        public bool CanGroundDash => Abilities.Has(PlayerAbility.Dash) && groundDashCooldownTimer <= 0f;
        public bool CanAirDash => Abilities.Has(PlayerAbility.Dash) && AirDashesLeft > 0;
        public bool IsJumpBuffered => Input.IsJumpBuffered(settings.jumpBufferTime);
        public bool IsDashBuffered => Input.IsDashBuffered(settings.dashBufferTime);

        public int WallJumpSide
        {
            get
            {
                if (Sensor.WallSide != 0)
                    return Sensor.WallSide;
                return wallCoyoteTimer > 0f ? wallCoyoteSide : 0;
            }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            Input = GetComponent<PlayerInputReader>();
            Sensor = GetComponent<PlayerCollisionSensor>();
            Abilities = GetComponent<PlayerAbilities>();

            StateMachine = new PlayerStateMachine();
            GroundedState = new GroundedState(this);
            JumpingState = new JumpingState(this);
            DoubleJumpingState = new DoubleJumpingState(this);
            FallingState = new FallingState(this);
            DashingState = new DashingState(this);
            WallSlidingState = new WallSlidingState(this);
            WallJumpingState = new WallJumpingState(this);
        }

        private void Start()
        {
            RestoreAirCharges();
            Sensor.Refresh();
            StateMachine.Initialize(Sensor.IsGrounded ? GroundedState : FallingState);
        }

        private void FixedUpdate()
        {
            if (isFrozen)
                return;

            float deltaTime = Time.fixedDeltaTime;
            Sensor.Refresh();
            UpdateTimers(deltaTime);
            StateMachine.FixedTick(deltaTime);
            UpdateFacing();
        }

        public void SetFrozen(bool frozen)
        {
            isFrozen = frozen;
            body.simulated = !frozen;
            if (frozen)
                body.linearVelocity = Vector2.zero;
        }

        public void ResetMotion()
        {
            StateMachine.ChangeState(FallingState);
            body.linearVelocity = Vector2.zero;
            controlLockTimer = 0f;
            dashExitTimer = 0f;
            groundDashCooldownTimer = 0f;
            wallCoyoteTimer = 0f;
            Input.ClearBuffers();
            RestoreAirCharges();
        }

        public void RestoreAirCharges()
        {
            AirJumpsLeft = settings.maxAirJumps;
            AirDashesLeft = settings.maxAirDashes;
            LastWallJumpSide = 0;
        }

        public bool CanWallJumpFrom(int side)
        {
            if (side == 0 || !Abilities.Has(PlayerAbility.WallJump))
                return false;
            return settings.allowSameWallRepeat || side != LastWallJumpSide;
        }

        public bool CanStartWallSlide()
        {
            int side = Sensor.WallSide;
            if (side == 0 || Sensor.IsGrounded || !Abilities.Has(PlayerAbility.WallJump))
                return false;
            if (Velocity.y > 0f)
                return false;
            return !settings.wallClingRequiresInput || Input.Horizontal == side;
        }

        public void StartWallJump(int side)
        {
            WallJumpingState.SetWallSide(side);
            StateMachine.ChangeState(WallJumpingState);
        }

        public void SetFacing(int direction)
        {
            if (direction != 0)
                FacingDirection = direction > 0 ? 1 : -1;
        }

        public void LockControl(int direction, float duration)
        {
            controlLockDirection = direction;
            controlLockTimer = duration;
        }

        public void StartDashExit(int direction)
        {
            dashExitDirection = direction;
            dashExitTimer = settings.dashExitDecayTime;
        }

        public void StartGroundDashCooldown()
        {
            groundDashCooldownTimer = settings.dashCooldown;
        }

        public void ClearWallCoyote()
        {
            wallCoyoteTimer = 0f;
        }

        public void ApplyHorizontalMovement(float acceleration, float deceleration, float deltaTime)
        {
            int input = Input.Horizontal;
            if (controlLockTimer > 0f && input == controlLockDirection)
                return;

            Vector2 velocity = body.linearVelocity;
            if (dashExitTimer > 0f && input == dashExitDirection)
            {
                float progress = 1f - dashExitTimer / settings.dashExitDecayTime;
                float speed = Mathf.Lerp(settings.dashExitSpeed, settings.maxMoveSpeed, progress);
                velocity.x = dashExitDirection * Mathf.Max(speed, settings.maxMoveSpeed);
            }
            else
            {
                float target = input * settings.maxMoveSpeed;
                bool speedingUp = input != 0 && (Mathf.Approximately(velocity.x, 0f) || Mathf.Sign(velocity.x) == input);
                float rate = speedingUp ? acceleration : deceleration;
                velocity.x = Mathf.MoveTowards(velocity.x, target, rate * deltaTime);
                dashExitTimer = 0f;
            }

            body.linearVelocity = velocity;
        }

        public void ApplyGravity(float multiplier, float deltaTime)
        {
            Vector2 velocity = body.linearVelocity;
            velocity.y -= settings.Gravity * multiplier * deltaTime;
            velocity.y = Mathf.Max(velocity.y, -settings.maxFallSpeed);
            body.linearVelocity = velocity;
        }

        private void UpdateTimers(float deltaTime)
        {
            controlLockTimer -= deltaTime;
            dashExitTimer -= deltaTime;
            groundDashCooldownTimer -= deltaTime;

            if (!Sensor.IsGrounded && Sensor.WallSide != 0)
            {
                wallCoyoteTimer = settings.wallCoyoteTime;
                wallCoyoteSide = Sensor.WallSide;
            }
            else
            {
                wallCoyoteTimer -= deltaTime;
            }
        }

        private void UpdateFacing()
        {
            PlayerStateId state = StateMachine.CurrentState.Id;
            if (state == PlayerStateId.Dashing || state == PlayerStateId.WallSliding)
                return;
            if (controlLockTimer > 0f)
                return;
            SetFacing(Input.Horizontal);
        }
    }
}
