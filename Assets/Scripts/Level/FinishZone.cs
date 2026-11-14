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
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null || GameSession.Instance.IsFinished)
                return;

            player.Input.enabled = false;
            player.SetFrozen(true);
            GameSession.Instance.CompleteLevel();
        }
    }
}
