using Mossbound.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace Mossbound.CameraSystem
{
    /// <summary>
    /// Cuts the Cinemachine camera to the player after a teleport (portal, respawn),
    /// so it does not fly across the level with damping.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraTeleportSnap : MonoBehaviour
    {
        [SerializeField] private PlayerController player;

        private CinemachineCamera virtualCamera;

        private void Awake()
        {
            virtualCamera = GetComponent<CinemachineCamera>();
            Require.Assigned(player, nameof(player), this);
        }

        private void OnEnable()
        {
            if (player != null)
                player.Teleported += Snap;
        }

        private void OnDisable()
        {
            if (player != null)
                player.Teleported -= Snap;
        }

        // Forget the previous frame: on the next update the camera jumps straight to the target, without damping.
        private void Snap()
        {
            virtualCamera.PreviousStateIsValid = false;
        }
    }
}
