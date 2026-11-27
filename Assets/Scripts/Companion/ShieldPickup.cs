using Mossbound.Player;
using UnityEngine;

namespace Mossbound.Companion
{
    [RequireComponent(typeof(Collider2D))]
    public class ShieldPickup : MonoBehaviour
    {
        [Header("Hover Animation")]
        [SerializeField] private float hoverAmplitude = 0.1f;
        [SerializeField] private float hoverSpeed = 3f;

        private Vector3 startPos;

        private void Start()
        {
            startPos = transform.position;
        }

        private void Update()
        {
            float yOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
            transform.position = startPos + new Vector3(0f, yOffset, 0f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ResetHealth();
                Destroy(gameObject);
            }
        }
    }
}