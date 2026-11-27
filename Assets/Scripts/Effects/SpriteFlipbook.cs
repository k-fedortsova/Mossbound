using UnityEngine;

namespace Mossbound.Effects
{
    /// <summary>
    /// Loops a few sprite frames (torch and candle flames) without an Animator.
    /// Each copy starts on a random frame, so a row of torches does not flicker in sync.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFlipbook : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField, Min(0.1f)] private float framesPerSecond = 12f;

        private SpriteRenderer spriteRenderer;
        private float timeOffset;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            Require.NotEmpty(frames, nameof(frames), this);
            timeOffset = Random.value * 10f;
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0)
                return;

            int frame = (int)((Time.time + timeOffset) * framesPerSecond) % frames.Length;
            spriteRenderer.sprite = frames[frame];
        }
    }
}
