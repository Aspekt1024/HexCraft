using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aspekt.Hex.UI
{
    public abstract class TooltipElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public interface IObserver
        {
            void TooltipItemEnter(TooltipElement item);
            void TooltipItemExit(TooltipElement item);
            void TooltipItemClicked(TooltipElement item);
        }
        
        private readonly List<IObserver> observers = new List<IObserver>();

        public void RegisterObserver(IObserver observer)
        {
            observers.Add(observer);
        }

        public abstract Tooltip.Details GetTooltipDetails(int playerId);

        public void OnPointerEnter(PointerEventData eventData)
        {
            observers.ForEach(o => o.TooltipItemEnter(this));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            observers.ForEach(o => o.TooltipItemExit(this));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            observers.ForEach(o => o.TooltipItemClicked(this));
        }
    }
}