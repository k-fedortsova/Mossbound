using System.Collections;
using Mossbound.Player;
using UnityEngine;

namespace Mossbound.Companion
{
    public class BreakableBlock : CompanionInteractable
    {
        [Header("Block Sprites")]
        [SerializeField] private Sprite[] damageSprites;
        [SerializeField] private Sprite debrisSprite;
        [SerializeField, Min(1)] private int debrisCountPerBlock = 6;

        [Header("Juice & Timings")]
        [SerializeField] private float hitDuration = 0.25f;

        private SpriteRenderer[] blockRenderers;
        private int currentHits = 0;
        private const int MaxHits = 3;

        public override bool CanInteract => base.CanInteract && currentHits < MaxHits;

        private void Awake()
        {
            blockRenderers = GetComponentsInChildren<SpriteRenderer>();
            UpdateSprites(0);
        }

        public override IEnumerator Interact(PlayerHealth playerHealth)
        {
            yield return new WaitForSeconds(hitDuration);

            currentHits++;

            if (currentHits >= MaxHits)
            {
                SpawnDebris();
                Destroy(gameObject);
            }
            else
            {
                UpdateSprites(currentHits);
                StartCoroutine(ShakeRoutine(0.12f, 0.08f));
            }
        }

        private void SpawnDebris()
        {
            if (debrisSprite == null) return;

            foreach (SpriteRenderer sr in blockRenderers)
            {
                if (sr != null)
                {
                    DebrisPiece.SpawnBurst(
                        sr.transform.position,
                        debrisSprite,
                        debrisCountPerBlock,
                        sr.sortingLayerID,
                        sr.sortingOrder);
                }
            }
        }

        private void UpdateSprites(int spriteIndex)
        {
            if (damageSprites == null || spriteIndex >= damageSprites.Length) return;

            foreach (SpriteRenderer sr in blockRenderers)
            {
                if (sr != null)
                    sr.sprite = damageSprites[spriteIndex];
            }
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            Vector3 originalPos = transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                transform.localPosition = originalPos + new Vector3(x, y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalPos;
        }
    }
}