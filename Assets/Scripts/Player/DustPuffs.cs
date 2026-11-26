using System.Collections.Generic;
using UnityEngine;

namespace Mossbound.Player
{
    public class DustPuffs
    {
        private const float PixelsPerUnit = 64f;
        private const float Drag = 6f;

        private readonly List<(SpriteRenderer renderer, Vector2 velocity, float size, float age)> puffs =
            new List<(SpriteRenderer, Vector2, float, float)>();
        private readonly Stack<SpriteRenderer> pool = new Stack<SpriteRenderer>();
        private readonly SpriteRenderer source;
        private readonly Color color;
        private readonly float lifetime;
        private readonly float speed;
        private readonly Sprite sprite;
        private readonly Transform root;

        public DustPuffs(SpriteRenderer source, Color color, float lifetime, float speed)
        {
            this.source = source;
            this.color = color;
            this.lifetime = lifetime;
            this.speed = speed;

            Texture2D texture = Texture2D.whiteTexture;
            var rect = new Rect(0f, 0f, texture.width, texture.height);
            sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
            root = new GameObject("Dust Puffs").transform;
        }

        public void Emit(Vector3 position, int count)
        {
            for (int i = 0; i < count; i++)
            {
                SpriteRenderer puff = pool.Count > 0 ? pool.Pop() : Create();
                puff.gameObject.SetActive(true);
                puff.transform.position = position;
                puff.color = color;

                Vector2 velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(0.1f, 0.5f)) * speed;
                puffs.Add((puff, velocity, Random.Range(1f, 2f), 0f));
            }
        }

        public void Update(float deltaTime)
        {
            float drag = Mathf.Exp(-Drag * deltaTime);
            for (int i = puffs.Count - 1; i >= 0; i--)
            {
                var (puff, velocity, size, age) = puffs[i];
                age += deltaTime;
                if (age >= lifetime)
                {
                    puff.gameObject.SetActive(false);
                    pool.Push(puff);
                    puffs.RemoveAt(i);
                    continue;
                }

                float remaining = 1f - age / lifetime;
                puff.transform.position += (Vector3)(velocity * deltaTime);
                puff.transform.localScale = Vector3.one * (size * remaining);
                Color faded = color;
                faded.a *= remaining;
                puff.color = faded;
                puffs[i] = (puff, velocity * drag, size, age);
            }
        }

        public void Dispose()
        {
            if (root != null)
                Object.Destroy(root.gameObject);
            Object.Destroy(sprite);
        }

        private SpriteRenderer Create()
        {
            var puff = new GameObject("Dust").AddComponent<SpriteRenderer>();
            puff.transform.SetParent(root, false);
            puff.sprite = sprite;
            puff.sharedMaterial = source.sharedMaterial;
            puff.sortingLayerID = source.sortingLayerID;
            puff.sortingOrder = source.sortingOrder + 1;
            return puff;
        }
    }
}
