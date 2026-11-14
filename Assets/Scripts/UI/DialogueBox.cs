using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Mossbound.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogueBox : MonoBehaviour
    {
        [Tooltip("Reads the dialogue controls; this window only shows text.")]
        [SerializeField] private DialogueInput input;
        [SerializeField] private Text speakerText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text choicesText;
        [SerializeField] private GameObject continueHint;
        [SerializeField, Min(1f)] private float charactersPerSecond = 45f;

        private CanvasGroup canvasGroup;

        private bool isOpen;

        private void Awake()
        {
            Require.Assigned(input, nameof(input), this);
            Require.Assigned(speakerText, nameof(speakerText), this);
            Require.Assigned(bodyText, nameof(bodyText), this);
            Require.Assigned(choicesText, nameof(choicesText), this);
            Require.Assigned(continueHint, nameof(continueHint), this);

            canvasGroup = GetComponent<CanvasGroup>();
            SetVisible(false);
        }

        public bool TryOpen()
        {
            if (isOpen)
                return false;

            isOpen = true;
            speakerText.text = string.Empty;
            bodyText.text = string.Empty;
            choicesText.text = string.Empty;
            SetVisible(true);
            return true;
        }

        public void Close()
        {
            isOpen = false;
            SetVisible(false);
        }

        public IEnumerator Say(string speaker, string text)
        {
            yield return TypeLine(speaker, text);
            continueHint.SetActive(true);
            yield return WaitForContinue();
            continueHint.SetActive(false);
        }

        public IEnumerator Ask(string speaker, string question, string[] choices, Action<int> picked)
        {
            yield return TypeLine(speaker, question);

            int count = Mathf.Min(choices.Length, input.MaxChoices);
            var parts = new string[count];
            for (int i = 0; i < count; i++)
                parts[i] = $"{i + 1}  {choices[i]}";
            choicesText.text = string.Join("        ", parts);

            int choice = -1;
            while (choice < 0)
            {
                yield return null;
                choice = input.PressedChoice(count);
            }

            choicesText.text = string.Empty;
            picked(choice);
        }

        private IEnumerator TypeLine(string speaker, string text)
        {
            speakerText.text = speaker;
            bodyText.text = string.Empty;
            continueHint.SetActive(false);

            yield return null;
            for (float shown = 0f; shown < text.Length; shown += charactersPerSecond * Time.deltaTime)
            {
                if (input.ContinuePressed)
                    break;
                bodyText.text = text.Substring(0, (int)shown);
                yield return null;
            }
            bodyText.text = text;
        }

        private IEnumerator WaitForContinue()
        {
            yield return null;
            while (!input.ContinuePressed)
                yield return null;
        }

        private void SetVisible(bool visible)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
