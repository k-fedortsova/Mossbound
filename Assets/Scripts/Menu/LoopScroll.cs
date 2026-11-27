using UnityEngine;

namespace Mossbound.Menu
{
    // Slides a tiled background sideways forever (the sky of the menu); it jumps back by one tile
    // width once it has moved that far, which is invisible on a tiled sprite.
    public class LoopScroll : MonoBehaviour
    {
        [SerializeField] private float speed = -0.4f;
        [Tooltip("Width of one tile of the sprite, in world units.")]
        [SerializeField, Min(0.01f)] private float wrapWidth = 100f;

        private Vector3 origin;
        private float offset;

        private void Awake()
        {
            origin = transform.localPosition;
        }

        private void Update()
        {
            offset = Mathf.Repeat(offset + speed * Time.deltaTime, wrapWidth);
            transform.localPosition = origin + new Vector3(offset - wrapWidth * 0.5f, 0f, 0f);
        }
    }
}
