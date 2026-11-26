using Mossbound.Combat;
using UnityEngine;

namespace Mossbound.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class GolemAI : MonoBehaviour
    {
        private enum State { Patrol, Idle, Chase, Attack }

        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float patrolSpeed = 1.8f;
        [SerializeField, Min(0.1f)] private float chaseSpeed = 3.2f;
        [SerializeField, Min(0.5f)] private float patrolRadius = 3f;

        [Header("Idle Pauses")]
        [SerializeField, Min(0.2f)] private float minWalkTime = 1.5f;
        [SerializeField, Min(0.2f)] private float maxWalkTime = 3.5f;
        [SerializeField, Min(0.2f)] private float minIdleTime = 1.5f;
        [SerializeField, Min(0.2f)] private float maxIdleTime = 3f;

        [Header("Detection")]
        [SerializeField, Min(0.5f)] private float detectRange = 5f;
        [SerializeField, Min(0.5f)] private float loseRange = 7f;
        [SerializeField, Min(0.3f)] private float attackRange = 1.2f;

        [Header("Attack")]
        [SerializeField, Min(1)] private int attackDamage = 1;
        [SerializeField, Min(0.05f)] private float attackHitDelay = 0.25f;
        [SerializeField, Min(0.1f)] private float attackDuration = 0.6f;
        [SerializeField, Min(0.1f)] private float attackCooldown = 1.2f;
        [SerializeField] private Vector2 attackHitboxOffset = new Vector2(0.8f, 0f);
        [SerializeField, Min(0.1f)] private float attackHitboxRadius = 0.65f;
        [SerializeField] private LayerMask targetLayers = ~0;

        [Header("Environment Checks")]
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private Vector2 ledgeCheckOffset = new Vector2(0.5f, -0.4f);
        [SerializeField, Min(0.1f)] private float ledgeCheckDistance = 0.6f;
        [SerializeField] private Vector2 wallCheckOffset = new Vector2(0.45f, 0f);
        [SerializeField, Min(0.05f)] private float wallCheckDistance = 0.25f;

        [Header("Visuals (Optional)")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private static readonly int AttackParameter = Animator.StringToHash("Attack");

        private Rigidbody2D body;
        private Transform player;
        private Vector2 spawnPosition;
        private State state = State.Patrol;
        private int facing = 1;
        private float stateTimer;
        private float attackCooldownTimer;
        private bool hitDealt;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spawnPosition = transform.position;

            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;

            EnterPatrol();
        }

        private void FixedUpdate()
        {
            if (attackCooldownTimer > 0f)
                attackCooldownTimer -= Time.fixedDeltaTime;

            if (state != State.Attack)
                UpdateDetection();

            switch (state)
            {
                case State.Patrol:
                    TickPatrol();
                    break;
                case State.Idle:
                    TickIdle();
                    break;
                case State.Chase:
                    TickChase();
                    break;
                case State.Attack:
                    TickAttack();
                    break;
            }

            if (animator != null)
                animator.SetFloat(SpeedParameter, Mathf.Abs(body.linearVelocity.x));
        }

        private void UpdateDetection()
        {
            if (player == null)
                return;

            float dist = Vector2.Distance(transform.position, player.position);

            if (state != State.Chase && dist <= detectRange)
                state = State.Chase;
            else if (state == State.Chase && dist > loseRange)
                EnterIdle();
        }

        private void TickPatrol()
        {
            stateTimer -= Time.fixedDeltaTime;
            if (stateTimer <= 0f)
            {
                EnterIdle();
                return;
            }

            float offsetFromSpawn = transform.position.x - spawnPosition.x;
            bool outOfBounds = (facing > 0 && offsetFromSpawn >= patrolRadius)
                            || (facing < 0 && offsetFromSpawn <= -patrolRadius);

            if (outOfBounds || IsWallAhead() || !HasGroundAhead())
                SetFacing(-facing);

            SetHorizontalVelocity(facing * patrolSpeed);
        }

        private void TickIdle()
        {
            SetHorizontalVelocity(0f);
            stateTimer -= Time.fixedDeltaTime;
            if (stateTimer <= 0f)
            {
                if (Random.value < 0.25f)
                    SetFacing(-facing);
                EnterPatrol();
            }
        }

        private void TickChase()
        {
            if (player == null)
            {
                EnterIdle();
                return;
            }

            float deltaX = player.position.x - transform.position.x;
            SetFacing(deltaX > 0f ? 1 : -1);

            if (Vector2.Distance(transform.position, player.position) <= attackRange)
            {
                SetHorizontalVelocity(0f);
                if (attackCooldownTimer <= 0f)
                    StartAttack();
                return;
            }

            if (IsWallAhead() || !HasGroundAhead())
            {
                SetHorizontalVelocity(0f);
                return;
            }

            SetHorizontalVelocity(facing * chaseSpeed);
        }

        private void StartAttack()
        {
            state = State.Attack;
            stateTimer = 0f;
            hitDealt = false;
            SetHorizontalVelocity(0f);

            if (animator != null)
                animator.SetTrigger(AttackParameter);
        }

        private void TickAttack()
        {
            SetHorizontalVelocity(0f);
            stateTimer += Time.fixedDeltaTime;

            if (!hitDealt && stateTimer >= attackHitDelay)
            {
                hitDealt = true;
                DealAttackDamage();
            }

            if (stateTimer >= attackDuration)
            {
                attackCooldownTimer = attackCooldown;
                state = State.Chase;
            }
        }

        private void DealAttackDamage()
        {
            Vector2 center = GetAttackHitboxCenter();
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackHitboxRadius, targetLayers);
            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject == gameObject)
                    continue;

                if (hit.CompareTag("Player") && hit.TryGetComponent(out IDamageable damageable))
                    damageable.TakeDamage(new DamageInfo(attackDamage, transform.position));
            }
        }

        private void EnterPatrol()
        {
            state = State.Patrol;
            stateTimer = Random.Range(minWalkTime, maxWalkTime);
        }

        private void EnterIdle()
        {
            state = State.Idle;
            stateTimer = Random.Range(minIdleTime, maxIdleTime);
            SetHorizontalVelocity(0f);
        }

        private void SetFacing(int dir)
        {
            if (dir == 0)
                return;
            facing = dir > 0 ? 1 : -1;
            if (spriteRenderer != null)
                spriteRenderer.flipX = facing < 0;
        }

        private void SetHorizontalVelocity(float speedX)
        {
            body.linearVelocity = new Vector2(speedX, body.linearVelocity.y);
        }

        private Vector2 GetAttackHitboxCenter()
        {
            return (Vector2)transform.position + new Vector2(attackHitboxOffset.x * facing, attackHitboxOffset.y);
        }

        private bool HasGroundAhead()
        {
            Vector2 origin = (Vector2)transform.position + new Vector2(ledgeCheckOffset.x * facing, ledgeCheckOffset.y);
            return Physics2D.Raycast(origin, Vector2.down, ledgeCheckDistance, groundLayers);
        }

        private bool IsWallAhead()
        {
            Vector2 origin = (Vector2)transform.position + new Vector2(wallCheckOffset.x * facing, wallCheckOffset.y);
            return Physics2D.Raycast(origin, Vector2.right * facing, wallCheckDistance, groundLayers);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = Application.isPlaying ? (Vector3)spawnPosition : transform.position;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(center + Vector3.left * patrolRadius, center + Vector3.right * patrolRadius);

            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, detectRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetAttackHitboxCenter(), attackHitboxRadius);

            Gizmos.color = Color.green;
            Vector3 ledgeOrigin = transform.position + new Vector3(ledgeCheckOffset.x * facing, ledgeCheckOffset.y, 0f);
            Gizmos.DrawLine(ledgeOrigin, ledgeOrigin + Vector3.down * ledgeCheckDistance);

            Gizmos.color = Color.cyan;
            Vector3 wallOrigin = transform.position + new Vector3(wallCheckOffset.x * facing, wallCheckOffset.y, 0f);
            Gizmos.DrawLine(wallOrigin, wallOrigin + Vector3.right * (facing * wallCheckDistance));
        }
    }
}