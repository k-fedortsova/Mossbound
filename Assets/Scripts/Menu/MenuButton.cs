using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mossbound.Menu
{
    // Juicy menu button on top of a regular uGUI Button (its transition is None; all feedback lives here).
    // Hover and keyboard selection are the same state: the pointer selects the button it enters, so mouse
    // and keyboard / gamepad never show two highlighted buttons at once.
    //  - selected: springy grow, a sword slides in from the left, the icon grows, the label turns gold
    //  - pressed: the "pressed" sprite, content sinks a few pixels, the button squashes
    [RequireComponent(typeof(Button))]
    public class MenuButton : MonoBehaviour,
        IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler,
        ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        [Header("Parts")]
        [Tooltip("Everything that scales; kept separate from the root so the hit area does not change size.")]
        [SerializeField] private RectTransform visual;
        [SerializeField] private Image background;
        [SerializeField] private Sprite regularSprite;
        [SerializeField] private Sprite pressedSprite;
        [Tooltip("Icon and label; moved down while the button is held so they sink with the pressed sprite.")]
        [SerializeField] private RectTransform content;
        [SerializeField] private RectTransform icon;
        [SerializeField] private Text label;
        [SerializeField] private RectTransform sword;
        [SerializeField] private CanvasGroup swordGroup;

        [Header("Feel")]
        [SerializeField] private float selectedScale = 1.08f;
        [SerializeField] private float pressedScale = 0.92f;
        [SerializeField] private float stiffness = 320f;
        [SerializeField] private float damping = 14f;
        [SerializeField] private float pressedContentOffset = -9f;
        [SerializeField] private float swordHiddenOffset = -80f;
        [SerializeField] private Color labelColor = Color.white;
        [SerializeField] private Color labelSelectedColor = new Color32(0xff, 0xe0, 0x8a, 0xff);

        private Button button;
        private bool selected;
        private bool pointerDown;
        private float submitFlash;
        private float scale = 1f;
        private float scaleVelocity;
        private float selectBlend;
        private Vector2 contentOrigin;
        private Vector2 swordOrigin;

        // Lazy, because MainMenu.Awake may run before this button's Awake.
        public Button Button => button != null ? button : button = GetComponent<Button>();

        private void Awake()
        {
            button = Button;
            CacheOrigins();
        }

        // For buttons built in code at runtime (the pause menu): AddComponent runs Awake before the parts exist.
        public void Configure(RectTransform visual, Image background, Sprite regularSprite, Sprite pressedSprite,
            RectTransform content, RectTransform icon, Text label, RectTransform sword, CanvasGroup swordGroup)
        {
            this.visual = visual;
            this.background = background;
            this.regularSprite = regularSprite;
            this.pressedSprite = pressedSprite;
            this.content = content;
            this.icon = icon;
            this.label = label;
            this.sword = sword;
            this.swordGroup = swordGroup;
            CacheOrigins();
        }

        private void CacheOrigins()
        {
            if (content != null)
                contentOrigin = content.anchoredPosition;
            if (sword != null)
                swordOrigin = sword.anchoredPosition;
            if (swordGroup != null)
                swordGroup.alpha = 0f;
        }

        private void OnDisable()
        {
            pointerDown = false;
            selected = false;
        }

        // A short kick used when the button is activated, on top of the regular spring.
        public void Punch(float strength = 6f)
        {
            scaleVelocity += strength;
        }

        private void Update()
        {
            if (visual == null)
                return;
            float dt = Mathf.Min(Time.unscaledDeltaTime, 1f / 20f);
            bool interactable = button.IsInteractable();
            submitFlash = Mathf.Max(0f, submitFlash - dt);
            bool pressed = interactable && (pointerDown || submitFlash > 0f);
            bool highlighted = interactable && selected;

            // Damped spring toward the target scale; overshoots a little, which reads as "bouncy".
            float target = pressed ? pressedScale : highlighted ? selectedScale : 1f;
            scaleVelocity += (target - scale) * stiffness * dt;
            scaleVelocity *= Mathf.Exp(-damping * dt);
            scale += scaleVelocity * dt;

            float squash = (scale - 1f) * 0.35f;
            visual.localScale = new Vector3(scale - squash, scale + squash, 1f);

            selectBlend = Mathf.MoveTowards(selectBlend, highlighted ? 1f : 0f, dt * 6f);
            float eased = EaseOutBack(selectBlend);

            if (background != null)
                background.sprite = pressed && pressedSprite != null ? pressedSprite : regularSprite;
            if (content != null)
                content.anchoredPosition = contentOrigin + new Vector2(0f, pressed ? pressedContentOffset : 0f);
            if (label != null)
                label.color = Color.Lerp(labelColor, labelSelectedColor, selectBlend);
            if (icon != null)
            {
                float iconScale = 1f + 0.15f * eased;
                icon.localScale = new Vector3(iconScale, iconScale, 1f);
            }
            if (sword != null)
            {
                sword.anchoredPosition = swordOrigin + new Vector2(Mathf.LerpUnclamped(swordHiddenOffset, 0f, eased), 0f);
                if (swordGroup != null)
                    swordGroup.alpha = selectBlend;
            }
        }

        private static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button.IsInteractable() && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnPointerExit(PointerEventData eventData) => pointerDown = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                pointerDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                pointerDown = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            selected = true;
            scaleVelocity += 2.5f;
        }

        public void OnDeselect(BaseEventData eventData) => selected = false;

        public void OnSubmit(BaseEventData eventData)
        {
            if (button.IsInteractable())
                submitFlash = 0.12f;
        }
    }
}
