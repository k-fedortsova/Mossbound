using UnityEngine;
using UnityEngine.InputSystem;

namespace Mossbound.Menu
{
    // Slow "breathing" pan of the menu camera plus a small parallax toward the mouse,
    // so the island never looks like a still picture.
    public class MenuCameraDrift : MonoBehaviour
    {
        [SerializeField] private Vector2 driftAmplitude = new Vector2(0.35f, 0.2f);
        [SerializeField, Min(0f)] private float driftSpeed = 0.12f;
        [SerializeField] private Vector2 mouseInfluence = new Vector2(0.25f, 0.15f);
        [SerializeField, Min(0.01f)] private float smoothing = 2.5f;

        private Vector3 origin;
        private Vector2 mouseOffset;

        private void Awake()
        {
            origin = transform.position;
        }

        private void LateUpdate()
        {
            float t = Time.unscaledTime * driftSpeed * Mathf.PI * 2f;
            var drift = new Vector2(Mathf.Sin(t) * driftAmplitude.x, Mathf.Sin(t * 1.7f) * driftAmplitude.y);

            Vector2 target = Vector2.zero;
            Mouse mouse = Mouse.current;
            if (mouse != null && Screen.width > 0 && Screen.height > 0)
            {
                Vector2 pointer = mouse.position.ReadValue();
                var normalized = new Vector2(pointer.x / Screen.width - 0.5f, pointer.y / Screen.height - 0.5f) * 2f;
                target = Vector2.Scale(Vector2.ClampMagnitude(normalized, 1.5f), mouseInfluence);
            }
            mouseOffset = Vector2.Lerp(mouseOffset, target, 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime));

            transform.position = origin + (Vector3)(drift + mouseOffset);
        }
    }
}
