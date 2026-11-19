using System;
using System.Collections;
using Mossbound.Gameplay;
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
        [SerializeField, Min(0f)] private float deathDelay = 0.4f;

        private PlayerController controller;
        private PlayerHealth health;
        private Vector3 spawnPoint;
        private bool isRespawning;

        public event Action Respawned;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            health = GetComponent<PlayerHealth>();
            spawnPoint = transform.position;

            Require.Assigned(screenFader, nameof(screenFader), this);
        }

        private void OnEnable()
        {
            if (health != null)
                health.Died += OnDied;
        }

        private void OnDisable()
        {
            if (health != null)
                health.Died -= OnDied;
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
            StartRespawn(0f);
        }

        private void OnDied()
        {
            StartRespawn(deathDelay);
        }

        private void StartRespawn(float delayBeforeFade)
        {
            if (isRespawning || GameSession.Instance.IsFinished)
                return;

            StartCoroutine(RespawnRoutine(delayBeforeFade));
        }

        private IEnumerator RespawnRoutine(float delayBeforeFade)
        {
            isRespawning = true;
            controller.SetFrozen(true);

            if (delayBeforeFade > 0f)
                yield return new WaitForSeconds(delayBeforeFade);

            float halfFade = fadeDuration * 0.5f;

            yield return screenFader.Fade(1f, halfFade);
            controller.TeleportTo(spawnPoint);
            Respawned?.Invoke();
            yield return screenFader.Fade(0f, halfFade);

            controller.Input.ClearBuffers();
            controller.SetFrozen(false);
            isRespawning = false;
        }
    }
}