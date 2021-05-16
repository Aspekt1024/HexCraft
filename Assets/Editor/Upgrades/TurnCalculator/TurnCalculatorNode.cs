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

        public int InitialSupplies { get; }
        public int InitialProduction { get; }
        public int SuppliesPerTurn { get; }

        public string Action { get; private set; }

        public TurnCalculatorNode(PlayerData currentPlayerData, List<BuildingCell> currentBuildings)
        {
            PlayerData = currentPlayerData.Clone();
            Buildings = currentBuildings.ToList();

            PreviousNode = null;
            Action = "Start";
        }

        public TurnCalculatorNode(TurnCalculatorNode originalNode, HexCell newCell, int numTurnsToAfford)
        {
            PlayerData = originalNode.PlayerData.Clone();
            Buildings = originalNode.Buildings.ToList();
            
            var suppliers = Cells.GetSuppliers(Buildings.Select(b => b as HexCell).ToList());
            InitialSupplies = PlayerData.CurrencyData.Supplies;
            InitialProduction = PlayerData.CurrencyData.AvailableProduction;
            SuppliesPerTurn = suppliers.Sum(c => c.GetSupplies(PlayerData));
            PlayerData.CurrencyData.Supplies += SuppliesPerTurn * numTurnsToAfford;

            PlayerData.TurnNumber += numTurnsToAfford;
            PlayerData.CurrencyData.Supplies -= newCell.Cost.supplies;
            PlayerData.CurrencyData.UtilisedProduction += newCell.Cost.production;
            
            PlayerData.TechnologyData.AddTechnology(newCell.Technology);
            
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

        public TurnCalculatorNode(TurnCalculatorNode originalNode, UpgradeAction.UpgradeDetails upgrade, int numTurnsToAfford)
        {
            PlayerData = originalNode.PlayerData.Clone();
            Buildings = originalNode.Buildings;
            
            var suppliers = Cells.GetSuppliers(Buildings.Select(b => b as HexCell).ToList());
            var generatedSupplies = suppliers.Sum(c => c.GetSupplies(PlayerData));
            PlayerData.CurrencyData.Supplies += generatedSupplies * numTurnsToAfford;
            
            PlayerData.TurnNumber += numTurnsToAfford;
            PlayerData.CurrencyData.Supplies -= upgrade.cost.supplies;
            PlayerData.CurrencyData.UtilisedProduction += upgrade.cost.production;

            PlayerData.TechnologyData.AddTechnology(upgrade.tech);

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