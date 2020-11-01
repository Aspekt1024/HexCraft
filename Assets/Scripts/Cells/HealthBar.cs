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
        private HexCamera mainCam;
        private Transform cellTf;

        private Coroutine setHealthRoutine;

        protected override void Awake()
        {
            base.Awake();
            tf = transform;
            mainCam = FindObjectOfType<HexCamera>();
        }

        public void RegisterObserver(IObserver observer)
        {
            this.observer = observer;
        }

        public void SetHealth(Transform cellTf, float prevPercent, float newPercent)
        {
            this.cellTf = cellTf;
            Show();
            
            if (setHealthRoutine != null) StopCoroutine(setHealthRoutine);
            setHealthRoutine = StartCoroutine(SetHealthRoutine(prevPercent, newPercent));
        }

        private void LateUpdate()
        {
            if (cellTf == null) return;
            var pos = cellTf.position + Vector3.forward * 1f;
            tf.position = mainCam.Camera.WorldToScreenPoint(pos);
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
            gameObject.SetActive(false);
        }
    }
}