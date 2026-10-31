using UnityEngine;

namespace Mossbound.Player
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerCollisionSensor : MonoBehaviour
    {
        [SerializeField] private PlayerMovementSettings settings;
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private LayerMask wallLayers;
        [SerializeField, Min(0.01f)] private float groundCheckDistance = 0.05f;
        [Tooltip("Fraction of the body width that must stand on ground. Kept narrow so the player drops once their centre passes the edge.")]
        [SerializeField, Range(0.1f, 1f)] private float groundCheckWidth = 0.25f;
        [SerializeField, Range(0.1f, 1f)] private float wallCheckHeight = 0.5f;
        [SerializeField, Range(0f, 1f)] private float minSurfaceNormal = 0.9f;

        private Collider2D bodyCollider;

        public bool IsGrounded { get; private set; }
        public int WallSide { get; private set; }
        public Collider2D GroundCollider { get; private set; }

        private void Awake()
        {
            bodyCollider = GetComponent<Collider2D>();
        }

        public void Refresh()
        {
            Bounds bounds = bodyCollider.bounds;
            IsGrounded = CheckGround(bounds);
            WallSide = CheckWall(bounds, 1) ? 1 : CheckWall(bounds, -1) ? -1 : 0;
        }

        private bool CheckGround(Bounds bounds)
        {
            Vector2 size = new Vector2(bounds.size.x * groundCheckWidth, 0.05f);
            Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + size.y * 0.5f);
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, groundCheckDistance, groundLayers);
            bool grounded = hit.collider != null && hit.normal.y >= minSurfaceNormal;
            GroundCollider = grounded ? hit.collider : null;
            return grounded;
        }

        private bool CheckWall(Bounds bounds, int direction)
        {
            Vector2 size = new Vector2(0.05f, bounds.size.y * wallCheckHeight);
            Vector2 origin = new Vector2(bounds.center.x + direction * (bounds.extents.x - size.x * 0.5f), bounds.center.y);
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, new Vector2(direction, 0f), settings.wallCheckDistance, wallLayers);
            return hit.collider != null && Mathf.Abs(hit.normal.x) >= minSurfaceNormal;
        }

        private void OnDrawGizmosSelected()
        {
            Collider2D target = bodyCollider != null ? bodyCollider : GetComponent<Collider2D>();
            if (target == null || settings == null)
                return;

            Bounds bounds = target.bounds;
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(new Vector3(bounds.center.x, bounds.min.y - groundCheckDistance * 0.5f),
                new Vector3(bounds.size.x * groundCheckWidth, groundCheckDistance));

            Gizmos.color = WallSide != 0 ? Color.cyan : Color.gray;
            Vector3 wallSize = new Vector3(settings.wallCheckDistance, bounds.size.y * wallCheckHeight);
            Gizmos.DrawWireCube(new Vector3(bounds.max.x + settings.wallCheckDistance * 0.5f, bounds.center.y), wallSize);
            Gizmos.DrawWireCube(new Vector3(bounds.min.x - settings.wallCheckDistance * 0.5f, bounds.center.y), wallSize);
        }
    }
}
