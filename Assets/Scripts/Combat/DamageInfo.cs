using UnityEngine;

namespace Mossbound.Combat
{
    public readonly struct DamageInfo
    {
        public int Amount { get; }
        public Vector2 SourcePosition { get; }

        public DamageInfo(int amount, Vector2 sourcePosition)
        {
            Amount = amount;
            SourcePosition = sourcePosition;
        }
    }

    public interface IDamageable
    {
        bool TakeDamage(DamageInfo info);
    }
}