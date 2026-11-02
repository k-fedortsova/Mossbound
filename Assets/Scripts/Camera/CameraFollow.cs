using Mossbound.Player;
using Mossbound.Player.StateMachine;
using UnityEngine;

namespace Mossbound.CameraSystem
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 offset = new Vector2(0f, 1.5f);
        [SerializeField, Range(0.15f, 0.25f)] private float smoothTime = 0.2f;

        [Header("Look Ahead")]
        [SerializeField, Min(0f)] private float lookAhead = 1.75f;
        [Tooltip("Input must hold a new direction this long before the look-ahead flips.")]
        [SerializeField, Min(0f)] private float lookTurnDelay = 0.15f;

        [Header("Dash")]
        [SerializeField, Min(0f)] private float dashLookAhead = 2.5f;
        [Tooltip("How long the dash offset is kept after the dash ends.")]
        [SerializeField, Min(0f)] private float dashLookHoldTime = 0.15f;

        [Header("Vertical")]
        [SerializeField, Min(0f)] private float verticalDeadZone = 1.25f;

        private PlayerController player;
        private PlayerRespawn respawn;
        private Vector3 velocity;
        private float lookDirection = 1f;
        private float turnTimer;
        private Vector2 dashDirection;
        private float dashLookTimer;
        private float focusY;

        private void Awake()
        {
            player = target.GetComponent<PlayerController>();
            respawn = target.GetComponent<PlayerRespawn>();
        }

        private void OnEnable()
        {
            if (respawn != null)
                respawn.Respawned += SnapToTarget;
        }

        private void OnDisable()
        {
            if (respawn != null)
                respawn.Respawned -= SnapToTarget;
        }

        private void Start()
        {
            SnapToTarget();
        }

        private void LateUpdate()
        {
            UpdateLookDirection(Time.deltaTime);
            UpdateDashLook(Time.deltaTime);
            UpdateFocusY();
            transform.position = Vector3.SmoothDamp(transform.position, GetDesiredPosition(), ref velocity, smoothTime);
        }

        private void SnapToTarget()
        {
            lookDirection = player != null ? player.FacingDirection : lookDirection;
            turnTimer = 0f;
            dashLookTimer = 0f;
            focusY = target.position.y;
            velocity = Vector3.zero;
            transform.position = GetDesiredPosition();
        }

        private void UpdateLookDirection(float deltaTime)
        {
            int input = player != null ? player.Input.Horizontal : 0;
            if (input == 0 || input == (int)lookDirection)
            {
                turnTimer = 0f;
                return;
            }

            turnTimer += deltaTime;
            if (turnTimer >= lookTurnDelay)
            {
                lookDirection = input;
                turnTimer = 0f;
            }
        }

        private void UpdateDashLook(float deltaTime)
        {
            if (player != null && player.StateMachine.CurrentState.Id == PlayerStateId.Dashing)
            {
                dashDirection = player.DashingState.Direction;
                dashLookTimer = dashLookHoldTime;
                if (dashDirection.x != 0f)
                    lookDirection = Mathf.Sign(dashDirection.x);
                return;
            }

            dashLookTimer -= deltaTime;
        }

        private void UpdateFocusY()
        {
            float y = target.position.y;
            if (y > focusY + verticalDeadZone)
                focusY = y - verticalDeadZone;
            else if (y < focusY - verticalDeadZone)
                focusY = y + verticalDeadZone;
        }

        private Vector3 GetDesiredPosition()
        {
            Vector2 look = dashLookTimer > 0f ? dashDirection * dashLookAhead : new Vector2(lookDirection * lookAhead, 0f);
            return new Vector3(target.position.x + offset.x + look.x, focusY + offset.y + look.y, transform.position.z);
        }
    }
}
