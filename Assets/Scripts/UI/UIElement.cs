using System.Collections;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIElement : MonoBehaviour
    {
        public bool IsVisible => state != States.Hidden;
        public bool IsHiding => state == States.Hidden || state == States.TransitionToHidden;

        private enum States
        {
            Hidden,
            TransitionToVisible,
            Visible,
            TransitionToHidden,
        }

        private States state;

        protected const float FadeDuration = 0.6f; 
        
        private CanvasGroup group;
        private Coroutine showRoutine;
        private Coroutine hideRoutine;

        protected virtual void Awake()
        {
            group = GetComponent<CanvasGroup>();
            SetHidden();
        }

        public virtual void Show()
        {
            if (state == States.Visible || state == States.TransitionToVisible) return;
            ClearRoutines();
            showRoutine = StartCoroutine(ShowRoutine());
        }

        public virtual void Hide()
        {
            if (state == States.Hidden || state == States.TransitionToHidden) return;
            ClearRoutines();
            hideRoutine = StartCoroutine(HideRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            state = States.TransitionToVisible;
            float timer = FadeDuration * group.alpha;
            while (timer < FadeDuration)
            {
                timer += Time.deltaTime;
                group.alpha = Mathf.Lerp(0f, 1f, timer / FadeDuration);
                yield return null;
            }
            SetVisible();
        }

        private IEnumerator HideRoutine()
        {
            state = States.TransitionToHidden;
            float timer = FadeDuration * (1 - group.alpha);
            while (timer < FadeDuration)
            {
                timer += Time.deltaTime;
                group.alpha = Mathf.Lerp(1f, 0f, timer / FadeDuration);
                yield return null;
            }
            SetHidden();
        }

        private void SetVisible()
        {
            state = States.Visible;
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }

        private void SetHidden()
        {
            state = States.Hidden;
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        private void ClearRoutines()
        {
            if (hideRoutine != null) StopCoroutine(hideRoutine);
            if (showRoutine != null) StopCoroutine(showRoutine);
        }
    }
}