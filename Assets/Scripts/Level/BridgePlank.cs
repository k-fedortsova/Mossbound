using UnityEngine;

namespace Mossbound.Level
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class BridgePlank : MonoBehaviour
    {
        private const float ShakePixels = 1.5f;
        private const float PixelsPerUnit = 16f;
        private const float FallGravity = 30f;
        private const float FallFadeTime = 0.5f;
        private const float RegrowFadeTime = 0.3f;
        private const float SwingToArt = 0.5f;

        private enum Phase
        {
            Intact,
            Shaking,
            Falling,
            Regrowing,
        }

        [Header("Swing")]
        [Tooltip("Keep the vine tops above the view, and set the same Y on the vines' local position.")]
        [SerializeField, Min(1f)] private float vineLength = 10f;
        [Tooltip("Swing to each side, in degrees.")]
        [SerializeField] private float amplitude = 10f;
        [SerializeField, Min(0.1f)] private float period = 3f;
        [SerializeField, Range(0f, 1f)] private float swingPhase;

        [Header("Collapse")]
        [SerializeField] private bool collapses;
        [SerializeField, Min(0f)] private float collapseDelay = 0.55f;
        [SerializeField, Min(0f)] private float regrowDelay = 2.5f;

        [Header("Parts")]
        [SerializeField] private SpriteRenderer art;
        [SerializeField] private SpriteRenderer strand;
        [SerializeField] private SpriteRenderer[] vines;

        private Rigidbody2D body;
        private BoxCollider2D box;
        private Vector2 anchor;
        private Vector2[] vineOffsets;
        private float restTilt;
        private float swingAngle;
        private Phase phase;
        private float phaseTimer;
        private float fallSpeed;

        public Vector2 Delta { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            box = GetComponent<BoxCollider2D>();
            anchor = (Vector2)transform.position + Vector2.up * vineLength;
            restTilt = art.transform.localEulerAngles.z;

            vineOffsets = new Vector2[vines.Length];
            for (int i = 0; i < vines.Length; i++)
                vineOffsets[i] = new Vector2(vines[i].transform.localPosition.x, 0f);
        }

        public void NotifyStanding()
        {
            if (!collapses || phase != Phase.Intact)
                return;

            EnterPhase(Phase.Shaking);
        }

        private void FixedUpdate()
        {
            swingAngle = GetAngle(Time.fixedTime);
            Vector2 next = GetPosition(swingAngle);
            Delta = next - body.position;
            body.MovePosition(next);
        }

        private void Update()
        {
            phaseTimer += Time.deltaTime;
            switch (phase)
            {
                case Phase.Shaking:
                    UpdateShaking();
                    break;
                case Phase.Falling:
                    UpdateFalling();
                    break;
                case Phase.Regrowing:
                    UpdateRegrowing();
                    break;
            }
        }

        private void LateUpdate()
        {
            float tilt = restTilt + swingAngle * Mathf.Rad2Deg * SwingToArt;
            art.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);

            for (int i = 0; i < vines.Length; i++)
                UpdateVine(vines[i], vineOffsets[i]);
        }

        private void UpdateVine(SpriteRenderer vine, Vector2 offset)
        {
            Vector2 top = anchor + offset;
            Vector2 span = (Vector2)transform.position + offset - top;

            float angle = Mathf.Atan2(span.x, -span.y) * Mathf.Rad2Deg;
            vine.transform.SetPositionAndRotation(top, Quaternion.Euler(0f, 0f, angle));
            vine.size = new Vector2(vine.size.x, span.magnitude);
        }

        private void UpdateShaking()
        {
            float pixels = ShakePixels / PixelsPerUnit;
            art.transform.localPosition = new Vector3(Random.Range(-pixels, pixels), Random.Range(-pixels, pixels));
            if (phaseTimer < collapseDelay)
                return;

            art.transform.localPosition = Vector3.zero;
            box.enabled = false;
            fallSpeed = 0f;
            EnterPhase(Phase.Falling);
        }

        private void UpdateFalling()
        {
            fallSpeed += FallGravity * Time.deltaTime;
            art.transform.localPosition += Vector3.down * (fallSpeed * Time.deltaTime);
            SetAlpha(1f - Mathf.Clamp01(phaseTimer / FallFadeTime));

            if (phaseTimer < regrowDelay)
                return;

            art.transform.localPosition = Vector3.zero;
            box.enabled = true;
            EnterPhase(Phase.Regrowing);
        }

        private void UpdateRegrowing()
        {
            SetAlpha(Mathf.Clamp01(phaseTimer / RegrowFadeTime));
            if (phaseTimer >= RegrowFadeTime)
                EnterPhase(Phase.Intact);
        }

        private void EnterPhase(Phase next)
        {
            phase = next;
            phaseTimer = 0f;
        }

        private void SetAlpha(float alpha)
        {
            Color color = art.color;
            color.a = alpha;
            art.color = color;
            strand.color = color;
        }

        private float GetAngle(float time)
        {
            float wave = Mathf.Sin(2f * Mathf.PI * (time / period + swingPhase));
            return amplitude * Mathf.Deg2Rad * wave;
        }

        private Vector2 GetPosition(float angle)
        {
            return anchor + new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle)) * vineLength;
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 top = Application.isPlaying ? anchor : (Vector2)transform.position + Vector2.up * vineLength;
            float angle = amplitude * Mathf.Deg2Rad;
            Vector2 side = new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle)) * vineLength;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(top, top + side);
            Gizmos.DrawLine(top, top + new Vector2(-side.x, side.y));
        }
    }
}
