using System.Collections;
using UnityEngine;

namespace Mossbound.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        [Tooltip("Fade from black when the scene was loaded after a level transition.")]
        [SerializeField, Min(0.01f)] private float sceneFadeInTime = 1f;

        private static bool fadeInOnLoad;

        private CanvasGroup canvasGroup;

        public static void FadeInOnNextLoad() => fadeInOnLoad = true;

        // Static state survives play mode restarts when domain reload is disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => fadeInOnLoad = false;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = fadeInOnLoad ? 1f : 0f;
        }

        private void Start()
        {
            if (!fadeInOnLoad)
                return;

            fadeInOnLoad = false;
            StartCoroutine(Fade(0f, sceneFadeInTime));
        }

        public IEnumerator Fade(float targetAlpha, float duration)
        {
            float startAlpha = canvasGroup.alpha;
            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                yield return null;
            }
            canvasGroup.alpha = targetAlpha;
        }
    }
}
