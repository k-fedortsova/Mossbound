using Mossbound.Dialogue;
using Mossbound.Player;
using UnityEngine;

namespace Mossbound.Level
{
    [RequireComponent(typeof(NpcInteractionZone), typeof(DialogueRunner))]
    public class SimpleNpc : MonoBehaviour
    {
        [SerializeField] private DialogueData dialogue;

        private NpcInteractionZone zone;
        private DialogueRunner runner;

        private void Awake()
        {
            zone = GetComponent<NpcInteractionZone>();
            runner = GetComponent<DialogueRunner>();

            if (dialogue == null)
                Debug.LogError($"{nameof(SimpleNpc)} on '{name}' has no dialogue asset.", this);
        }

        private void OnEnable()
        {
            zone.Interactable = true;
            zone.Interacted += OnInteracted;
            runner.Finished += OnConversationEnded;
            runner.Interrupted += OnConversationEnded;
        }

        private void OnDisable()
        {
            runner.Interrupt();

            zone.Interacted -= OnInteracted;
            runner.Finished -= OnConversationEnded;
            runner.Interrupted -= OnConversationEnded;
        }

        private void OnInteracted(PlayerController player)
        {
            if (dialogue == null)
                return;

            if (runner.TryRun(player, runner.PlayLines(dialogue)))
                zone.Interactable = false;
        }

        private void OnConversationEnded()
        {
            zone.Interactable = true;
        }
    }
}