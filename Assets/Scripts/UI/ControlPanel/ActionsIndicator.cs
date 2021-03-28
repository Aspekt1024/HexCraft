using Aspekt.Hex.UI;

namespace UI.ControlPanel
{
    public class ActionsIndicator : TooltipElement
    {
        private bool isCurrentPlayer;

        public void SetAsCurrentPlayer(bool isCurrentPlayer)
        {
            this.isCurrentPlayer = isCurrentPlayer;
        }
        
        public override Tooltip.Details GetTooltipDetails(int playerId)
        {
            return new Tooltip.Details(
                GetTitle(),
                GetDescription()
            );
        }

        private string GetTitle()
        {
            return isCurrentPlayer
                ? "End turn"
                : "Opponent's turn";
        }

        private string[] GetDescription()
        {
            if (isCurrentPlayer)
            {
                return new[]
                {
                    "Click to end your turn now."
                };
            }
            else
            {
                return new[]
                {
                    "Waiting for your opponent to finish their turn."
                };
            }
        }
    }
}