using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class Tooltip : UIElement, TooltipElement.IObserver
    {
#pragma warning disable 649
        [SerializeField] private RectTransform content;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private GameObject costObject;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI[] descriptionFields;
#pragma warning restore 649

        private bool isEnabled;
        private TooltipElement current;
        private float timeEntered;
        private float timeExited;
        private bool isShowTriggered;

        private const float ShowHideDelay = 0.2f;

        [Serializable]
        public struct Details
        {
            public readonly string Title;
            public readonly int CostCurrency1;
            public readonly int CostCurrency2;
            public readonly int CostCurrency3;
            public readonly int ActionCost;
            public readonly string[] Description;

            public Details(string title, int cost1, int cost2, int cost3, int actionCost, string[] description)
            {
                Title = title;
                CostCurrency1 = cost1;
                CostCurrency2 = cost2;
                CostCurrency3 = cost3;
                ActionCost = actionCost;
                Description = description;
            }
            
            public bool IsValid()
            {
                return HasTitle || HasDescription;
            }

            public bool HasTitle => !string.IsNullOrWhiteSpace(Title);
            public bool HasDescription => Description.Any() && !string.IsNullOrWhiteSpace(Description[0]);
        }

        protected override void Awake()
        {
            base.Awake();
            FadeDuration = 0.3f;
        }

        public void Tick()
        {
            if (!isEnabled) return;

            if (current == null)
            {
                if (IsShowing && Time.unscaledTime > timeExited + ShowHideDelay)
                {
                    isShowTriggered = false;
                    Hide();
                }
            }
            else if (Time.unscaledTime > timeEntered + ShowHideDelay)
            {
                ShowTooltip(current);
            }
        }

        public void EnableTooltips()
        {
            isEnabled = true;
            isShowTriggered = false;
        }

        public void DisableTooltips()
        {
            isEnabled = false;
            current = null;
            isShowTriggered = false;
            Hide();
        }
        
        public void TooltipItemEnter(TooltipElement item)
        {
            if (current == item) return;
            current = item;
            timeEntered = Time.unscaledTime;
            isShowTriggered = false;
        }

        public void TooltipItemExit(TooltipElement item)
        {
            current = null;
            timeExited = Time.unscaledTime;
        }

        public void TooltipItemClicked(TooltipElement item)
        {
            current = null;
            timeExited = 0f;
        }

        public override void Show()
        {
            if (isShowTriggered) return;
            
            isShowTriggered = true;

            const float margin = 15f;
            var xPivot = Screen.width - Input.mousePosition.x - margin < content.rect.width ? 1f : 0f;
            content.pivot = new Vector2(xPivot, 0f);
            
            content.localPosition = Vector3.zero;
            transform.position = Input.mousePosition;
            base.Show();
        }

        private void ShowTooltip(TooltipElement item)
        {
            var tooltipDetails = item.GetTooltipDetails();
            SetTitle(tooltipDetails);
            SetCost(tooltipDetails);
            SetDescription(tooltipDetails);
            Show();
        }

        private void SetTitle(Details details)
        {
            if (details.HasTitle)
            {
                title.gameObject.SetActive(true);
                title.text = details.Title;
            }
            else
            {
                title.gameObject.SetActive(false);
            }
        }

        private void SetCost(Details details)
        {
            if (details.CostCurrency1 == 0)
            {
                costObject.SetActive(false);
            }
            else
            {
                costObject.SetActive(true);
                costText.text = details.CostCurrency1.ToString();
            }
        }

        private void SetDescription(Details details)
        {
            // TODO allow any number of items?
            var numDescriptions = details.Description.Length;
            for (int i = 0; i < numDescriptions; i++)
            {
                descriptionFields[i].text = details.Description[i];
            }
            
            for (int i = numDescriptions; i < descriptionFields.Length; i++)
            {
                descriptionFields[i].gameObject.SetActive(false);
            }
        }
    }
}