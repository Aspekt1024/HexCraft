using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;

namespace Aspekt.Hex.Upgrades
{
    public class TurnCalculatorNode
    {
        public PlayerData PlayerData { get; }
        public List<BuildingCell> Buildings { get; }

        public float Cost => PlayerData.TurnNumber;
        public float F => Cost + h;
        public TurnCalculatorNode PreviousNode { get; }
        
        private float h = .1f;

        public string Action { get; private set; }

        public TurnCalculatorNode(PlayerData currentPlayerData, List<BuildingCell> currentBuildings)
        {
            PlayerData = currentPlayerData.Clone();
            Buildings = currentBuildings.ToList();

            PreviousNode = null;
            Action = "Start";
        }
        
        public TurnCalculatorNode(TurnCalculatorNode originalNode, int numTurnsElapsed)
        {
            PlayerData = originalNode.PlayerData.Clone();
            PlayerData.TurnNumber += numTurnsElapsed; 
            Buildings = originalNode.Buildings;

            for (int i = 0; i < numTurnsElapsed; i++)
            {
                var suppliers = Cells.GetSuppliers(Buildings.Select(b => b as HexCell).ToList());
                var generatedSupplies = suppliers.Sum(c => c.GetSupplies(PlayerData));
                PlayerData.CurrencyData.Supplies += generatedSupplies;
            }

            PreviousNode = originalNode;
            Action = "Next Turn";
        }

        public TurnCalculatorNode(TurnCalculatorNode originalNode, HexCell newCell)
        {
            PlayerData = originalNode.PlayerData.Clone();
            PlayerData.TechnologyData.AddTechnology(newCell.Technology);
            PlayerData.CurrencyData.Supplies -= newCell.Cost.supplies;
            PlayerData.CurrencyData.UtilisedProduction += newCell.Cost.production;
            
            Buildings = originalNode.Buildings.ToList();
            if (newCell is BuildingCell building)
            {
                Buildings.Add(building);
            }

            if (newCell is IProductionGenerator productionGenerator)
            {
                PlayerData.CurrencyData.MaxProduction += productionGenerator.GetProduction();
            }

            PreviousNode = originalNode;
            Action = "Purchase " + newCell.DisplayName;
        }

        public TurnCalculatorNode(TurnCalculatorNode originalNode, UpgradeAction.UpgradeDetails upgrade)
        {
            PlayerData = originalNode.PlayerData.Clone();
            PlayerData.TechnologyData.AddTechnology(upgrade.tech);
            PlayerData.CurrencyData.Supplies -= upgrade.cost.supplies;
            PlayerData.CurrencyData.UtilisedProduction += upgrade.cost.production;

            Buildings = originalNode.Buildings;

            PreviousNode = originalNode;
            Action = "Upgrade " + upgrade.title;
        }
        
        public bool IsEqual(TurnCalculatorNode other)
        {
            return other.PlayerData.HasSameData(PlayerData)
                && other.Buildings.Count == Buildings.Count
                && other.Buildings.All(b => Buildings.Contains(b));
        }
    }
}