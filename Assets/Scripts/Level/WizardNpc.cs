using System;
using System.Collections;
using Mossbound.Gameplay;
using Mossbound.Player;
using Mossbound.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Mossbound.Level
{
    public enum WizardState { Idle, InteractionAvailable, RiddleActive, CorrectAnswer, WrongAnswer, Disappearing, Finished }

    [Serializable]
    public struct DialogueLine
    {
        public string speaker;
        [TextArea(2, 4)] public string text;

        public DialogueLine(string speaker, string text)
        {
            this.speaker = speaker;
            this.text = text;
        }
    }

    [RequireComponent(typeof(BoxCollider2D))]
    public class WizardNpc : MonoBehaviour
    {
        private const string Wizard = "Old Wizard";
        private const string Knight = "Knight";

        [SerializeField] private InputActionReference interactAction;
        [SerializeField] private DialogueBox dialogue;
        [Tooltip("\"Press E\" hint shown while the player is in range.")]
        [SerializeField] private GameObject prompt;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Conversation")]
        [SerializeField] private DialogueLine[] greeting =
        {
            new(Wizard, "Ah, a traveller! Few find their way this deep into the mossy woods."),
            new(Knight, "I seek the castle beyond the old bridges."),
            new(Wizard, "Those bridges are broken and treacherous. Answer my riddle, and I shall open a shorter path."),
        };
        [SerializeField] private string riddleSpeaker = Wizard;
        [SerializeField, TextArea(3, 6)] private string question =
            "I am the path that has no stones, crossing vast kingdoms without ever moving. " +
            "I can carry you to the highest elven towers or the deepest dwarven mines in the blink of an eye. What am I?";
        [SerializeField] private string[] answers = { "Bridge", "Portal", "Map" };
        [SerializeField, Min(0)] private int correctAnswer = 1;
        [SerializeField] private DialogueLine correctReply = new(Wizard, "Wise indeed! The portal is yours. Farewell, traveller.");
        [SerializeField] private DialogueLine wrongReply = new(Wizard, "Alas, no. The bridges it is, then. Watch your step.");

        [Header("Result")]
        [SerializeField] private string correctTitle = "THE PORTAL AWAKENS";
        [SerializeField, TextArea] private string correctHint = "A shortcut has opened.";
        [SerializeField] private string wrongTitle = "THE PORTAL STAYS CLOSED";
        [SerializeField, TextArea] private string wrongHint = "Cross the broken bridges.";
        [Tooltip("Raised once, only after the correct answer. Hook the portal's activation here.")]
        [SerializeField] private UnityEvent portalUnlocked;

        [Header("Disappear")]
        [SerializeField, Min(0.01f)] private float disappearTime = 0.8f;
        [SerializeField, Min(0f)] private float disappearRise = 0.5f;

        private BoxCollider2D interactionZone;
        private PlayerController player;

        public WizardState State { get; private set; } = WizardState.Idle;

        private void Reset()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void Awake()
        {
            interactionZone = GetComponent<BoxCollider2D>();
            SetPrompt(false);
        }

        private void OnEnable()
        {
            interactAction.action.Enable();
        }

        private void OnDisable()
        {
            interactAction.action.Disable();
        }

        private void Update()
        {
            if (State == WizardState.InteractionAvailable && interactAction.action.WasPressedThisFrame() && dialogue.TryOpen())
                StartCoroutine(Converse());
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (State != WizardState.Idle)
                return;

            var controller = other.GetComponentInParent<PlayerController>();
            if (controller == null)
                return;

            player = controller;
            State = WizardState.InteractionAvailable;
            SetPrompt(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (State != WizardState.InteractionAvailable || other.GetComponentInParent<PlayerController>() != player)
                return;

            player = null;
            State = WizardState.Idle;
            SetPrompt(false);
        }

        private IEnumerator Converse()
        {
            State = WizardState.RiddleActive;
            SetPrompt(false);
            player.SetFrozen(true);

            foreach (DialogueLine line in greeting)
                yield return dialogue.Say(line.speaker, line.text);

            int answer = -1;
            yield return dialogue.Ask(riddleSpeaker, question, answers, picked => answer = picked);

            bool correct = answer == correctAnswer;
            State = correct ? WizardState.CorrectAnswer : WizardState.WrongAnswer;
            DialogueLine reply = correct ? correctReply : wrongReply;
            yield return dialogue.Say(reply.speaker, reply.text);

            dialogue.Close();
            player.SetFrozen(false);
            player.Input.ClearBuffers();

            if (correct)
            {
                GameSession.Instance.ShowMessage(correctTitle, correctHint);
                portalUnlocked.Invoke();
            }
            else
            {
                GameSession.Instance.ShowMessage(wrongTitle, wrongHint);
            }

            yield return Disappear();
        }

        private IEnumerator Disappear()
        {
            State = WizardState.Disappearing;
            interactionZone.enabled = false;

            Transform visual = spriteRenderer.transform;
            Vector3 start = visual.localPosition;
            Color color = spriteRenderer.color;
            for (float time = 0f; time < disappearTime; time += Time.deltaTime)
            {
                float t = time / disappearTime;
                visual.localPosition = start + Vector3.up * (disappearRise * t);
                spriteRenderer.color = new Color(color.r, color.g, color.b, color.a * (1f - t));
                yield return null;
            }

            spriteRenderer.enabled = false;
            State = WizardState.Finished;
        }

        private void SetPrompt(bool visible)
        {
            if (prompt != null)
                prompt.SetActive(visible);
        }

        private void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider2D>();
            Gizmos.color = new Color(0.6f, 0.4f, 1f, 0.8f);
            Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
        }
    }
}
