using System;
using System.Collections;
using Mossbound.Player;
using Mossbound.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Mossbound.Gameplay
{
    public class GameSession : MonoBehaviour
    {
        [SerializeField] private PlayerRespawn player;
        [SerializeField] private InputActionReference restartAction;

        [Header("Next Level")]
        [Tooltip("Time the completion message stays on screen before the fade to the next level.")]
        [SerializeField, Min(0f)] private float nextLevelDelay = 2f;
        [SerializeField, Min(0.01f)] private float nextLevelFadeTime = 1f;

        private bool isLoadingNextLevel;

        public static GameSession Instance { get; private set; }

        public float ElapsedTime { get; private set; }
        public int Falls { get; private set; }
        public bool IsFinished { get; private set; }
        public bool HasNextLevel => SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings;

        public event Action<int> FallsChanged;
        public event Action<string, string> MessageRequested;
        public event Action Finished;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            restartAction.action.Enable();
            player.Respawned += OnPlayerRespawned;
        }

        private void OnDisable()
        {
            restartAction.action.Disable();
            player.Respawned -= OnPlayerRespawned;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if (restartAction.action.WasPressedThisFrame() && !isLoadingNextLevel)
            {
                Restart();
                return;
            }

            if (!IsFinished)
                ElapsedTime += Time.deltaTime;
        }

        public void ShowMessage(string title, string hint)
        {
            if (!IsFinished)
                MessageRequested?.Invoke(title, hint);
        }

        public void CompleteLevel()
        {
            if (IsFinished)
                return;

            IsFinished = true;
            Finished?.Invoke();
            if (HasNextLevel)
                StartCoroutine(LoadNextLevel());
        }

        private IEnumerator LoadNextLevel()
        {
            isLoadingNextLevel = true;
            yield return new WaitForSeconds(nextLevelDelay);

            var fader = FindAnyObjectByType<ScreenFader>();
            if (fader != null)
                yield return fader.Fade(1f, nextLevelFadeTime);

            ScreenFader.FadeInOnNextLoad();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnPlayerRespawned()
        {
            Falls++;
            FallsChanged?.Invoke(Falls);
        }
    }
}
