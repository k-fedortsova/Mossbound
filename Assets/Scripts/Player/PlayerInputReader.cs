using UnityEngine;
using UnityEngine.InputSystem;

namespace Mossbound.Player
{
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference dashAction;
        [SerializeField, Range(0f, 1f)] private float deadZone = 0.3f;

        private float jumpPressedTime = float.NegativeInfinity;
        private float dashPressedTime = float.NegativeInfinity;

        public int Horizontal { get; private set; }
        public int Vertical { get; private set; }
        public bool IsJumpHeld { get; private set; }

        private void OnEnable()
        {
            moveAction.action.Enable();
            jumpAction.action.Enable();
            dashAction.action.Enable();
        }

        private void OnDisable()
        {
            moveAction.action.Disable();
            jumpAction.action.Disable();
            dashAction.action.Disable();
            Horizontal = 0;
            Vertical = 0;
            IsJumpHeld = false;
            ClearBuffers();
        }

        private void Update()
        {
            Vector2 move = moveAction.action.ReadValue<Vector2>();
            Horizontal = ToDigital(move.x);
            Vertical = ToDigital(move.y);
            IsJumpHeld = jumpAction.action.IsPressed();

            if (jumpAction.action.WasPressedThisFrame())
                jumpPressedTime = Time.time;
            if (dashAction.action.WasPressedThisFrame())
                dashPressedTime = Time.time;
        }

        public bool IsJumpBuffered(float bufferTime)
        {
            return Time.time - jumpPressedTime <= bufferTime;
        }

        public bool IsDashBuffered(float bufferTime)
        {
            return Time.time - dashPressedTime <= bufferTime;
        }

        public void ConsumeJump()
        {
            jumpPressedTime = float.NegativeInfinity;
        }

        public void ConsumeDash()
        {
            dashPressedTime = float.NegativeInfinity;
        }

        public void ClearBuffers()
        {
            ConsumeJump();
            ConsumeDash();
        }

        private int ToDigital(float value)
        {
            if (Mathf.Abs(value) < deadZone)
                return 0;
            return value > 0f ? 1 : -1;
        }
    }
}
