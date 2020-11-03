using Aspekt.Hex.UI.Control;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aspekt.Hex.UI
{
    public class ControlPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
#pragma warning disable 649
        [SerializeField] private CellControl cellControl;
#pragma warning restore 649
        
        public interface IEventReceiver
        {
            void SetCursorInUI(MonoBehaviour caller, bool isInUI);
        }

        private IEventReceiver observer;

        public void Init(Tooltip tooltip)
        {
            cellControl.Init(tooltip);
        }
        
        public void RegisterSingleObserver(IEventReceiver observer)
        {
            this.observer = observer;
        }

        public void SetCellSelected(HexCell cell, NetworkGamePlayerHex queryingPlayer) => cellControl.SetCellDetails(cell, queryingPlayer);

        public void OnPointerEnter(PointerEventData eventData)
        {
            observer?.SetCursorInUI(this, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            observer?.SetCursorInUI(this, false);
        }
    }
}