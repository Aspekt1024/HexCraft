using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using UnityEngine;

namespace Aspekt.Hex.Upgrades
{
    public class TurnCalculatorNode
    {
        public PlayerData PlayerData { get; }
        public List<BuildingCell> Buildings { get; }

        public float Cost { get; private set; }
        public float F => Cost + h;
        public TurnCalculatorNode PreviousNode { get; }
        
        private float h = .1f;

        public int InitialSupplies { get; private set; }
        public int InitialProduction { get; private set; }
        public int SuppliesPerTurn { get; private set; }

        public string Action { get; private set; }

        public TurnCalculatorNode(PlayerData currentPlayerData, List<BuildingCell> currentBuildings)
        {
            PlayerData = currentPlayerData.Clone();
            Buildings = currentBuildings.ToList();

            PreviousNode = null;
            Action = "Start";
        }

        public TurnCalculatorNode(TurnCalculatorNode originalNode, HexCell newCell, int numTurnsToAfford, Currency targetCost)
        {
            PlayerData = originalNode.PlayerData.Clone();
            Buildings = originalNode.Buildings.ToList();
            
            ApplyCurrencyModifications(numTurnsToAfford, newCell.Cost);
            
            PlayerData.TechnologyData.AddTechnology(newCell.Technology);
            
            if (newCell is BuildingCell building)
            {
                Buildings.Add(building);
                PlayerData.CurrencyData.MaxProduction += building.GetCurrencyBonus().production;
            }

            PreviousNode = originalNode;
            Action = "Purchase " + newCell.DisplayName;

            CalculateNodeCosts(targetCost);
        }

        public TurnCalculatorNode(TurnCalculatorNode originalNode, UpgradeAction.UpgradeDetails upgrade, int numTurnsToAfford, Currency targetCost)
        {
            PlayerData = originalNode.PlayerData.Clone();
            Buildings = originalNode.Buildings;
            
            ApplyCurrencyModifications(numTurnsToAfford, upgrade.cost);
            PlayerData.TechnologyData.AddTechnology(upgrade.tech);

            PreviousNode = originalNode;
            Action = "Upgrade " + upgrade.title;

            CalculateNodeCosts(targetCost);
        }

        private void ApplyCurrencyModifications(int numTurnsToAfford, Currency cost)
        {
            var suppliers = Cells.GetSuppliers(Buildings.Select(b => b as HexCell).ToList());
            InitialSupplies = PlayerData.CurrencyData.Supplies;
            InitialProduction = PlayerData.CurrencyData.AvailableProduction;
            SuppliesPerTurn = suppliers.Sum(c => c.CalculateSupplies(PlayerData));
            
            PlayerData.CurrencyData.Supplies += SuppliesPerTurn * numTurnsToAfford;
            
            PlayerData.TurnNumber += numTurnsToAfford;
            PlayerData.CurrencyData.Supplies -= cost.supplies;
            PlayerData.CurrencyData.UtilisedProduction += cost.production;
        }

        private void CalculateNodeCosts(Currency targetCost)
        {
            Cost = PlayerData.TurnNumber;
            
            var suppliers = Cells.GetSuppliers(Buildings.Select(b => b as HexCell).ToList());
            var suppliesPerTurn = suppliers.Sum(c => c.CalculateSupplies(PlayerData));
            if (PlayerData.CurrencyData.Supplies >= targetCost.supplies)
            {
                if (PlayerData.CurrencyData.AvailableProduction >= targetCost.production)
                {
                    h = 0;
                }
                else
                {
                    h = 1f / suppliesPerTurn;
                }
            }
            else if (suppliesPerTurn <= 0)
            {
                h = 50;
            }
            else
            {
                h = Mathf.CeilToInt((float)(targetCost.supplies - PlayerData.CurrencyData.Supplies) / suppliesPerTurn);
            }
        }
        
        public bool IsEqual(TurnCalculatorNode other)
        {
            return other.PlayerData.HasSameData(PlayerData)
                && other.Buildings.Count == Buildings.Count
                && other.Buildings.All(b => Buildings.Contains(b));
        }
    }
}