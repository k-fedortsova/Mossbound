using System.Collections.Generic;
using Mossbound.Gameplay;
using Mossbound.Player;
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

        [Header("Health")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Image[] heartIcons;
        [SerializeField] private Sprite fullHeartSprite;
        [SerializeField] private Sprite emptyHeartSprite;

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
            Require.Assigned(session, nameof(session), this);
            Require.Assigned(timerText, nameof(timerText), this);
            Require.Assigned(fallsText, nameof(fallsText), this);
            Require.Assigned(banner, nameof(banner), this);
            Require.Assigned(bannerTitle, nameof(bannerTitle), this);
            Require.Assigned(bannerHint, nameof(bannerHint), this);
            Require.Assigned(finishPanel, nameof(finishPanel), this);
            Require.Assigned(finishStats, nameof(finishStats), this);

            finishPanel.SetActive(false);
            banner.alpha = 0f;
        }

        private void OnEnable()
        {
            session.FallsChanged += UpdateFalls;
            session.MessageRequested += EnqueueMessage;
            session.Finished += ShowFinish;

            if (playerHealth != null)
                playerHealth.HealthChanged += UpdateHealth;
        }

        private void OnDisable()
        {
            session.FallsChanged -= UpdateFalls;
            session.MessageRequested -= EnqueueMessage;
            session.Finished -= ShowFinish;

            if (playerHealth != null)
                playerHealth.HealthChanged -= UpdateHealth;
        }

        private void Start()
        {
            UpdateFalls(session.Falls);

            if (playerHealth != null)
                UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        private void Update()
        {
            timerText.text = FormatTime(session.ElapsedTime);
            UpdateBanner();
        }

        private void UpdateHealth(int current, int max)
        {
            if (heartIcons == null)
                return;

            for (int i = 0; i < heartIcons.Length; i++)
            {
                if (heartIcons[i] != null)
                    heartIcons[i].sprite = i < current ? fullHeartSprite : emptyHeartSprite;
            }
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