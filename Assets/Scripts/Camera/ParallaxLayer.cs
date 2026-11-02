using UnityEngine;

namespace Mossbound.CameraSystem
{
    [DefaultExecutionOrder(100)]
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Vector2 factor = new Vector2(0.8f, 0.8f);
        [SerializeField] private Vector2 basePosition;

        private void LateUpdate()
        {
            Vector3 cameraPosition = cameraTransform.position;
            transform.position = new Vector3(
                basePosition.x + cameraPosition.x * factor.x,
                basePosition.y + cameraPosition.y * factor.y,
                transform.position.z);
        }
    }
}
