using System;
using Mossbound.Combat;
using UnityEngine;

namespace Mossbound.Player
{
    [RequireComponent(typeof(PlayerController), typeof(PlayerInputReader))]
    public class PlayerAttack : MonoBehaviour, IMoveSpeedModifier
    {
        [Header("Timing")]
        [SerializeField, Min(0.05f)] private float attackDuration = 0.35f;
        [SerializeField, Min(0.05f)] private float cooldown = 0.45f;
        [SerializeField, Min(0f)] private float attackBufferTime = 0.12f;
        [SerializeField, Min(0f)] private float comboResetTime = 0.8f;
        [SerializeField, Range(0f, 1f)] private float moveSpeedMultiplier = 0.2f;

        [Header("Hitbox")]
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField] private Vector2 hitboxOffset = new Vector2(0.8f, 0f);
        [SerializeField, Min(0.1f)] private float hitboxRadius = 0.65f;
        [SerializeField] private LayerMask targetLayers = ~0;

        private PlayerController controller;
        private PlayerInputReader input;
        private PlayerGuard guard;

        private float attackTimer;
        private float cooldownTimer;
        private float comboResetTimer;
        private int nextAttackIndex;

        public event Action<int> AttackStarted;

        public bool IsAttacking => attackTimer > 0f;
        public float MoveSpeedMultiplier => IsAttacking ? moveSpeedMultiplier : 1f;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            input = GetComponent<PlayerInputReader>();
            guard = GetComponent<PlayerGuard>();
        }

        private void Update()
        {
            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;
            if (cooldownTimer > 0f)
                cooldownTimer -= Time.deltaTime;

            if (comboResetTimer > 0f)
            {
                comboResetTimer -= Time.deltaTime;
                if (comboResetTimer <= 0f)
                    nextAttackIndex = 0;
            }

            if (!controller.Sensor.IsGrounded)
            {
                attackTimer = 0f;
                return;
            }

            bool guarding = guard != null && guard.IsGuarding;
            if (!guarding && cooldownTimer <= 0f && input.IsAttackBuffered(attackBufferTime))
            {
                input.ConsumeAttack();
                PerformAttack();
            }
        }

        private void PerformAttack()
        {
            int currentIndex = nextAttackIndex;
            nextAttackIndex = (nextAttackIndex + 1) % 2;
            comboResetTimer = comboResetTime;

            attackTimer = attackDuration;
            cooldownTimer = cooldown;
            AttackStarted?.Invoke(currentIndex);

            Vector2 center = GetHitboxCenter();
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, hitboxRadius, targetLayers);
            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject == gameObject)
                    continue;

                if (hit.TryGetComponent(out IDamageable damageable))
                    damageable.TakeDamage(new DamageInfo(damage, transform.position));
            }
        }

        private Vector2 GetHitboxCenter()
        {
            int dir = controller != null ? controller.FacingDirection : 1;
            return (Vector2)transform.position + new Vector2(hitboxOffset.x * dir, hitboxOffset.y);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetHitboxCenter(), hitboxRadius);
        }
    }
}