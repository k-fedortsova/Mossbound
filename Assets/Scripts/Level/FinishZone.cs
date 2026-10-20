using Mossbound.Gameplay;
using Mossbound.Player;
using UnityEngine;

namespace Mossbound.Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class FinishZone : MonoBehaviour
    {
        private void Reset()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var input = other.GetComponentInParent<PlayerInputReader>();
            if (input == null || GameSession.Instance.IsFinished)
                return;

            input.enabled = false;
            GameSession.Instance.CompleteLevel();
        }
    }
}
