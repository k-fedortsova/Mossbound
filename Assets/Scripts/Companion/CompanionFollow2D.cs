using UnityEngine;

namespace Mossbound.CompanionPrototype
{
    /// <summary>Simple flying follow behaviour for the isolated companion prototype.</summary>
    [DisallowMultipleComponent]
    public sealed class CompanionFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 followOffset = new Vector2(-0.8f, 0.9f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.18f;
        [SerializeField, Min(0f)] private float hoverAmplitude = 0.10f;
        [SerializeField, Min(0f)] private float hoverFrequency = 2.4f;
        [SerializeField] private bool flipToFaceTarget = true;

        private Vector3 velocity;
        private SpriteRenderer[] renderers;

        public Transform Target { get => target; set => target = value; }
        public Vector2 FollowOffset { get => followOffset; set => followOffset = value; }

        private void Awake()
        {
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            float bob = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
            Vector3 destination = target.position + new Vector3(followOffset.x, followOffset.y + bob, 0f);
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, smoothTime);

            if (flipToFaceTarget)
            {
                float dx = target.position.x - transform.position.x;
                if (Mathf.Abs(dx) > 0.05f)
                {
                    foreach (SpriteRenderer sr in renderers)
                        if (sr != null) sr.flipX = dx < 0f;
                }
            }
        }

        public void SnapToFollowPosition()
        {
            if (target == null) return;
            transform.position = target.position + new Vector3(followOffset.x, followOffset.y, 0f);
            velocity = Vector3.zero;
        }
    }
}
