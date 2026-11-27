using System;
using System.Collections;
using Mossbound.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mossbound.Menu
{
    // Start screen flow: intro animation, Play / Controls / Quit and the Controls window.
    // The first level is the scene right after the menu in the build settings.
    public class MainMenu : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private ScreenFader fader;
        [SerializeField] private Camera menuCamera;

        [Header("Main")]
        [Tooltip("Turned off while the intro plays and while the Controls window is open.")]
        [SerializeField] private CanvasGroup mainGroup;
        [SerializeField] private RectTransform title;
        [SerializeField] private RectTransform panel;
        [SerializeField] private MenuButton playButton;
        [SerializeField] private MenuButton controlsButton;
        [SerializeField] private MenuButton quitButton;
        [SerializeField] private CanvasGroup hint;

        [Header("Controls Window")]
        [SerializeField] private CanvasGroup controlsGroup;
        [SerializeField] private RectTransform controlsWindow;
        [SerializeField] private MenuButton controlsBackButton;

        [Header("Cursor")]
        [SerializeField] private Texture2D cursor;
        [SerializeField] private Vector2 cursorHotspot = new Vector2(12f, 8f);

        [Header("Timing")]
        [SerializeField, Min(0.01f)] private float fadeInTime = 1.2f;
        [SerializeField, Min(0.01f)] private float fadeOutTime = 0.9f;

        private MenuButton[] mainButtons;
        private Vector2 titleOrigin, panelOrigin;
        private Vector2[] buttonOrigins;
        private bool busy = true;
        private bool controlsOpen;
        private float cameraSize;

        private void Awake()
        {
            mainButtons = Application.platform == RuntimePlatform.WebGLPlayer
                ? new[] { playButton, controlsButton }
                : new[] { playButton, controlsButton, quitButton };
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                quitButton.gameObject.SetActive(false);

            titleOrigin = title.anchoredPosition;
            panelOrigin = panel.anchoredPosition;
            buttonOrigins = Array.ConvertAll(mainButtons, b => ((RectTransform)b.transform).anchoredPosition);
            cameraSize = menuCamera.orthographicSize;

            playButton.Button.onClick.AddListener(OnPlay);
            controlsButton.Button.onClick.AddListener(OpenControls);
            quitButton.Button.onClick.AddListener(OnQuit);
            controlsBackButton.Button.onClick.AddListener(CloseControls);

            controlsGroup.gameObject.SetActive(false);
            mainGroup.interactable = false;
        }

        private void Start()
        {
            if (cursor != null)
                Cursor.SetCursor(cursor, cursorHotspot, CursorMode.Auto);
            StartCoroutine(Intro());
        }

        private void OnDestroy()
        {
            if (cursor != null)
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void Update()
        {
            if (hint != null)
                hint.alpha = busy ? Mathf.MoveTowards(hint.alpha, 0f, Time.unscaledDeltaTime * 3f)
                    : 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 2.5f);

            if (busy)
                return;

            if (controlsOpen && BackPressed())
            {
                CloseControls();
                return;
            }

            // Keyboard / gamepad users get a selection back as soon as they navigate.
            EventSystem events = EventSystem.current;
            if (events != null && events.currentSelectedGameObject == null && NavigatePressed())
                events.SetSelectedGameObject((controlsOpen ? controlsBackButton : playButton).gameObject);
        }

        // ---------- Intro ----------

        private IEnumerator Intro()
        {
            busy = true;
            title.anchoredPosition = titleOrigin + new Vector2(0f, 420f);
            panel.localScale = new Vector3(1f, 0f, 1f);
            foreach (MenuButton button in mainButtons)
                button.GetComponent<CanvasGroup>().alpha = 0f;

            var fadeGroup = fader.GetComponent<CanvasGroup>();
            fadeGroup.alpha = 1f;
            StartCoroutine(fader.Fade(0f, fadeInTime));

            StartCoroutine(Tween(0.25f, 0.9f, t => title.anchoredPosition = titleOrigin + new Vector2(0f, 420f * (1f - EaseOutBounce(t)))));
            StartCoroutine(Tween(0.6f, 0.55f, t =>
            {
                panel.localScale = new Vector3(1f, EaseOutBack(t), 1f);
                panel.anchoredPosition = panelOrigin;
            }));

            for (int i = 0; i < mainButtons.Length; i++)
            {
                int index = i;
                var rect = (RectTransform)mainButtons[i].transform;
                var group = mainButtons[i].GetComponent<CanvasGroup>();
                StartCoroutine(Tween(0.95f + 0.12f * i, 0.45f, t =>
                {
                    group.alpha = Mathf.Clamp01(t * 1.6f);
                    rect.anchoredPosition = buttonOrigins[index] + new Vector2(-90f * (1f - EaseOutBack(t)), 0f);
                }));
            }

            yield return WaitUnscaled(0.95f + 0.12f * mainButtons.Length + 0.35f);

            busy = false;
            mainGroup.interactable = true;
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }

        // ---------- Buttons ----------

        private void OnPlay()
        {
            if (busy)
                return;
            StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            busy = true;
            mainGroup.interactable = false;
            playButton.Punch(9f);
            yield return WaitUnscaled(0.15f);

            // Push into the island while the screen goes dark.
            StartCoroutine(Tween(0f, fadeOutTime, t => menuCamera.orthographicSize = Mathf.Lerp(cameraSize, cameraSize * 0.82f, t * t)));
            StartCoroutine(Tween(0f, fadeOutTime * 0.6f, t => title.anchoredPosition = titleOrigin + new Vector2(0f, 420f * t * t)));
            yield return fader.Fade(1f, fadeOutTime);

            ScreenFader.FadeInOnNextLoad();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        private void OnQuit()
        {
            if (busy)
                return;
            StartCoroutine(QuitRoutine());
        }

        private IEnumerator QuitRoutine()
        {
            busy = true;
            mainGroup.interactable = false;
            quitButton.Punch(9f);
            yield return fader.Fade(1f, fadeOutTime * 0.7f);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OpenControls()
        {
            if (busy || controlsOpen)
                return;
            controlsButton.Punch();
            StartCoroutine(ControlsRoutine(true));
        }

        private void CloseControls()
        {
            if (busy || !controlsOpen)
                return;
            StartCoroutine(ControlsRoutine(false));
        }

        private IEnumerator ControlsRoutine(bool open)
        {
            busy = true;
            controlsOpen = open;
            mainGroup.interactable = false;
            controlsGroup.interactable = false;
            controlsGroup.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);

            yield return Tween(0f, open ? 0.4f : 0.25f, t =>
            {
                float k = open ? t : 1f - t;
                controlsGroup.alpha = Mathf.Clamp01(k * 1.5f);
                float s = open ? Mathf.LerpUnclamped(0.6f, 1f, EaseOutBack(t)) : Mathf.Lerp(1f, 0.7f, t);
                controlsWindow.localScale = new Vector3(s, s, 1f);
            });

            if (open)
            {
                controlsGroup.interactable = true;
                EventSystem.current.SetSelectedGameObject(controlsBackButton.gameObject);
            }
            else
            {
                controlsGroup.gameObject.SetActive(false);
                mainGroup.interactable = true;
                EventSystem.current.SetSelectedGameObject(controlsButton.gameObject);
            }
            busy = false;
        }

        // ---------- Input ----------

        private static bool BackPressed()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;
            return (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame))
                   || (gamepad != null && gamepad.buttonEast.wasPressedThisFrame);
        }

        private static bool NavigatePressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.upArrowKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame
                                     || keyboard.wKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame
                                     || keyboard.tabKey.wasPressedThisFrame))
                return true;
            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.leftStick.ReadValue().sqrMagnitude > 0.25f;
        }

        // ---------- Tween helpers ----------

        private static IEnumerator Tween(float delay, float duration, Action<float> apply)
        {
            if (delay > 0f)
                yield return WaitUnscaled(delay);
            for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
            {
                apply(time / duration);
                yield return null;
            }
            apply(1f);
        }

        private static IEnumerator WaitUnscaled(float seconds)
        {
            for (float time = 0f; time < seconds; time += Time.unscaledDeltaTime)
                yield return null;
        }

        private static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }

        private static float EaseOutBounce(float x)
        {
            const float n1 = 7.5625f, d1 = 2.75f;
            if (x < 1f / d1) return n1 * x * x;
            if (x < 2f / d1) return n1 * (x -= 1.5f / d1) * x + 0.75f;
            if (x < 2.5f / d1) return n1 * (x -= 2.25f / d1) * x + 0.9375f;
            return n1 * (x -= 2.625f / d1) * x + 0.984375f;
        }
    }
}
