using System.Collections.Generic;
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

        [Header("Dash Trail")]
        [SerializeField, Min(0.01f)] private float ghostInterval = 0.03f;
        [SerializeField, Min(0.01f)] private float ghostLifetime = 0.2f;
        [SerializeField] private Color ghostColor = new Color(0.6f, 0.9f, 1f, 0.55f);
        [Tooltip("Drawn behind the terrain so the trail never covers platforms.")]
        [SerializeField] private int ghostSortingOrder = -5;

        private readonly List<(SpriteRenderer renderer, float age)> ghosts = new List<(SpriteRenderer, float)>();
        private readonly Stack<SpriteRenderer> ghostPool = new Stack<SpriteRenderer>();
        private Transform ghostRoot;
        private float ghostTimer;

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
            if (ghostRoot != null)
                Destroy(ghostRoot.gameObject);
        }

        private void LateUpdate()
        {
            spriteRenderer.flipX = player.FacingDirection < 0;
            float recovery = 1f - Mathf.Exp(-recoverySpeed * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, recovery);
            PlayAnimation(SelectAnimation(player.StateMachine.CurrentState.Id));
            UpdateDashTrail();
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
                    ghostTimer = 0f;
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

        private void UpdateDashTrail()
        {
            if (player.StateMachine.CurrentState.Id == PlayerStateId.Dashing)
            {
                ghostTimer -= Time.deltaTime;
                if (ghostTimer <= 0f)
                {
                    SpawnGhost();
                    ghostTimer += ghostInterval;
                }
            }

            for (int i = ghosts.Count - 1; i >= 0; i--)
            {
                var (ghost, age) = ghosts[i];
                age += Time.deltaTime;
                if (age >= ghostLifetime)
                {
                    ghost.gameObject.SetActive(false);
                    ghostPool.Push(ghost);
                    ghosts.RemoveAt(i);
                    continue;
                }

                Color color = ghostColor;
                color.a *= 1f - age / ghostLifetime;
                ghost.color = color;
                ghosts[i] = (ghost, age);
            }
        }

        private void SpawnGhost()
        {
            if (ghostRoot == null)
                ghostRoot = new GameObject("Dash Trail").transform;

            SpriteRenderer ghost = ghostPool.Count > 0 ? ghostPool.Pop() : CreateGhost();
            ghost.gameObject.SetActive(true);
            ghost.sprite = spriteRenderer.sprite;
            ghost.flipX = spriteRenderer.flipX;
            ghost.color = ghostColor;
            ghost.transform.SetPositionAndRotation(transform.position, transform.rotation);
            ghost.transform.localScale = transform.lossyScale;
            ghosts.Add((ghost, 0f));
        }

        private SpriteRenderer CreateGhost()
        {
            var ghost = new GameObject("Ghost").AddComponent<SpriteRenderer>();
            ghost.transform.SetParent(ghostRoot, false);
            ghost.sharedMaterial = spriteRenderer.sharedMaterial;
            ghost.sortingLayerID = spriteRenderer.sortingLayerID;
            ghost.sortingOrder = ghostSortingOrder;
            return ghost;
        }

        private void Squash(Vector2 scale)
        {
            transform.localScale = new Vector3(baseScale.x * scale.x, baseScale.y * scale.y, baseScale.z);
        }
    }
}
