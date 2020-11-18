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
        [SerializeField] private GameObject suppliesObject;
        [SerializeField] private TextMeshProUGUI suppliesText;
        [SerializeField] private GameObject produceObject;
        [SerializeField] private TextMeshProUGUI produceText;
        [SerializeField] private GameObject actionsObject;
        [SerializeField] private TextMeshProUGUI actionText;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI requirements;
#pragma warning restore 649

        private bool isEnabled;
        private TooltipElement current;
        private float timeEntered;
        private float timeExited;
        private bool isShowTriggered;

        private NetworkGamePlayerHex player;

        private const float ShowHideDelay = 0.2f;

        [Serializable]
        public struct Details
        {
            public readonly string Title;
            public readonly Cost Cost;
            public readonly int ActionCost;
            public readonly string[] Description;
            public readonly string RequirementsText;

            public Details(string title, string[] description)
            {
                Title = title;
                Cost = new Cost();
                ActionCost = 0;
                Description = description;
                RequirementsText = "";
            }
            
            public Details(string title, Cost cost, int actionCost, string[] description)
            {
                Title = title;
                Cost = cost;
                ActionCost = actionCost;
                Description = description;
                RequirementsText = "";
            }

            public Details(string title, string requirements)
            {
                
                Title = title;
                Cost = new Cost();
                ActionCost = 0;
                Description = new string[0];
                RequirementsText = requirements;
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

        public void SetPlayer(NetworkGamePlayerHex player)
        {
            this.player = player;
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
                ShowTooltip(current, player.ID);
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

        private void ShowTooltip(TooltipElement item, int playerId)
        {
            if (isShowTriggered) return;
            
            var tooltipDetails = item.GetTooltipDetails(playerId);
            SetTitle(tooltipDetails);
            SetCost(tooltipDetails);
            SetActions(tooltipDetails);
            SetDescription(tooltipDetails);
            SetRequirements(tooltipDetails);
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
            SetCostObject(suppliesObject, suppliesText, details.Cost.supplies, player.PlayerData.CurrencyData.Supplies);
            SetCostObject(produceObject, produceText, details.Cost.production, player.PlayerData.CurrencyData.AvailableProduction);
        }

        private static void SetCostObject(GameObject obj, TextMeshProUGUI textObj, int cost, int budget)
        {
            if (cost == 0)
            {
                obj.SetActive(false);
            }
            else
            {
                obj.SetActive(true);
                textObj.text = cost <= budget ? cost.ToString() : $"<color=red>{cost}</color>";
            }
        }

        private void SetActions(Details details)
        {
            if (details.ActionCost == 0)
            {
                actionsObject.SetActive(false);
            }
            else
            {
                actionsObject.SetActive(true);
                actionText.text = details.ActionCost + " Action" + (details.ActionCost == 1 ? "" : "s");
            }
        }

        private void SetDescription(Details details)
        {
            if (!details.HasDescription)
            {
                description.gameObject.SetActive(false);
                return;
            }
            
            description.gameObject.SetActive(true);
            
            var descriptionText = "";
            foreach (var desc in details.Description)
            {
                if (descriptionText != "")
                {
                    descriptionText += "\n\n";
                }
                descriptionText += desc;
            }

            description.text = descriptionText;
        }

        private void SetRequirements(Details details)
        {
            if (string.IsNullOrWhiteSpace(details.RequirementsText))
            {
                requirements.gameObject.SetActive(false);
            }
            else
            {
                requirements.gameObject.SetActive(true);
                requirements.text = details.RequirementsText;
            }
        }
    }
}