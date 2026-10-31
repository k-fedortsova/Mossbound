using UnityEngine;
using Mossbound.Player;

namespace Mossbound.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class Portal : MonoBehaviour
    {
        [Header("Portal")]
        [SerializeField] private GameObject portalVisual;
        [SerializeField] private Animator portalAnimator;

        [Header("Teleport")]
        [SerializeField] private Transform destination;
        [SerializeField, Min(0f)] private float cooldown = 0.75f;

        private Collider2D portalCollider;
        private bool isActive;
        private float lastTeleportTime = -Mathf.Infinity;

        private void Awake()
        {
            portalCollider = GetComponent<Collider2D>();
            portalCollider.isTrigger = true;

            if (portalVisual == null)
                portalVisual = gameObject;

            if (portalAnimator == null)
                portalAnimator = portalVisual.GetComponent<Animator>();

            SetPortalActive(false);
        }

        public void EnablePortal()
        {
            SetPortalActive(true);
        }

        public void SetPortalActive(bool active)
        {
            isActive = active;

            if (portalVisual != null)
                portalVisual.SetActive(active);

            if (portalCollider != null)
                portalCollider.enabled = active;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActive || Time.time < lastTeleportTime + cooldown)
                return;

            PlayerController player = other.GetComponentInParent<PlayerController>();

            if (player == null)
                return;

            if (destination == null)
            {
                Debug.LogWarning(
                    "Portal destination is not assigned.",
                    this
                );
                return;
            }

            lastTeleportTime = Time.time;

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();

            if (body != null)
                body.linearVelocity = Vector2.zero;

            player.transform.position = destination.position;
        }
    }
}