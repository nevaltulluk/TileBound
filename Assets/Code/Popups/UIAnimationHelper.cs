using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Popups
{
    public static class UIAnimationHelper
    {
        private const float DefaultAnimationDuration = 0.3f;
        private const string OpenAnimTrigger = "Open";
        private const string CloseAnimTrigger = "Close";

        public static void SetActiveWithAnimation(GameObject target, bool active, bool useAnimations, MonoBehaviour coroutineRunner)
        {
            if (target == null) return;

            if (!useAnimations)
            {
                target.SetActive(active);
                return;
            }

            if (active)
            {
                OpenWithAnimation(target, coroutineRunner);
            }
            else
            {
                CloseWithAnimation(target, coroutineRunner);
            }
        }

        private static void OpenWithAnimation(GameObject target, MonoBehaviour coroutineRunner)
        {
            // Try using Animator first if available
            Animator animator = target.GetComponent<Animator>();
            if (animator != null && animator.enabled)
            {
                target.SetActive(true);
                animator.SetTrigger(OpenAnimTrigger);
                return;
            }

            // Fallback to coroutine-based animation
            target.SetActive(true);
            coroutineRunner.StartCoroutine(AnimateOpen(target));
        }

        private static void CloseWithAnimation(GameObject target, MonoBehaviour coroutineRunner)
        {
            // Try using Animator first if available
            Animator animator = target.GetComponent<Animator>();
            if (animator != null && animator.enabled)
            {
                animator.SetTrigger(CloseAnimTrigger);
                coroutineRunner.StartCoroutine(DeactivateAfterAnimation(target, animator));
                return;
            }

            // Fallback to coroutine-based animation
            coroutineRunner.StartCoroutine(AnimateClose(target));
        }

        private static IEnumerator AnimateOpen(GameObject target)
        {
            RectTransform rectTransform = target.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            
            // Add CanvasGroup if it doesn't exist (for alpha animation)
            if (canvasGroup == null)
            {
                canvasGroup = target.AddComponent<CanvasGroup>();
            }

            Vector3 initialScale = Vector3.zero;
            float initialAlpha = 0f;
            
            if (rectTransform != null)
            {
                rectTransform.localScale = initialScale;
            }
            canvasGroup.alpha = initialAlpha;

            float elapsed = 0f;
            AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            
            while (elapsed < DefaultAnimationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / DefaultAnimationDuration);
                float curveValue = scaleCurve.Evaluate(t);

                if (rectTransform != null)
                {
                    rectTransform.localScale = Vector3.Lerp(initialScale, Vector3.one, curveValue);
                }
                canvasGroup.alpha = Mathf.Lerp(initialAlpha, 1f, t);

                yield return null;
            }

            // Ensure final state
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
            }
            canvasGroup.alpha = 1f;
        }

        private static IEnumerator AnimateClose(GameObject target)
        {
            RectTransform rectTransform = target.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            
            // If no canvas group exists, just animate scale and deactivate
            if (canvasGroup == null)
            {
                if (rectTransform != null)
                {
                    float animationElapsed = 0f;
                    AnimationCurve animationScaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    
                    while (animationElapsed < DefaultAnimationDuration)
                    {
                        animationElapsed += Time.unscaledDeltaTime;
                        float t = Mathf.Clamp01(animationElapsed / DefaultAnimationDuration);
                        float curveValue = animationScaleCurve.Evaluate(t);
                        rectTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, curveValue);
                        yield return null;
                    }
                }
                target.SetActive(false);
                yield break;
            }

            // Animate with canvas group (scale + alpha)
            Vector3 targetScale = Vector3.zero;
            float targetAlpha = 0f;
            float elapsed = 0f;
            AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            
            while (elapsed < DefaultAnimationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / DefaultAnimationDuration);
                float curveValue = scaleCurve.Evaluate(t);

                if (rectTransform != null)
                {
                    rectTransform.localScale = Vector3.Lerp(Vector3.one, targetScale, curveValue);
                }
                canvasGroup.alpha = Mathf.Lerp(1f, targetAlpha, t);

                yield return null;
            }

            // Ensure final state
            if (rectTransform != null)
            {
                rectTransform.localScale = targetScale;
            }
            canvasGroup.alpha = targetAlpha;
            
            target.SetActive(false);
        }

        private static IEnumerator DeactivateAfterAnimation(GameObject target, Animator animator)
        {
            // Wait for the close animation to complete
            // This is a simple approach - you might want to use Animator state info for more accuracy
            yield return new WaitForSeconds(DefaultAnimationDuration);
            
            // Only deactivate if the animator is still playing the close animation
            // Otherwise just deactivate
            target.SetActive(false);
        }
    }
}

