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

        public void Init(Transform cellTf, float initialHealthPercent)
        {
            this.cellTf = cellTf;
            health.fillAmount = initialHealthPercent;
        }

        public void SetHealth(float percent)
        {
            if (setHealthRoutine != null) StopCoroutine(setHealthRoutine);
            setHealthRoutine = StartCoroutine(SetHealthRoutine(percent));
        }

        private void LateUpdate()
        {
            if (cellTf == null) return;
            var pos = cellTf.position + Vector3.forward * 1f;
            tf.position = mainCam.Camera.WorldToScreenPoint(pos);
        }

        private IEnumerator SetHealthRoutine(float newPercent)
        {
            Show();
            
            var originalPercent = health.fillAmount;
            float timer = 0f;
            const float duration = 0.5f;
            
            while (timer < duration)
            {
                timer += Time.deltaTime;
                health.fillAmount = Mathf.Lerp(originalPercent, newPercent, timer / duration);
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