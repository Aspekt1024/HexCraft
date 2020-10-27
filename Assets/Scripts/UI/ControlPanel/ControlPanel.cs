using Aspekt.Hex.UI.Control;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class ControlPanel : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private Turns turns;
        [SerializeField] private CellControl cellControl;
#pragma warning restore 649
        
        public interface IEventReceiver
        {
            void OnEndTurnRequested();
        }

        private IEventReceiver observer;

        public void RegisterSingleObserver(IEventReceiver observer)
        {
            this.observer = observer;
        }

        public void SetTurnNumber(int turnNumber) => turns.SetTurnNumber(turnNumber);
        public void SetPlayerTurn(bool isPlayerTurn) => turns.SetPlayerTurn(isPlayerTurn);
        public void SetCellSelected(HexCell cell, NetworkGamePlayerHex queryingPlayer) => cellControl.SetCellDetails(cell, queryingPlayer);

        public void EndTurn()
        {
            observer?.OnEndTurnRequested();
        }
    }
}