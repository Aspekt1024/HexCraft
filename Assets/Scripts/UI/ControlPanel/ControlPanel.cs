using System;
using Aspekt.Hex.UI.Control;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aspekt.Hex.UI
{
    public class ControlPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
#pragma warning disable 649
        [SerializeField] private Turns turns;
        [SerializeField] private CellControl cellControl;
#pragma warning restore 649
        
        public interface IEventReceiver
        {
            void OnEndTurnRequested();
            void SetCursorInUI(bool isInUI);
        }

        private IEventReceiver observer;

        public void RegisterSingleObserver(IEventReceiver observer)
        {
            this.observer = observer;
        }

        public void SetPlayerTurn(PlayerData playerData)
        {
            if (playerData == null)
            {
                turns.SetPlayerTurn(null);
                return;
            }
            turns.SetTurnNumber(playerData.TurnNumber);
            turns.SetPlayerTurn(playerData.Player);
        }

        public void SetCellSelected(HexCell cell, NetworkGamePlayerHex queryingPlayer) => cellControl.SetCellDetails(cell, queryingPlayer);

        public void EndTurn()
        {
            observer?.OnEndTurnRequested();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            observer?.SetCursorInUI(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            observer?.SetCursorInUI(false);
        }
    }
}