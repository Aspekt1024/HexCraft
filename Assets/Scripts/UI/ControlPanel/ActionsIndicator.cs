using Aspekt.Hex.UI;

namespace UI.ControlPanel
{
    public class ActionsIndicator : TooltipElement
    {
        private bool isCurrentPlayer;
        private int numActionsRemaining;
        
        public void SetTurnDetails(bool isCurrentPlayer, int numActions)
        {
            this.isCurrentPlayer = isCurrentPlayer;
            numActionsRemaining = numActions;
        }
        
        public override Tooltip.Details GetTooltipDetails()
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
                    "You have " + numActionsRemaining + " actions remaining.",
                    "Click to end your turn now."
                };
            }
            else
            {
                return new[]
                {
                    "Your opponent is taking their turn."
                };
            }
        }
    }
}