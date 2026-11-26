using System;
using Mossbound.Combat;
using UnityEngine;

namespace Mossbound.Enemies
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int maxHealth = 3;
        [SerializeField, Min(0f)] private float deathDestroyDelay = 1f;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private Color hitFlashColor = new Color(1f, 0.4f, 0.4f);
        [SerializeField, Min(0.01f)] private float hitFlashDuration = 0.1f;

        private static readonly int HurtParameter = Animator.StringToHash("Hurt");
        private static readonly int DieParameter = Animator.StringToHash("Die");

        private Color baseColor;
        private float flashTimer;

        public event Action<int, int> HealthChanged;
        public event Action Died;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsDead => CurrentHealth <= 0;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            if (spriteRenderer != null)
                baseColor = spriteRenderer.color;
        }

        private void Update()
        {
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f && spriteRenderer != null)
                    spriteRenderer.color = baseColor;
            }
        }

        public bool TakeDamage(DamageInfo info)
        {
            if (IsDead)
                return false;

            CurrentHealth = Mathf.Max(0, CurrentHealth - info.Amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            Debug.Log($"{name} HP: {CurrentHealth}/{maxHealth}");

            if (spriteRenderer != null)
            {
                spriteRenderer.color = hitFlashColor;
                flashTimer = hitFlashDuration;
            }

            if (CurrentHealth == 0)
            {
                HandleDeath();
            }
            else if (animator != null)
            {
                animator.SetTrigger(HurtParameter);
            }

            return true;
        }

        private void HandleDeath()
        {
            Died?.Invoke();

            if (animator != null)
                animator.SetTrigger(DieParameter);

            if (TryGetComponent(out GolemAI ai))
                ai.enabled = false;

            if (TryGetComponent(out Rigidbody2D body))
            {
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }

            foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
                col.enabled = false;

            Destroy(gameObject, deathDestroyDelay);
        }
    }
}