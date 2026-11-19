using System;
using System.Collections;
using Mossbound.Dialogue;
using Mossbound.Gameplay;
using Mossbound.Player;
using Mossbound.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Mossbound.Level
{
    public enum WizardState
    {
        Idle = 0,
        InteractionAvailable = 1,
        RiddleActive = 2,
        CorrectAnswer = 3,
        WrongAnswer = 4,
        Disappearing = 5,
        Finished = 6,
    }

    [RequireComponent(typeof(NpcInteractionZone), typeof(DialogueRunner))]
    public class WizardNpc : MonoBehaviour
    {
        private const string Wizard = "Old Wizard";

        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Conversation")]
        [Tooltip("Greeting lines shown before the riddle.")]
        [SerializeField] private DialogueData greeting;
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

        private NpcInteractionZone zone;
        private DialogueRunner runner;
        private bool answeredCorrectly;

        public WizardState State { get; private set; } = WizardState.Idle;

        private void Awake()
        {
            zone = GetComponent<NpcInteractionZone>();
            runner = GetComponent<DialogueRunner>();

            Require.Assigned(greeting, nameof(greeting), this);
            Require.Assigned(spriteRenderer, nameof(spriteRenderer), this);
            if (Require.NotEmpty(answers, nameof(answers), this))
                Require.InRange(correctAnswer, answers.Length, nameof(correctAnswer), this);
        }

        private void OnEnable()
        {
            zone.PlayerEntered += OnPlayerEntered;
            zone.PlayerExited += OnPlayerExited;
            zone.Interacted += OnInteracted;
            runner.Finished += OnConversationFinished;
            runner.Interrupted += OnConversationInterrupted;
        }

        private void OnDisable()
        {
            runner.Interrupt();

            zone.PlayerEntered -= OnPlayerEntered;
            zone.PlayerExited -= OnPlayerExited;
            zone.Interacted -= OnInteracted;
            runner.Finished -= OnConversationFinished;
            runner.Interrupted -= OnConversationInterrupted;
        }

        private void OnPlayerEntered(PlayerController player)
        {
            if (State == WizardState.Idle)
                State = WizardState.InteractionAvailable;
        }

        private void OnPlayerExited()
        {
            if (State == WizardState.InteractionAvailable)
                State = WizardState.Idle;
        }

        private void OnInteracted(PlayerController player)
        {
            if (State != WizardState.InteractionAvailable)
                return;

            if (!runner.TryRun(player, Converse()))
                return;

            State = WizardState.RiddleActive;
            zone.Interactable = false;
        }

        private IEnumerator Converse()
        {
            yield return runner.PlayLines(greeting);

            int answer = -1;
            yield return runner.Ask(riddleSpeaker, question, answers, picked => answer = picked);

            answeredCorrectly = answer == correctAnswer;
            State = answeredCorrectly ? WizardState.CorrectAnswer : WizardState.WrongAnswer;
            yield return runner.Say(answeredCorrectly ? correctReply : wrongReply);
        }

        private void OnConversationFinished()
        {
            if (answeredCorrectly)
            {
                GameSession.Instance.ShowMessage(correctTitle, correctHint);
                portalUnlocked.Invoke();
            }
            else
            {
                GameSession.Instance.ShowMessage(wrongTitle, wrongHint);
            }

            StartCoroutine(Disappear());
        }

        private void OnConversationInterrupted()
        {
            State = zone.IsPlayerInRange ? WizardState.InteractionAvailable : WizardState.Idle;
            zone.Interactable = true;
        }

        private IEnumerator Disappear()
        {
            State = WizardState.Disappearing;
            zone.Interactable = false;

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
    }
}