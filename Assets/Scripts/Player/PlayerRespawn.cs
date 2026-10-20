using System;
using System.Collections;
using Mossbound.UI;
using UnityEngine;

namespace Mossbound.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerRespawn : MonoBehaviour
    {
        [SerializeField] private ScreenFader screenFader;
        [SerializeField] private float killY = -20f;
        [SerializeField, Range(0.4f, 0.6f)] private float fadeDuration = 0.5f;

        private PlayerController controller;
        private Vector3 spawnPoint;
        private bool isRespawning;

        public event Action Respawned;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            spawnPoint = transform.position;
        }

        private void FixedUpdate()
        {
            if (transform.position.y < killY)
                Respawn();
        }

        public void SetSpawnPoint(Vector3 point)
        {
            spawnPoint = point;
        }

        public void Respawn()
        {
            if (!isRespawning)
                StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            isRespawning = true;
            controller.SetFrozen(true);
            float halfFade = fadeDuration * 0.5f;

            yield return screenFader.Fade(1f, halfFade);
            transform.position = spawnPoint;
            controller.ResetMotion();
            Respawned?.Invoke();
            yield return screenFader.Fade(0f, halfFade);

            controller.Input.ClearBuffers();
            controller.SetFrozen(false);
            isRespawning = false;
        }
    }
}
