using System.Collections;
using UnityEngine;

namespace Mossbound.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
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
