using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Mossbound.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogueBox : MonoBehaviour
    {
        [SerializeField] private InputActionReference continueAction;
        [SerializeField] private Text speakerText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text choicesText;
        [SerializeField] private GameObject continueHint;
        [SerializeField, Min(1f)] private float charactersPerSecond = 45f;

        private static readonly Key[] ChoiceKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4 };
        private static readonly Key[] NumpadKeys = { Key.Numpad1, Key.Numpad2, Key.Numpad3, Key.Numpad4 };

        private CanvasGroup canvasGroup;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            SetVisible(false);
        }

        private void OnEnable()
        {
            continueAction.action.Enable();
        }

        private void OnDisable()
        {
            continueAction.action.Disable();
        }

        public bool TryOpen()
        {
            if (IsOpen)
                return false;

            IsOpen = true;
            speakerText.text = string.Empty;
            bodyText.text = string.Empty;
            choicesText.text = string.Empty;
            SetVisible(true);
            return true;
        }

        public void Close()
        {
            IsOpen = false;
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

            int count = Mathf.Min(choices.Length, ChoiceKeys.Length);
            var parts = new string[count];
            for (int i = 0; i < count; i++)
                parts[i] = $"{i + 1}  {choices[i]}";
            choicesText.text = string.Join("        ", parts);

            int choice = -1;
            while (choice < 0)
            {
                yield return null;
                choice = PressedChoice(count);
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
                if (ContinuePressed())
                    break;
                bodyText.text = text.Substring(0, (int)shown);
                yield return null;
            }
            bodyText.text = text;
        }

        private IEnumerator WaitForContinue()
        {
            yield return null;
            while (!ContinuePressed())
                yield return null;
        }

        private bool ContinuePressed()
        {
            if (continueAction.action.WasPressedThisFrame())
                return true;
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame);
        }

        private static int PressedChoice(int count)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return -1;
            for (int i = 0; i < count; i++)
                if (keyboard[ChoiceKeys[i]].wasPressedThisFrame || keyboard[NumpadKeys[i]].wasPressedThisFrame)
                    return i;
            return -1;
        }

        private void SetVisible(bool visible)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
