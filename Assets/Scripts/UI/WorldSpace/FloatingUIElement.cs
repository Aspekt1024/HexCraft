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
        [SerializeField] private TextMeshProUGUI normalText;
        [SerializeField] private TextMeshProUGUI combatText;
        [SerializeField] private Image image;
        [SerializeField] private Animator animator;

        [Header("Icons")]
        [SerializeField] private Sprite suppliesIcon;
        [SerializeField] private Sprite produceIcon;
#pragma warning restore 649

        private IObserver observer;
        private float timeStarted;

        public void RegisterObserver(IObserver observer) => this.observer = observer;

        public void Begin(Transform targetTf, string textString, FloatingUI.Style style = FloatingUI.Style.None, Sprite icon = null)
        {
            image.sprite = icon;
            if (style == FloatingUI.Style.Produce)
            {
                image.sprite = produceIcon;
            }
            else if (style == FloatingUI.Style.Supplies)
            {
                image.sprite = suppliesIcon;
            }
            image.gameObject.SetActive(image.sprite != null);

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
                normalText.gameObject.SetActive(false);
                return;
            }
            
            if (style == FloatingUI.Style.None || style == FloatingUI.Style.Produce || style == FloatingUI.Style.Supplies)
            {
                normalText.text = textString;
                normalText.gameObject.SetActive(true);
                combatText.gameObject.SetActive(false);
            }
            else if (style == FloatingUI.Style.Combat)
            {
                combatText.text = textString;
                normalText.gameObject.SetActive(false);
                combatText.gameObject.SetActive(true);
            }
        }
    }
}