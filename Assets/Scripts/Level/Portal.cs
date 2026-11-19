using UnityEngine;
using Mossbound.Player;

namespace Mossbound.Level
{
    [RequireComponent(typeof(Collider2D))]
    public class Portal : MonoBehaviour
    {
        [Header("Portal")]
        [SerializeField] private GameObject portalVisual;

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

            Require.Assigned(destination, nameof(destination), this);

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

            lastTeleportTime = Time.time;
            player.TeleportTo(destination.position);
        }
    }
}