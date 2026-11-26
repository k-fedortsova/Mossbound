using UnityEngine;

namespace Mossbound.Dialogue
{
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Mossbound/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        [SerializeField] private DialogueLine[] lines = System.Array.Empty<DialogueLine>();

        public int Count => lines.Length;

        public DialogueLine GetLine(int index)
        {
            return lines[index];
        }
    }
}