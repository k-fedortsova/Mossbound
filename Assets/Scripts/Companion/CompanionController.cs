using System.Collections;
using System.Collections.Generic;
using Mossbound.CompanionPrototype;
using Mossbound.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Mossbound.Companion
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CompanionFollow2D))]
    public class CompanionController : MonoBehaviour
    {
        public static CompanionController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Animator companionAnimator;

        [Header("UI Prompt")]
        [SerializeField] private GameObject interactPromptObject;
        [SerializeField] private Text interactPromptText;

        [Header("Animation States")]
        [SerializeField] private string idleState = "idle";
        [SerializeField] private string interactState = "attack";
        [SerializeField] private string hitState = "hit";
        [SerializeField] private string deathState = "cry";
        [SerializeField, Min(0.05f)] private float hitAnimDuration = 0.35f;

        [Header("Hit Knockback")]
        [SerializeField] private Vector2 hitKnockbackOffset = new Vector2(0.75f, 0.25f);
        [SerializeField, Min(0.05f)] private float knockbackDuration = 0.2f;

        private CompanionFollow2D follow;
        private Vector2 defaultFollowOffset;
        private readonly List<CompanionInteractable> nearbyInteractables = new List<CompanionInteractable>();
        private CompanionInteractable currentTarget;
        private bool isInteracting;
        private bool isDead;
        private Coroutine hitCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            follow = GetComponent<CompanionFollow2D>();
            defaultFollowOffset = follow.FollowOffset;

            if (playerTransform == null && follow.Target != null)
                playerTransform = follow.Target;

            if (playerHealth == null && playerTransform != null)
                playerHealth = playerTransform.GetComponent<PlayerHealth>();

            if (interactPromptObject != null)
                interactPromptObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.Damaged += OnPlayerDamaged;
                playerHealth.Died += OnPlayerDied;
                playerHealth.HealthChanged += OnPlayerHealthChanged;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.Damaged -= OnPlayerDamaged;
                playerHealth.Died -= OnPlayerDied;
                playerHealth.HealthChanged -= OnPlayerHealthChanged;
            }
        }

        private void Update()
        {
            if (isDead || isInteracting) return;

            UpdateCurrentTarget();
            UpdatePrompt();

            if (currentTarget != null && currentTarget.CanInteract && Keyboard.current != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    StartCoroutine(InteractRoutine(currentTarget));
                }
            }
        }

        private void UpdatePrompt()
        {
            if (interactPromptObject == null) return;

            bool show = !isDead && !isInteracting && currentTarget != null && currentTarget.CanInteract;
            interactPromptObject.SetActive(show);

            if (show && interactPromptText != null)
                interactPromptText.text = currentTarget.PromptText;
        }

        private void OnPlayerDamaged()
        {
            if (isDead || isInteracting) return;

            if (hitCoroutine != null)
                StopCoroutine(hitCoroutine);

            hitCoroutine = StartCoroutine(HitRoutine());
        }

        private IEnumerator HitRoutine()
        {
            PlayState(hitState);

            if (currentTarget == null && playerTransform != null)
            {
                float dir = transform.position.x < playerTransform.position.x ? -1f : 1f;
                follow.FollowOffset = defaultFollowOffset + new Vector2(dir * hitKnockbackOffset.x, hitKnockbackOffset.y);
            }

            float waitBack = Mathf.Min(knockbackDuration, hitAnimDuration);
            yield return new WaitForSeconds(waitBack);

            if (!isDead && currentTarget == null)
                follow.FollowOffset = defaultFollowOffset;

            float remainingAnim = hitAnimDuration - waitBack;
            if (remainingAnim > 0f)
                yield return new WaitForSeconds(remainingAnim);

            if (!isDead && !isInteracting)
                PlayState(idleState);

            hitCoroutine = null;
        }

        private void OnPlayerDied()
        {
            isDead = true;
            StopAllCoroutines();
            hitCoroutine = null;
            isInteracting = false;
            currentTarget = null;
            ReturnToPlayer();
            UpdatePrompt();
            PlayState(deathState);
        }

        private void OnPlayerHealthChanged(int current, int max)
        {
            if (isDead && current > 0)
            {
                isDead = false;
                PlayState(idleState);
                UpdatePrompt();
            }
        }

        public void RegisterInteractable(CompanionInteractable interactable)
        {
            if (interactable != null && !nearbyInteractables.Contains(interactable))
            {
                nearbyInteractables.Add(interactable);
            }
        }

        public void UnregisterInteractable(CompanionInteractable interactable)
        {
            nearbyInteractables.Remove(interactable);
        }

        private void UpdateCurrentTarget()
        {
            nearbyInteractables.RemoveAll(item => item == null || !item.CanInteract);

            if (nearbyInteractables.Count == 0)
            {
                if (currentTarget != null)
                {
                    currentTarget = null;
                    ReturnToPlayer();
                }
                return;
            }

            CompanionInteractable closest = null;
            float minDist = float.MaxValue;

            foreach (var item in nearbyInteractables)
            {
                float dist = Vector2.Distance(playerTransform.position, item.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = item;
                }
            }

            if (closest != currentTarget)
            {
                currentTarget = closest;
                MoveToTarget(currentTarget);
            }
        }

        private void MoveToTarget(CompanionInteractable target)
        {
            follow.Target = target.DragonAnchor;
            follow.FollowOffset = Vector2.zero;
        }

        private void ReturnToPlayer()
        {
            follow.Target = playerTransform;
            follow.FollowOffset = defaultFollowOffset;
        }

        private IEnumerator InteractRoutine(CompanionInteractable target)
        {
            isInteracting = true;
            UpdatePrompt();
            PlayState(interactState);

            yield return StartCoroutine(target.Interact(playerHealth));

            isInteracting = false;

            if (!isDead)
            {
                PlayState(idleState);

                if (target == null || !target.CanInteract || !nearbyInteractables.Contains(target))
                {
                    currentTarget = null;
                    ReturnToPlayer();
                }

                UpdatePrompt();
            }
        }

        private void PlayState(string stateName)
        {
            if (companionAnimator != null && !string.IsNullOrWhiteSpace(stateName))
            {
                companionAnimator.Play(stateName, 0, 0f);
            }
        }
    }
}