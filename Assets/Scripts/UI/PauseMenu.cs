using System.Collections;
using System.Collections.Generic;
using Mossbound.Gameplay;
using Mossbound.Menu;
using Mossbound.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mossbound.UI
{
    // Esc (or the gamepad Select button) pauses any level: Resume, Main Menu, Quit.
    // It installs itself in every scene that has a GameSession, so the level scenes need no changes.
    // The look matches the main menu: same scroll, ribbon and animated buttons (sprites in Resources/MenuUI).
    public class PauseMenu : MonoBehaviour
    {
        private const string MainMenuScene = "MainMenu";
        private const string ArtFolder = "MenuUI/";

        private static readonly Color Cream = new Color32(0xff, 0xf4, 0xd6, 0xff);
        private static readonly Color Ink = new Color32(0x1d, 0x1a, 0x2b, 0xff);

        private CanvasGroup root;
        private CanvasGroup fade;
        private RectTransform window;
        private MenuButton resumeButton, menuButton, quitButton;
        private readonly List<Behaviour> frozen = new List<Behaviour>();
        private bool paused;
        private bool busy;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // A scene can be loaded from the pause screen (Restart, Main Menu): never start it frozen.
            Time.timeScale = 1f;
            AudioListener.pause = false;
            if (FindAnyObjectByType<GameSession>() == null || FindAnyObjectByType<PauseMenu>() != null)
                return;
            new GameObject("Pause Menu").AddComponent<PauseMenu>();
        }

        private void Awake()
        {
            BuildUi();
            EnsureEventSystem();
            root.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (busy || !TogglePressed())
                return;
            if (paused)
                StartCoroutine(Resume());
            else
                StartCoroutine(Pause());
        }

        private static bool TogglePressed()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;
            return (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                   || (gamepad != null && gamepad.selectButton.wasPressedThisFrame);
        }

        // ---------- Pause / resume ----------

        private IEnumerator Pause()
        {
            busy = true;
            paused = true;
            Time.timeScale = 0f;
            AudioListener.pause = true;

            // The player must not jump or dash from the keys used in the menu, and the level timer / restart key stop too.
            frozen.Clear();
            foreach (Behaviour behaviour in FindObjectsByType<PlayerInputReader>(FindObjectsSortMode.None))
                Freeze(behaviour);
            foreach (Behaviour behaviour in FindObjectsByType<GameSession>(FindObjectsSortMode.None))
                Freeze(behaviour);

            root.gameObject.SetActive(true);
            root.interactable = false;
            EventSystem.current.SetSelectedGameObject(null);
            yield return Tween(0.3f, t =>
            {
                root.alpha = Mathf.Clamp01(t * 1.6f);
                float s = Mathf.LerpUnclamped(0.7f, 1f, EaseOutBack(t));
                window.localScale = new Vector3(s, s, 1f);
            });
            root.interactable = true;
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
            busy = false;
        }

        private IEnumerator Resume()
        {
            busy = true;
            root.interactable = false;
            EventSystem.current.SetSelectedGameObject(null);
            yield return Tween(0.2f, t =>
            {
                root.alpha = 1f - t;
                float s = Mathf.Lerp(1f, 0.8f, t);
                window.localScale = new Vector3(s, s, 1f);
            });
            root.gameObject.SetActive(false);

            foreach (Behaviour behaviour in frozen)
                if (behaviour != null)
                    behaviour.enabled = true;
            frozen.Clear();
            Time.timeScale = 1f;
            AudioListener.pause = false;
            paused = false;
            busy = false;
        }

        private void Freeze(Behaviour behaviour)
        {
            if (!behaviour.enabled)
                return;
            behaviour.enabled = false;
            frozen.Add(behaviour);
        }

        private void OnResume()
        {
            if (busy || !paused)
                return;
            resumeButton.Punch();
            StartCoroutine(Resume());
        }

        private void OnMainMenu()
        {
            if (busy)
                return;
            menuButton.Punch(9f);
            StartCoroutine(Leave(() => SceneManager.LoadScene(MainMenuScene)));
        }

        private void OnQuit()
        {
            if (busy)
                return;
            quitButton.Punch(9f);
            StartCoroutine(Leave(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }));
        }

        private IEnumerator Leave(System.Action then)
        {
            busy = true;
            root.interactable = false;
            yield return Tween(0.6f, t => fade.alpha = t);
            Time.timeScale = 1f;
            AudioListener.pause = false;
            then();
        }

        // ---------- UI ----------

        private void BuildUi()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasObject = new GameObject("Pause UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvasObject.layer = LayerMask.NameToLayer("UI");
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            Transform canvasTransform = canvasObject.transform;

            // Always-visible hint in the corner.
            Text hint = CreateText(canvasTransform, "Pause Hint", font, 24, new Color(1f, 1f, 1f, 0.7f),
                new Vector2(1f, 0f), new Vector2(-40f, 30f), new Vector2(400f, 40f));
            hint.alignment = TextAnchor.LowerRight;
            hint.text = "Esc: Pause";

            var rootRect = Stretched(canvasTransform, "Pause");
            root = rootRect.gameObject.AddComponent<CanvasGroup>();
            Image backdrop = rootRect.gameObject.AddComponent<Image>();
            backdrop.color = new Color(0.03f, 0.04f, 0.08f, 0.6f);

            window = Rect(rootRect, "Window", new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(560f, 600f));
            Image paper = SlicedImage(window, "Paper", Load("Banner"), Vector2.zero, new Vector2(560f, 600f), 1f);
            Stretch(paper.rectTransform);

            RectTransform ribbon = SlicedImage(window, "Ribbon", Load("Ribbon_Blue"), new Vector2(0f, 290f), new Vector2(620f, 128f), 1f).rectTransform;
            Text title = CreateText(ribbon, "Text", font, 64, Cream, new Vector2(0.5f, 0.5f), new Vector2(0f, 4f), new Vector2(420f, 100f));
            title.fontStyle = FontStyle.Bold;
            title.text = "PAUSED";

            resumeButton = CreateButton(window, font, "RESUME", "Icon_Play", "Button_Blue", "Sword_Blue", 110f);
            menuButton = CreateButton(window, font, "MAIN MENU", "Icon_Back", "Button_Blue", "Sword_Blue", -20f);
            quitButton = CreateButton(window, font, "QUIT", "Icon_Quit", "Button_Red", "Sword_Red", -150f);
            LinkVertical(resumeButton.Button, menuButton.Button, quitButton.Button);
            resumeButton.Button.onClick.AddListener(OnResume);
            menuButton.Button.onClick.AddListener(OnMainMenu);
            quitButton.Button.onClick.AddListener(OnQuit);

            RectTransform fadeRect = Stretched(canvasTransform, "Fade");
            Image fadeImage = fadeRect.gameObject.AddComponent<Image>();
            fadeImage.color = Color.black;
            fadeImage.raycastTarget = false;
            fade = fadeRect.gameObject.AddComponent<CanvasGroup>();
            fade.alpha = 0f;
            fade.blocksRaycasts = false;
        }

        private static MenuButton CreateButton(Transform parent, Font font, string labelText, string iconName, string spriteName, string swordName, float y)
        {
            var size = new Vector2(430f, 116f);
            RectTransform rootRect = Rect(parent, labelText, new Vector2(0.5f, 0.5f), new Vector2(0f, y), size);
            var hitArea = rootRect.gameObject.AddComponent<Image>();
            hitArea.color = Color.clear;
            var button = rootRect.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = hitArea;

            Image swordImage = SlicedImage(rootRect, "Sword", Load(swordName), Vector2.zero, new Vector2(170f, 64f), 2f);
            RectTransform sword = swordImage.rectTransform;
            sword.anchorMin = sword.anchorMax = new Vector2(0f, 0.5f);
            sword.pivot = new Vector2(1f, 0.5f);
            sword.anchoredPosition = new Vector2(4f, 0f);
            var swordGroup = sword.gameObject.AddComponent<CanvasGroup>();
            swordGroup.blocksRaycasts = false;
            swordGroup.alpha = 0f;

            Sprite regular = Load(spriteName), pressed = Load(spriteName + "_Pressed");
            Image background = SlicedImage(rootRect, "Visual", regular, Vector2.zero, size, 1.3f);
            RectTransform content = Rect(background.rectTransform, "Content", new Vector2(0.5f, 0.5f), Vector2.zero, size);

            var iconImage = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            RectTransform icon = iconImage.rectTransform;
            icon.SetParent(content, false);
            icon.anchorMin = icon.anchorMax = new Vector2(0f, 0.5f);
            icon.sizeDelta = new Vector2(64f, 64f);
            icon.anchoredPosition = new Vector2(66f, 4f);
            iconImage.sprite = Load(iconName);
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            Text label = CreateText(content, "Label", font, 42, Color.white, new Vector2(0.5f, 0.5f), new Vector2(30f, 4f), new Vector2(320f, 70f));
            label.fontStyle = FontStyle.Bold;
            label.text = labelText;

            var menuButton = rootRect.gameObject.AddComponent<MenuButton>();
            menuButton.Configure(background.rectTransform, background, regular, pressed, content, icon, label, sword, swordGroup);
            return menuButton;
        }

        private static void LinkVertical(params Button[] buttons)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].navigation = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = buttons[(i - 1 + buttons.Length) % buttons.Length],
                    selectOnDown = buttons[(i + 1) % buttons.Length],
                };
            }
        }

        // Levels have no EventSystem of their own (the HUD is display-only).
        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null || FindAnyObjectByType<EventSystem>() != null)
                return;
            var go = new GameObject("EventSystem", typeof(EventSystem));
            go.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static Sprite Load(string name)
        {
            Sprite sprite = Resources.Load<Sprite>(ArtFolder + name);
            if (sprite == null)
                Debug.LogWarning($"PauseMenu: sprite Resources/{ArtFolder}{name} is missing.");
            return sprite;
        }

        private static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return rect;
        }

        private static RectTransform Stretched(Transform parent, string name)
        {
            RectTransform rect = Rect(parent, name, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Stretch(rect);
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Image SlicedImage(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size, float pixelsPerUnitMultiplier)
        {
            RectTransform rect = Rect(parent, name, new Vector2(0.5f, 0.5f), position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = sprite != null && sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            image.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(Transform parent, string name, Font font, int size, Color color, Vector2 anchor, Vector2 position, Vector2 box)
        {
            RectTransform rect = Rect(parent, name, anchor, position, box);
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            var text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = Ink;
            outline.effectDistance = new Vector2(3f, -3f);
            var shadow = rect.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(Ink.r, Ink.g, Ink.b, 0.6f);
            shadow.effectDistance = new Vector2(0f, -6f);
            return text;
        }

        private static IEnumerator Tween(float duration, System.Action<float> apply)
        {
            for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
            {
                apply(time / duration);
                yield return null;
            }
            apply(1f);
        }

        private static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }
    }
}
