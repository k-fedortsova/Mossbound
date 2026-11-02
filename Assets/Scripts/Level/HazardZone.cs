using Mossbound.Player;
using UnityEngine;

namespace Mossbound.Level
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class HazardZone : MonoBehaviour
    {
        private void Reset()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var respawn = other.GetComponentInParent<PlayerRespawn>();
            if (respawn != null)
                respawn.Respawn();
        }
    }
}
