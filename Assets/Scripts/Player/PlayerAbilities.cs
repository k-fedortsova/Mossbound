using System;
using UnityEngine;

namespace Mossbound.Player
{
    public enum PlayerAbility
    {
        DoubleJump,
        WallJump,
        Dash,
    }

    public class PlayerAbilities : MonoBehaviour
    {
        [SerializeField] private bool doubleJump = true;
        [SerializeField] private bool wallJump = true;
        [SerializeField] private bool dash = true;

        public event Action<PlayerAbility> AbilityUnlocked;

        public bool Has(PlayerAbility ability)
        {
            return ability switch
            {
                PlayerAbility.DoubleJump => doubleJump,
                PlayerAbility.WallJump => wallJump,
                PlayerAbility.Dash => dash,
                _ => false,
            };
        }

        public void Unlock(PlayerAbility ability)
        {
            if (Has(ability))
                return;

            switch (ability)
            {
                case PlayerAbility.DoubleJump:
                    doubleJump = true;
                    break;
                case PlayerAbility.WallJump:
                    wallJump = true;
                    break;
                case PlayerAbility.Dash:
                    dash = true;
                    break;
            }

            AbilityUnlocked?.Invoke(ability);
        }
    }
}
