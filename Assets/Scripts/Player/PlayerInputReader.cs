using UnityEngine;
using UnityEngine.InputSystem;

namespace Mossbound.Player
{
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference dashAction;
        [SerializeField] private InputActionReference guardAction;
        [SerializeField] private InputActionReference attackAction;
        [SerializeField, Range(0f, 1f)] private float deadZone = 0.3f;

        private float jumpPressedTime = float.NegativeInfinity;
        private float dashPressedTime = float.NegativeInfinity;
        private float attackPressedTime = float.NegativeInfinity;

        public int Horizontal { get; private set; }
        public int Vertical { get; private set; }
        public bool IsJumpHeld { get; private set; }
        public bool IsGuardHeld { get; private set; }

        private void OnEnable()
        {
            moveAction.action.Enable();
            jumpAction.action.Enable();
            dashAction.action.Enable();
            if (guardAction != null) guardAction.action.Enable();
            if (attackAction != null) attackAction.action.Enable();
        }

        private void OnDisable()
        {
            moveAction.action.Disable();
            jumpAction.action.Disable();
            dashAction.action.Disable();
            if (guardAction != null) guardAction.action.Disable();
            if (attackAction != null) attackAction.action.Disable();
            Horizontal = 0;
            Vertical = 0;
            IsJumpHeld = false;
            IsGuardHeld = false;
            ClearBuffers();
        }

        private void Update()
        {
            Vector2 move = moveAction.action.ReadValue<Vector2>();
            Horizontal = ToDigital(move.x);
            Vertical = ToDigital(move.y);
            IsJumpHeld = jumpAction.action.IsPressed();
            IsGuardHeld = guardAction != null && guardAction.action.IsPressed();

            if (jumpAction.action.WasPressedThisFrame())
                jumpPressedTime = Time.time;
            if (dashAction.action.WasPressedThisFrame())
                dashPressedTime = Time.time;
            if (attackAction != null && attackAction.action.WasPressedThisFrame())
                attackPressedTime = Time.time;
        }

        public bool IsJumpBuffered(float bufferTime)
        {
            return Time.time - jumpPressedTime <= bufferTime;
        }

        public bool IsDashBuffered(float bufferTime)
        {
            return Time.time - dashPressedTime <= bufferTime;
        }

        public bool IsAttackBuffered(float bufferTime)
        {
            return Time.time - attackPressedTime <= bufferTime;
        }

        public void ConsumeJump()
        {
            jumpPressedTime = float.NegativeInfinity;
        }

        public void ConsumeDash()
        {
            dashPressedTime = float.NegativeInfinity;
        }

        public void ConsumeAttack()
        {
            attackPressedTime = float.NegativeInfinity;
        }

        public void ClearBuffers()
        {
            ConsumeJump();
            ConsumeDash();
            ConsumeAttack();
        }

        private int ToDigital(float value)
        {
            if (Mathf.Abs(value) < deadZone)
                return 0;
            return value > 0f ? 1 : -1;
        }
    }
}