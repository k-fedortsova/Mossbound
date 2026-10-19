using System;
using Mossbound.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Mossbound.Gameplay
{
    public class GameSession : MonoBehaviour
    {
        [SerializeField] private PlayerRespawn player;
        [SerializeField] private InputActionReference restartAction;

        public static GameSession Instance { get; private set; }

        public float ElapsedTime { get; private set; }
        public int Falls { get; private set; }
        public bool IsFinished { get; private set; }

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
            if (restartAction.action.WasPressedThisFrame())
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
