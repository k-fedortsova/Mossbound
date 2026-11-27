using UnityEngine;

namespace Mossbound.Companion
{
    public class DebrisPiece : MonoBehaviour
    {
        private Vector2 velocity;
        private float angularVelocity;
        private float gravity = 16f;
        private float lifetime;
        private float elapsed;
        private SpriteRenderer sr;

        public static void SpawnBurst(
            Vector3 origin,
            Sprite sprite,
            int count,
            int sortingLayerID,
            int sortingOrder)
        {
            if (sprite == null || count <= 0) return;

            for (int i = 0; i < count; i++)
            {
                GameObject obj = new GameObject("DebrisPiece");
                obj.transform.position = origin + new Vector3(
                    Random.Range(-0.2f, 0.2f),
                    Random.Range(-0.2f, 0.2f),
                    0f);

                float scale = Random.Range(0.7f, 1.15f);
                obj.transform.localScale = new Vector3(scale, scale, 1f);

                SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingLayerID = sortingLayerID;
                renderer.sortingOrder = sortingOrder + 1;

                DebrisPiece piece = obj.AddComponent<DebrisPiece>();
                piece.sr = renderer;
                piece.velocity = new Vector2(Random.Range(-3.5f, 3.5f), Random.Range(3.5f, 6.5f));
                piece.angularVelocity = Random.Range(-420f, 420f);
                piece.lifetime = Random.Range(0.45f, 0.75f);
            }
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            if (elapsed >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            velocity.y -= gravity * Time.deltaTime;
            transform.position += (Vector3)(velocity * Time.deltaTime);
            transform.Rotate(0f, 0f, angularVelocity * Time.deltaTime);

            float fadeStart = lifetime * 0.5f;
            if (elapsed > fadeStart && sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, (elapsed - fadeStart) / (lifetime - fadeStart));
                sr.color = c;
            }
        }
    }
}