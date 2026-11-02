using System.Collections.Generic;
using Mossbound.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Mossbound.UI
{
    public class LevelHud : MonoBehaviour
    {
        [SerializeField] private GameSession session;

        [Header("Stats")]
        [SerializeField] private Text timerText;
        [SerializeField] private Text fallsText;

        [Header("Banner")]
        [SerializeField] private CanvasGroup banner;
        [SerializeField] private Text bannerTitle;
        [SerializeField] private Text bannerHint;
        [Tooltip("Time the banner stays fully visible, not counting the fades.")]
        [SerializeField, Min(0f)] private float bannerDuration = 3.5f;
        [SerializeField, Min(0.01f)] private float bannerFadeTime = 0.5f;

        [Header("Finish")]
        [SerializeField] private GameObject finishPanel;
        [SerializeField] private Text finishStats;

        private readonly Queue<(string title, string hint)> pendingMessages = new Queue<(string, string)>();
        private float bannerTimer;

        private void Awake()
        {
            finishPanel.SetActive(false);
            banner.alpha = 0f;
        }

        private void OnEnable()
        {
            session.FallsChanged += UpdateFalls;
            session.MessageRequested += EnqueueMessage;
            session.Finished += ShowFinish;
        }

        private void OnDisable()
        {
            session.FallsChanged -= UpdateFalls;
            session.MessageRequested -= EnqueueMessage;
            session.Finished -= ShowFinish;
        }

        private void Start()
        {
            UpdateFalls(session.Falls);
        }

        private void Update()
        {
            timerText.text = FormatTime(session.ElapsedTime);
            UpdateBanner();
        }

        private void UpdateBanner()
        {
            if (bannerTimer <= 0f)
            {
                if (pendingMessages.Count > 0)
                    DisplayMessage(pendingMessages.Dequeue());
                return;
            }

            bannerTimer -= Time.deltaTime;
            float visibleTime = Mathf.Min(bannerTimer, BannerTotalTime - bannerTimer);
            banner.alpha = Mathf.Clamp01(visibleTime / bannerFadeTime);
        }

        private void EnqueueMessage(string title, string hint)
        {
            pendingMessages.Enqueue((title, hint));
        }

        private void DisplayMessage((string title, string hint) message)
        {
            bannerTitle.text = message.title;
            bannerHint.text = message.hint;
            bannerTimer = BannerTotalTime;
        }

        private float BannerTotalTime => bannerDuration + bannerFadeTime * 2f;

        private void UpdateFalls(int falls)
        {
            fallsText.text = $"Falls: {falls}";
        }

        private void ShowFinish()
        {
            pendingMessages.Clear();
            bannerTimer = 0f;
            banner.alpha = 0f;

            // The session fades to the next level on its own; the full finish panel is only for the last level.
            if (session.HasNextLevel)
            {
                DisplayMessage(("LEVEL COMPLETE", $"Time: {FormatTime(session.ElapsedTime)}    Falls: {session.Falls}"));
                return;
            }

            finishPanel.SetActive(true);
            finishStats.text = $"Time: {FormatTime(session.ElapsedTime)}    Falls: {session.Falls}";
        }

        private static string FormatTime(float seconds)
        {
            int minutes = (int)(seconds / 60f);
            return $"{minutes}:{seconds - minutes * 60f:00.0}";
        }
    }
}
