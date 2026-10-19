using UnityEngine;

namespace Mossbound.CameraSystem
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 offset = new Vector2(0f, 1.5f);
        [SerializeField, Min(0f)] private float smoothTime = 0.15f;
        [SerializeField, Min(0f)] private float lookAhead = 1.5f;

        private Vector3 velocity;
        private float lookDirection = 1f;
        private Vector3 lastTargetPosition;

        private void Start()
        {
            lastTargetPosition = target.position;
            transform.position = GetDesiredPosition();
        }

        private void LateUpdate()
        {
            float deltaX = target.position.x - lastTargetPosition.x;
            if (Mathf.Abs(deltaX) > 0.001f)
                lookDirection = Mathf.Sign(deltaX);
            lastTargetPosition = target.position;

            transform.position = Vector3.SmoothDamp(transform.position, GetDesiredPosition(), ref velocity, smoothTime);
        }

        private Vector3 GetDesiredPosition()
        {
            Vector3 position = target.position;
            return new Vector3(position.x + offset.x + lookDirection * lookAhead, position.y + offset.y, transform.position.z);
        }
    }
}
