using UnityEngine;

namespace Mossbound.Player
{
    [RequireComponent(typeof(PlayerController), typeof(PlayerInputReader))]
    public class PlayerGuard : MonoBehaviour, IMoveSpeedModifier
    {
        [SerializeField, Range(0f, 1f)] private float moveSpeedMultiplier = 0.4f;

        private PlayerController controller;
        private PlayerInputReader input;
        private PlayerAttack attack;

        public bool IsGuarding { get; private set; }
        public float MoveSpeedMultiplier => IsGuarding ? moveSpeedMultiplier : 1f;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            input = GetComponent<PlayerInputReader>();
            attack = GetComponent<PlayerAttack>();
        }

        private void Update()
        {
            bool attacking = attack != null && attack.IsAttacking;
            IsGuarding = input.IsGuardHeld && controller.Sensor.IsGrounded && !attacking;
        }

        public bool CanBlock(Vector2 sourcePosition)
        {
            if (!IsGuarding)
                return false;

            float deltaX = sourcePosition.x - transform.position.x;
            return deltaX * controller.FacingDirection >= 0f;
        }
    }
}