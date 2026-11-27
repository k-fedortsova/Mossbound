using System.Collections;
using Mossbound.Player;
using UnityEngine;

namespace Mossbound.Companion
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class CompanionInteractable : MonoBehaviour
    {
        [Header("Companion Anchor")]
        [SerializeField] protected Transform dragonAnchor;

        [Header("UI Prompt")]
        [SerializeField] protected string promptText = "Press E to interact";

        public Transform DragonAnchor => dragonAnchor != null ? dragonAnchor : transform;
        public virtual string PromptText => promptText;
        public virtual bool CanInteract => isActiveAndEnabled;

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && CompanionController.Instance != null)
            {
                CompanionController.Instance.RegisterInteractable(this);
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") && CompanionController.Instance != null)
            {
                CompanionController.Instance.UnregisterInteractable(this);
            }
        }

        protected virtual void OnDisable()
        {
            if (CompanionController.Instance != null)
            {
                CompanionController.Instance.UnregisterInteractable(this);
            }
        }

        public abstract IEnumerator Interact(PlayerHealth playerHealth);
    }
}