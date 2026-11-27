using UnityEngine;

namespace Mossbound.Menu
{
    // Gentle sine motion around the start position: floating ducks, swaying title, hovering clouds.
    // Works for world objects and for UI (RectTransform anchored position).
    public class Bobber : MonoBehaviour
    {
        [SerializeField] private Vector2 amplitude = new Vector2(0f, 0.08f);
        [SerializeField, Min(0f)] private float speed = 1.5f;
        [Tooltip("Rotation swing in degrees.")]
        [SerializeField] private float rotation;
        [SerializeField] private bool randomPhase = true;

        private RectTransform rectTransform;
        private Vector3 origin;
        private float phase;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            origin = rectTransform != null ? (Vector3)rectTransform.anchoredPosition : transform.localPosition;
            phase = randomPhase ? Random.Range(0f, Mathf.PI * 2f) : 0f;
        }

        private void Update()
        {
            float t = Time.unscaledTime * speed + phase;
            var offset = new Vector3(Mathf.Sin(t * 0.5f) * amplitude.x, Mathf.Sin(t) * amplitude.y, 0f);
            if (rectTransform != null)
                rectTransform.anchoredPosition = origin + offset;
            else
                transform.localPosition = origin + offset;

            if (rotation != 0f)
                transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.8f) * rotation);
        }
    }
}
