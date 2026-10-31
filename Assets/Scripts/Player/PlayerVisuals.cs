using System.Collections.Generic;
using Mossbound.Player.StateMachine;
using UnityEngine;

namespace Mossbound.Player
{
    public class PlayerVisuals : MonoBehaviour
    {
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private static readonly int VerticalVelocityParameter = Animator.StringToHash("VerticalVelocity");
        private static readonly int RunAnimSpeedParameter = Animator.StringToHash("RunAnimSpeed");
        private static readonly int IsAirborneParameter = Animator.StringToHash("IsAirborne");
        private static readonly int IsDashingParameter = Animator.StringToHash("IsDashing");
        private static readonly int IsWallSlidingParameter = Animator.StringToHash("IsWallSliding");
        private static readonly int LandedParameter = Animator.StringToHash("Landed");

        [SerializeField] private PlayerController player;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        [Header("Squash & Stretch")]
        [SerializeField] private Vector2 jumpStretch = new Vector2(0.8f, 1.2f);
        [SerializeField] private Vector2 landSquash = new Vector2(1.2f, 0.8f);
        [SerializeField] private Vector2 dashStretch = new Vector2(1.3f, 0.75f);
        [SerializeField] private Vector2 wallSlideSquash = new Vector2(0.8f, 1.15f);
        [SerializeField] private Vector2 turnSquash = new Vector2(0.9f, 1.06f);
        [SerializeField, Min(0f)] private float recoverySpeed = 10f;
        [Tooltip("Sprite pivot to feet, in sprite units. Keeps the feet planted while squashing.")]
        [SerializeField, Min(0f)] private float feetOffset = 0.55f;

        [Header("Air Stretch")]
        [Tooltip("Scale reached at Max Stretch Speed while airborne.")]
        [SerializeField] private Vector2 airStretch = new Vector2(0.9f, 1.12f);
        [SerializeField, Min(0.01f)] private float maxStretchSpeed = 16f;

        [Header("Tilt")]
        [SerializeField, Range(0f, 15f)] private float maxTilt = 8f;
        [SerializeField, Min(0f)] private float tiltSpeed = 12f;

        [Header("Run Animation")]
        [SerializeField, Min(0f)] private float minRunAnimSpeed = 0.5f;

        [Header("Dust")]
        [SerializeField] private Color dustColor = new Color(0.85f, 0.8f, 0.68f, 0.8f);
        [SerializeField, Min(0.01f)] private float dustLifetime = 0.35f;
        [SerializeField, Min(0f)] private float dustSpeed = 2.5f;
        [SerializeField, Min(0)] private int jumpDustCount = 4;
        [SerializeField, Min(0)] private int landDustCount = 7;

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
        private Vector3 basePosition;
        private Vector3 currentScale;
        private Color baseColor;
        private DustPuffs dust;
        private float tilt;
        private int facing;

        private void Awake()
        {
            baseScale = transform.localScale;
            basePosition = transform.localPosition;
            currentScale = baseScale;
            baseColor = spriteRenderer.color;
        }

        private void Start()
        {
            SyncFacing();
            dust = new DustPuffs(spriteRenderer, dustColor, dustLifetime, dustSpeed);
            player.StateMachine.StateChanged += OnStateChanged;
        }

        private void OnDestroy()
        {
            if (player != null && player.StateMachine != null)
                player.StateMachine.StateChanged -= OnStateChanged;
            if (ghostRoot != null)
                Destroy(ghostRoot.gameObject);
            dust?.Dispose();
        }

        private void LateUpdate()
        {
            PlayerStateId state = player.StateMachine.CurrentState.Id;
            UpdateScale(state);
            UpdateTilt(state);
            UpdateAnimator(state);
            UpdateDashTrail();
            dust.Update(Time.deltaTime);
        }

        private void UpdateScale(PlayerStateId state)
        {
            if (player.FacingDirection != facing)
            {
                SyncFacing();
                Squash(turnSquash);
            }

            float recovery = 1f - Mathf.Exp(-recoverySpeed * Time.deltaTime);
            currentScale = Vector3.Lerp(currentScale, GetTargetScale(state), recovery);
            transform.localScale = currentScale;

            float feetShift = feetOffset * (currentScale.y - baseScale.y);
            transform.localPosition = basePosition + Vector3.up * feetShift;
        }

        private void SyncFacing()
        {
            facing = player.FacingDirection;
            spriteRenderer.flipX = facing < 0;
        }

        private Vector3 GetTargetScale(PlayerStateId state)
        {
            if (state == PlayerStateId.Grounded || state == PlayerStateId.Dashing || state == PlayerStateId.WallSliding)
                return baseScale;

            float amount = Mathf.Clamp01(Mathf.Abs(player.Velocity.y) / maxStretchSpeed);
            Vector2 stretch = Vector2.Lerp(Vector2.one, airStretch, amount);
            return new Vector3(baseScale.x * stretch.x, baseScale.y * stretch.y, baseScale.z);
        }

        private void UpdateTilt(PlayerStateId state)
        {
            float target = 0f;
            if (state != PlayerStateId.WallSliding)
                target = -maxTilt * Mathf.Clamp(player.Velocity.x / player.Settings.maxMoveSpeed, -1f, 1f);

            tilt = Mathf.Lerp(tilt, target, 1f - Mathf.Exp(-tiltSpeed * Time.deltaTime));
            transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
        }

        private void OnStateChanged(PlayerState state)
        {
            // A turn caused by the state change must not override that state's own squash.
            SyncFacing();
            spriteRenderer.color = state.Id == PlayerStateId.Dashing ? dashTint : baseColor;

            if (state.Id == PlayerStateId.Grounded)
                animator.SetTrigger(LandedParameter);
            else
                animator.ResetTrigger(LandedParameter);

            switch (state.Id)
            {
                case PlayerStateId.Jumping:
                case PlayerStateId.DoubleJumping:
                case PlayerStateId.WallJumping:
                    Squash(jumpStretch);
                    dust.Emit(FeetPosition, jumpDustCount);
                    break;
                case PlayerStateId.Dashing:
                    Squash(dashStretch);
                    ghostTimer = 0f;
                    if (player.Sensor.IsGrounded)
                        dust.Emit(FeetPosition, jumpDustCount);
                    break;
                case PlayerStateId.WallSliding:
                    Squash(wallSlideSquash);
                    break;
                case PlayerStateId.Grounded:
                    Squash(landSquash);
                    dust.Emit(FeetPosition, landDustCount);
                    break;
            }
        }

        private Vector3 FeetPosition =>
            transform.parent.TransformPoint(basePosition + Vector3.down * (feetOffset * baseScale.y));

        private void UpdateAnimator(PlayerStateId state)
        {
            float speed = Mathf.Abs(player.Velocity.x);
            animator.SetFloat(SpeedParameter, speed);
            animator.SetFloat(VerticalVelocityParameter, player.Velocity.y);
            animator.SetFloat(RunAnimSpeedParameter, Mathf.Max(minRunAnimSpeed, speed / player.Settings.maxMoveSpeed));
            animator.SetBool(IsAirborneParameter, state != PlayerStateId.Grounded
                && state != PlayerStateId.Dashing && state != PlayerStateId.WallSliding);
            animator.SetBool(IsDashingParameter, state == PlayerStateId.Dashing);
            animator.SetBool(IsWallSlidingParameter, state == PlayerStateId.WallSliding);
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
            currentScale = new Vector3(baseScale.x * scale.x, baseScale.y * scale.y, baseScale.z);
        }
    }
}
