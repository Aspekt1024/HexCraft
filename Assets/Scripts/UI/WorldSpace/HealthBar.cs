using System;
using System.Collections;
using Aspekt.Hex.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Aspekt.Hex
{
    public class HealthBar : UIElement
    {
#pragma warning disable 649
        [SerializeField] private Image health;
#pragma warning restore 649
        
        public interface IObserver
        {
            void OnHealthbarHidden(HealthBar bar);
        }

        private IObserver observer;

        private Transform tf;

        private Coroutine setHealthRoutine;

        protected override void Awake()
        {
            base.Awake();
            tf = transform;
            health.fillAmount = 1f;
        }

        public void RegisterObserver(IObserver observer)
        {
            this.observer = observer;
        }

        public void SetHealth(Transform cellTf, float prevPercent, float newPercent)
        {
            Show();
            tf.SetParent(cellTf);
            
            tf.localScale = Vector3.one;
            tf.position = cellTf.position + Vector3.forward * 0.8f + Vector3.up * 1.3f;
            
            if (setHealthRoutine != null) StopCoroutine(setHealthRoutine);
            setHealthRoutine = StartCoroutine(SetHealthRoutine(prevPercent, newPercent));
        }

        public void DepleteHealth(Transform cellTf)
        {
            SetHealth(cellTf, health.fillAmount, 0f);
        }

        private IEnumerator SetHealthRoutine(float prevPercent, float newPercent)
        {
            float timer = 0f;
            const float duration = 0.5f;
            
            while (timer < duration)
            {
                timer += Time.deltaTime;
                health.fillAmount = Mathf.Lerp(prevPercent, newPercent, timer / duration);
                yield return null;
            }
            
            if (newPercent > 0.999f || newPercent < 0.001f)
            {
                StartCoroutine(HideAfterDelay());
            }
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            Hide();
            yield return new WaitForSeconds(0.5f);
            observer.OnHealthbarHidden(this);
            gameObject.SetActive(false);
        }
    }
}