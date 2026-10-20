using Mossbound.Player.StateMachine;
using UnityEngine;

namespace Mossbound.Player
{
    public class PlayerVisuals : MonoBehaviour
    {
        private static readonly int IdleAnimation = Animator.StringToHash("Idle");
        private static readonly int RunAnimation = Animator.StringToHash("Run");
        private static readonly int JumpAnimation = Animator.StringToHash("Jump");
        private static readonly int FallAnimation = Animator.StringToHash("Fall");
        private static readonly int DashAnimation = Animator.StringToHash("Dash");
        private static readonly int WallSlideAnimation = Animator.StringToHash("WallSlide");

        [SerializeField] private PlayerController player;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        [Header("Squash & Stretch")]
        [SerializeField] private Vector2 jumpStretch = new Vector2(0.8f, 1.2f);
        [SerializeField] private Vector2 landSquash = new Vector2(1.2f, 0.8f);
        [SerializeField] private Vector2 dashStretch = new Vector2(1.3f, 0.75f);
        [SerializeField, Min(0f)] private float recoverySpeed = 10f;

        [Header("Dash")]
        [SerializeField] private Color dashTint = new Color(0.6f, 0.9f, 1f);

        private Vector3 baseScale;
        private Color baseColor;
        private int currentAnimation;

        private void Awake()
        {
            baseScale = transform.localScale;
            baseColor = spriteRenderer.color;
        }

        private void Start()
        {
            player.StateMachine.StateChanged += OnStateChanged;
        }

        private void OnDestroy()
        {
            if (player != null && player.StateMachine != null)
                player.StateMachine.StateChanged -= OnStateChanged;
        }

        private void LateUpdate()
        {
            spriteRenderer.flipX = player.FacingDirection < 0;
            float recovery = 1f - Mathf.Exp(-recoverySpeed * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, recovery);
            PlayAnimation(SelectAnimation(player.StateMachine.CurrentState.Id));
        }

        private void OnStateChanged(PlayerState state)
        {
            spriteRenderer.color = state.Id == PlayerStateId.Dashing ? dashTint : baseColor;

            switch (state.Id)
            {
                case PlayerStateId.Jumping:
                case PlayerStateId.DoubleJumping:
                case PlayerStateId.WallJumping:
                    Squash(jumpStretch);
                    break;
                case PlayerStateId.Dashing:
                    Squash(dashStretch);
                    break;
                case PlayerStateId.Grounded:
                    Squash(landSquash);
                    break;
            }
        }

        private int SelectAnimation(PlayerStateId state)
        {
            switch (state)
            {
                case PlayerStateId.Grounded:
                    bool running = player.Input.Horizontal != 0 && Mathf.Abs(player.Velocity.x) > 0.1f;
                    return running ? RunAnimation : IdleAnimation;
                case PlayerStateId.Dashing:
                    return DashAnimation;
                case PlayerStateId.WallSliding:
                    return WallSlideAnimation;
                default:
                    return player.Velocity.y > 0f ? JumpAnimation : FallAnimation;
            }
        }

        private void PlayAnimation(int animation)
        {
            if (animation == currentAnimation)
                return;

            animator.Play(animation);
            currentAnimation = animation;
        }

        private void Squash(Vector2 scale)
        {
            transform.localScale = new Vector3(baseScale.x * scale.x, baseScale.y * scale.y, baseScale.z);
        }
    }
}
