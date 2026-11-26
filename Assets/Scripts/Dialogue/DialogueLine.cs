using System;
using UnityEngine;

namespace Mossbound.Dialogue
{
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
}