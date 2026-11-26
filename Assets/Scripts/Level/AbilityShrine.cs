using Mossbound.Gameplay;
using Mossbound.Player;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mossbound.Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class AbilityShrine : MonoBehaviour
    {
        [SerializeField] private PlayerAbility ability;
        [SerializeField] private string title;
        [SerializeField, TextArea] private string hint;

        [Header("Visuals")]
        [SerializeField] private Transform visual;
        [SerializeField] private Light2D glow;
        [SerializeField, Min(0f)] private float bobHeight = 0.12f;
        [SerializeField, Min(0f)] private float bobSpeed = 2f;
        [SerializeField, Min(0f)] private float usedGlowIntensity = 0.25f;

        private Vector3 visualStartPosition;
        private bool isUsed;

        private void Reset()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void Awake()
        {
            visualStartPosition = visual.localPosition;
        }

        private void Update()
        {
            if (!isUsed)
                visual.localPosition = visualStartPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isUsed)
                return;

            var abilities = other.GetComponentInParent<PlayerAbilities>();
            if (abilities == null)
                return;

            isUsed = true;
            abilities.Unlock(ability);
            visual.localPosition = visualStartPosition;
            glow.intensity = usedGlowIntensity;
            GameSession.Instance.ShowMessage(title, hint);
        }
    }
}
