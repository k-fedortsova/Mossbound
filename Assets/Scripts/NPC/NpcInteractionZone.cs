using System;
using Mossbound.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mossbound.Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class NpcInteractionZone : MonoBehaviour
    {
        [SerializeField] private InputActionReference interactAction;
        [Tooltip("\"Press E\" hint shown while the player is in range and the NPC can talk.")]
        [SerializeField] private GameObject prompt;

        private PlayerController playerInRange;
        private bool interactable = true;

        public event Action<PlayerController> PlayerEntered;
        public event Action PlayerExited;
        public event Action<PlayerController> Interacted;

        public bool IsPlayerInRange => playerInRange != null;

        public bool Interactable
        {
            get => interactable;
            set
            {
                interactable = value;
                RefreshPrompt();
            }
        }

        private void Reset()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void Awake()
        {
            if (interactAction == null)
                Debug.LogError($"{nameof(NpcInteractionZone)} on '{name}' has no interact action.", this);

            RefreshPrompt();
        }

        private void OnEnable()
        {
            if (interactAction != null)
                interactAction.action.Enable();
        }

        private void OnDisable()
        {
            playerInRange = null;
            RefreshPrompt();
        }

        private void Update()
        {
            if (!interactable || playerInRange == null || interactAction == null)
                return;

            if (interactAction.action.WasPressedThisFrame())
                Interacted?.Invoke(playerInRange);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (playerInRange != null)
                return;

            var controller = other.GetComponentInParent<PlayerController>();
            if (controller == null)
                return;

            playerInRange = controller;
            RefreshPrompt();
            PlayerEntered?.Invoke(controller);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (playerInRange == null || other.GetComponentInParent<PlayerController>() != playerInRange)
                return;

            playerInRange = null;
            RefreshPrompt();
            PlayerExited?.Invoke();
        }

        private void RefreshPrompt()
        {
            if (prompt != null)
                prompt.SetActive(interactable && playerInRange != null);
        }

        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider2D>();
            if (box == null)
                return;

            Gizmos.color = new Color(0.6f, 0.4f, 1f, 0.8f);
            Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
        }
    }
}