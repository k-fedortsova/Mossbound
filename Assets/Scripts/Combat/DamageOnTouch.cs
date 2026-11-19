using UnityEngine;

namespace Mossbound.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class DamageOnTouch : MonoBehaviour
    {
        [SerializeField, Min(1)] private int damage = 1;

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(new DamageInfo(damage, transform.position));
        }
    }
}