using System.Collections;
using Mossbound.Player;
using UnityEngine;

namespace Mossbound.Companion
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovableObstacle : CompanionInteractable
    {
        [Header("Movement Settings")]
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField, Min(0.1f)] private float moveSpeed = 3.5f;

        private Rigidbody2D rb;
        private Vector2 posA;
        private Vector2 posB;
        private bool isAtB = false;
        private bool isMoving = false;

        public override bool CanInteract => base.CanInteract && !isMoving;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            posA = pointA != null ? (Vector2)pointA.position : rb.position;
            posB = pointB != null ? (Vector2)pointB.position : rb.position + Vector2.up * 3f;
        }

        public override IEnumerator Interact(PlayerHealth playerHealth)
        {
            isMoving = true;
            Vector2 destination = isAtB ? posA : posB;

            while (Vector2.Distance(rb.position, destination) > 0.01f)
            {
                Vector2 nextPos = Vector2.MoveTowards(rb.position, destination, moveSpeed * Time.fixedDeltaTime);
                rb.MovePosition(nextPos);
                yield return new WaitForFixedUpdate();
            }

            rb.MovePosition(destination);
            isAtB = !isAtB;
            isMoving = false;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 a = Application.isPlaying ? (Vector3)posA : (pointA != null ? pointA.position : transform.position);
            Vector3 b = Application.isPlaying ? (Vector3)posB : (pointB != null ? pointB.position : transform.position + Vector3.up * 3f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawWireSphere(a, 0.15f);
            Gizmos.DrawWireSphere(b, 0.15f);
        }
    }
}