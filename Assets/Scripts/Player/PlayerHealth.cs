using System;
using Mossbound.Combat;
using UnityEngine;

namespace Mossbound.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int maxHealth = 5;
        [SerializeField, Min(0f)] private float invulnerabilityDuration = 1f;

        private PlayerController controller;
        private PlayerGuard guard;
        private float invulnerabilityTimer;

        public event Action<int, int> HealthChanged;
        public event Action Died;
        public event Action Damaged;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;
        public bool IsInvulnerable => invulnerabilityTimer > 0f;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            guard = GetComponent<PlayerGuard>();
            CurrentHealth = maxHealth;
        }

        private void OnEnable()
        {
            controller.Teleported += OnTeleported;
        }

        private void OnDisable()
        {
            controller.Teleported -= OnTeleported;
        }

        private void Update()
        {
            if (invulnerabilityTimer > 0f)
                invulnerabilityTimer -= Time.deltaTime;
        }

        public bool TakeDamage(DamageInfo info)
        {
            if (IsDead || invulnerabilityTimer > 0f)
                return false;

            if (guard != null && guard.CanBlock(info.SourcePosition))
            {
                Debug.Log("Blocked!");
                return false;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - info.Amount);
            invulnerabilityTimer = invulnerabilityDuration;
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
            
            if (CurrentHealth == 0)
                Died?.Invoke();
            else
                Damaged?.Invoke();

            Debug.Log($"Player HP: {CurrentHealth}/{maxHealth}");
            return true;
        }

        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
            invulnerabilityTimer = 0f;
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void OnTeleported()
        {
            ResetHealth();
        }
    }
}