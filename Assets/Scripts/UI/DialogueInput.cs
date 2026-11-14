using UnityEngine;
using UnityEngine.InputSystem;

namespace Mossbound.UI
{
    public class DialogueInput : MonoBehaviour
    {
        [SerializeField] private InputActionReference continueAction;
        [Tooltip("One action per answer, in the order the answers are listed.")]
        [SerializeField] private InputActionReference[] choiceActions;

        public int MaxChoices => choiceActions.Length;

        public bool ContinuePressed => continueAction.action.WasPressedThisFrame();

        private void Awake()
        {
            Require.Assigned(continueAction, nameof(continueAction), this);
            if (Require.NotEmpty(choiceActions, nameof(choiceActions), this))
            {
                for (int i = 0; i < choiceActions.Length; i++)
                    Require.Assigned(choiceActions[i], $"{nameof(choiceActions)}[{i}]", this);
            }
        }

        private void OnEnable()
        {
            continueAction.action.Enable();
            foreach (InputActionReference choice in choiceActions)
                choice.action.Enable();
        }

        private void OnDisable()
        {
            continueAction.action.Disable();
            foreach (InputActionReference choice in choiceActions)
                choice.action.Disable();
        }

        public int PressedChoice(int count)
        {
            int available = Mathf.Min(count, choiceActions.Length);
            for (int i = 0; i < available; i++)
            {
                if (choiceActions[i].action.WasPressedThisFrame())
                    return i;
            }
            return -1;
        }
    }
}
