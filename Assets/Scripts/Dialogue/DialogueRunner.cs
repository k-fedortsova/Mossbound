using System;
using System.Collections;
using Mossbound.Player;
using Mossbound.UI;
using UnityEngine;

namespace Mossbound.Dialogue
{
    public class DialogueRunner : MonoBehaviour
    {
        [Tooltip("Leave empty to use the DialogueBox found in the scene (needed for prefabs).")]
        [SerializeField] private DialogueBox dialogueBox;

        private Coroutine routine;
        private PlayerController player;

        public bool IsRunning { get; private set; }

        public event Action Finished;

        public event Action Interrupted;

        private void Awake()
        {
            if (dialogueBox == null)
                dialogueBox = FindFirstObjectByType<DialogueBox>(FindObjectsInactive.Include);

            if (dialogueBox == null)
                Debug.LogError($"{nameof(DialogueRunner)} on '{name}' found no {nameof(DialogueBox)} in the scene.", this);
        }

        private void OnDisable()
        {
            Interrupt();
        }

        public bool TryRun(PlayerController talker, IEnumerator script)
        {
            if (IsRunning || talker == null || dialogueBox == null || !dialogueBox.TryOpen())
                return false;

            IsRunning = true;
            player = talker;
            player.SetFrozen(true);
            routine = StartCoroutine(Run(script));
            return true;
        }

        public void Interrupt()
        {
            if (!IsRunning)
                return;

            if (routine != null)
                StopCoroutine(routine);

            Release();
            Interrupted?.Invoke();
        }

        public IEnumerator PlayLines(DialogueData data)
        {
            if (data == null)
                yield break;

            for (int i = 0; i < data.Count; i++)
                yield return Say(data.GetLine(i));
        }

        public IEnumerator Say(DialogueLine line)
        {
            return dialogueBox.Say(line.speaker, line.text);
        }

        public IEnumerator Ask(string speaker, string question, string[] choices, Action<int> picked)
        {
            return dialogueBox.Ask(speaker, question, choices, picked);
        }

        private IEnumerator Run(IEnumerator script)
        {
            yield return script;

            Release();
            Finished?.Invoke();
        }

        private void Release()
        {
            routine = null;
            IsRunning = false;

            if (dialogueBox != null)
                dialogueBox.Close();

            if (player != null)
            {
                player.SetFrozen(false);
                player.Input.ClearBuffers();
            }

            player = null;
        }
    }
}