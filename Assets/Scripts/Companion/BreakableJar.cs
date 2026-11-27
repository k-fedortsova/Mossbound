using System.Collections;
using Mossbound.Player;
using UnityEngine;

namespace Mossbound.Companion
{
    public class BreakableJar : CompanionInteractable
    {
        [Header("Jar Sprites")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite crackedSprite;
        [SerializeField] private Sprite debrisSprite;
        [SerializeField, Min(1)] private int debrisCount = 6;

        [Header("Juice & Timings")]
        [SerializeField] private float hitDuration = 0.25f;

        [SerializeField] private GameObject lootPrefab;

        private int currentHits = 0;
        private const int MaxHits = 2;

        public override bool CanInteract => base.CanInteract && currentHits < MaxHits;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public override IEnumerator Interact(PlayerHealth playerHealth)
        {
            yield return new WaitForSeconds(hitDuration);

            currentHits++;

            if (currentHits == 1)
            {
                if (crackedSprite != null && spriteRenderer != null)
                    spriteRenderer.sprite = crackedSprite;

                StartCoroutine(ShakeRoutine(0.12f, 0.08f));
            }
            else if (currentHits >= MaxHits)
            {
                SpawnDebris();

                if (lootPrefab != null)
                    Instantiate(lootPrefab, transform.position, Quaternion.identity);

                Destroy(gameObject);
            }
        }

        private void SpawnDebris()
        {
            if (debrisSprite == null || spriteRenderer == null) return;

            DebrisPiece.SpawnBurst(
                transform.position,
                debrisSprite,
                debrisCount,
                spriteRenderer.sortingLayerID,
                spriteRenderer.sortingOrder);
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