using Mossbound.Gameplay;
using Mossbound.Player;
using UnityEngine;

namespace Mossbound.Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private Vector2 respawnOffset = new Vector2(0f, 0.7f);
        [SerializeField] private string zoneTitle;
        [SerializeField, TextArea] private string zoneHint;

        private bool isActivated;

        private void Reset()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isActivated)
                return;

            var respawn = other.GetComponentInParent<PlayerRespawn>();
            if (respawn == null)
                return;

            isActivated = true;
            respawn.SetSpawnPoint(transform.position + (Vector3)respawnOffset);
            if (!string.IsNullOrEmpty(zoneTitle))
                GameSession.Instance.ShowMessage(zoneTitle, zoneHint);
        }

        private void OnDrawGizmos()
        {
            Vector3 spawn = transform.position + (Vector3)respawnOffset;
            Gizmos.color = new Color(0.3f, 1f, 0.5f, 0.8f);
            Gizmos.DrawWireSphere(spawn, 0.3f);
            Gizmos.DrawLine(spawn, spawn + Vector3.up * 1.2f);
        }
    }
}
