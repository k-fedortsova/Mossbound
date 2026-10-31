using UnityEngine;

namespace Mossbound.Level
{
    public class WaterSurface : MonoBehaviour
    {
        [Header("Bob (up / down)")]
        [SerializeField, Min(0f)] private float bobPixels = 2f;
        [SerializeField, Min(0.1f)] private float bobPeriod = 2f;

        [Header("Drift (sideways)")]
        [Tooltip("Keep at 3 px or less: the water tucks only 3 px under the platform edges.")]
        [SerializeField, Min(0f)] private float driftPixels = 3f;
        [SerializeField, Min(0.1f)] private float driftPeriod = 3.5f;

        [Tooltip("Movement snaps to whole pixels so the pixel art stays crisp.")]
        [SerializeField, Min(1f)] private float pixelsPerUnit = 16f;

        private Vector3 startPosition;
        private float phase;

        private void Awake()
        {
            startPosition = transform.localPosition;
            phase = Random.value * Mathf.PI * 2f;
        }

        private void Update()
        {
            float time = Time.time * Mathf.PI * 2f;
            float y = Mathf.Sin(time / bobPeriod + phase) * bobPixels;
            float x = Mathf.Sin(time / driftPeriod + phase) * driftPixels;
            transform.localPosition = startPosition + new Vector3(Snap(x), Snap(y), 0f);
        }

        private float Snap(float pixels) => Mathf.Round(pixels) / pixelsPerUnit;
    }
}
