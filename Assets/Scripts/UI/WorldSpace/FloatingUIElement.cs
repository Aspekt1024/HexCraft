using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Aspekt.Hex.UI
{
    public class FloatingUIElement : UIElement
    {
        public interface IObserver
        {
            void OnComplete(FloatingUIElement element);
        }
        
#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Image image;
        [SerializeField] private Animator animator;
#pragma warning restore 649

        private IObserver observer;
        private float timeStarted;

        public void RegisterObserver(IObserver observer) => this.observer = observer;

        public void Begin(Transform targetTf, Sprite icon, string textString, FloatingUI.Style style = FloatingUI.Style.None)
        {
            
            image.sprite = icon;
            image.gameObject.SetActive(icon != null);

            SetText(textString, style);
            
            var tf = transform;
            
            var rot = tf.localEulerAngles;
            rot.x = FindObjectOfType<GameManager>().Camera.UICamera.transform.eulerAngles.x;
            tf.localEulerAngles = rot;

            tf.position = targetTf.position;

            Show();
            animator.enabled = true;
        }

        /// <summary>
        /// Called by the animator
        /// </summary>
        public void OnAnimationComplete()
        {
            Hide();
            animator.enabled = false;
        }

        protected override void OnHidden()
        {
            base.OnHidden();
            observer?.OnComplete(this);
        }

        private void SetText(string textString, FloatingUI.Style style)
        {
            if (string.IsNullOrEmpty(textString))
            {
                // TODO turn all text off
                text.gameObject.SetActive(false);
                return;
            }
            
            // TODO style
            if (style == FloatingUI.Style.None)
            {
                text.text = textString;
                text.gameObject.SetActive(true);
            }
        }
    }
}