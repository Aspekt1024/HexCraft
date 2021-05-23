using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Util;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aspekt.Hex.Upgrades
{
    public class TurnCalculator
    {
        private readonly GameConfig config;
        private readonly Cells cells;

        private readonly List<TurnCalculatorNode> openSet = new List<TurnCalculatorNode>();
        private readonly List<TurnCalculatorNode> closedSet = new List<TurnCalculatorNode>();
        
        private HexCell targetCell;
        private UpgradeAction.UpgradeDetails targetUpgrade;

        private Queue<TurnCalculatorNode> gamePlan;
        
        public TurnCalculator()
        {
            var game = Object.FindObjectOfType<GameManager>();
            config = InspectorUtil.GetPrivateValue<GameConfig, GameManager>("config", game);
            cells = Object.FindObjectOfType<Cells>();
            config.Init(cells);
        }
        
        public void QueueCalculation(HexCell cell, Action onCompleteCallback)
        {
            if (cell == null) return;
            var nullUpgradeDetails = new UpgradeAction.UpgradeDetails();
            SetupAndCalculateGamePlan(cell, nullUpgradeDetails, onCompleteCallback);
        }

        public void QueueCalculation(UpgradeAction.UpgradeDetails upgrade, Action onCompleteCallback)
        {
            if (upgrade.tech == Technology.None) return;
            SetupAndCalculateGamePlan(null, upgrade, onCompleteCallback);
        }

        private void SetupAndCalculateGamePlan(HexCell cell, UpgradeAction.UpgradeDetails upgrade, Action onCompleteCallback)
        {
            targetCell = cell;
            targetUpgrade = upgrade;
            
            var homeCell = (BuildingCell)cells.GetPrefab(Cells.CellTypes.Base);
            var playerData = CreatePlayerData(homeCell as HomeCell);
            var buildings = new List<BuildingCell> { homeCell };

            if (targetCell != null)
            {
                if (targetCell is BuildingCell buildingCell && buildings.Contains(buildingCell)) return;
                if (playerData.TechnologyData.HasTechnology(targetCell.Technology)) return;
            }
            else if (targetUpgrade.tech != Technology.None)
            {
                if (playerData.TechnologyData.HasTechnology(targetUpgrade.tech)) return;
            }
            else
            {
                return;
            }

            openSet.Clear();
            closedSet.Clear();
            
            openSet.Add(new TurnCalculatorNode(playerData, buildings));

            gamePlan = CalculateGamePlan();
            onCompleteCallback?.Invoke();
        }

        public Queue<TurnCalculatorNode> GetCalculatedPlan() => gamePlan;

        private Queue<TurnCalculatorNode> CalculateGamePlan()
        {
            var iterations = 0;
            while (openSet.Any())
            {
                iterations++;
                if (iterations > 4000) break;
                
                var node = GetCheapestOpenNode();
                var neighbours = GetNeighbouringNodes(node);
                foreach (var neighbour in neighbours)
                {
                    if (NodeAchievesGamePlan(neighbour))
                    {
                        return ConstructGamePlan(neighbour);
                    }
                    AddToOpenSet(neighbour);
                }
                closedSet.Add(node);
            }

            if (targetCell != null)
            {
                Debug.LogWarning("Failed to find game plan for " + targetCell.DisplayName);
            }
            else
            {
                Debug.LogWarning("Failed to find game plan for " + targetUpgrade.title);
            }
            return null;
        }

        private bool NodeAchievesGamePlan(TurnCalculatorNode node)
        {
            return node.PlayerData.TechnologyData.HasTechnology(targetCell != null ? targetCell.Technology : targetUpgrade.tech);
        }

        private Queue<TurnCalculatorNode> ConstructGamePlan(TurnCalculatorNode node)
        {
            var queue = new Stack<TurnCalculatorNode>();

            while (node != null)
            {
                queue.Push(node);
                node = node.PreviousNode;
            }

            var gamePlan = new Queue<TurnCalculatorNode>();
            while (queue.Any())
            {
                gamePlan.Enqueue(queue.Pop());
            }
            
            return gamePlan;
        }

        private TurnCalculatorNode GetCheapestOpenNode()
        {
            openSet.Sort((n1, n2) => n1.F.CompareTo(n2.F));
            var node = openSet[0];
            openSet.RemoveAt(0);
            return node;
        }

        private void AddToOpenSet(TurnCalculatorNode node)
        {
            if (node.PlayerData.TurnNumber > 50) return;
            
            if (openSet.Any(openNode => openNode.IsEqual(node) && openNode.F < node.F)) return;
            if (closedSet.Any(closedNode => closedNode.IsEqual(node) && closedNode.F < node.F)) return;
            openSet.Add(node);
        }

        private List<TurnCalculatorNode> GetNeighbouringNodes(TurnCalculatorNode node)
        {
            var neighbours = new List<TurnCalculatorNode>();
            foreach (var building in node.Buildings)
            {
                foreach (var action in building.Actions)
                {
                    var newNode = TryCreateNewNode(node, action);
                    if (newNode != null)
                    {
                        neighbours.Add(newNode);
                    }
                }
            }
            
            return neighbours;
        }

        private TurnCalculatorNode TryCreateNewNode(TurnCalculatorNode origin, ActionDefinition action)
        {
            if (action is BuildAction buildAction)
            {
                if (origin.Buildings.Contains(buildAction.prefab)) return null;
                if (origin.PlayerData.TechnologyData.HasTechnology(buildAction.prefab.Technology)) return null;
                if (!origin.PlayerData.TechnologyData.HasTechnologies(buildAction.techRequirements)) return null;

                var turnCount = GetTurnsUntilAffordable(origin, buildAction.prefab.Cost);
                if (turnCount < 0) return null;

                var costForHeuristic = targetCell == null ? targetUpgrade.cost : targetCell.Cost;
                return new TurnCalculatorNode(origin, buildAction.prefab, turnCount, costForHeuristic);
            }
            else if (action is UpgradeAction upgradeAction)
            {
                UpgradeAction.UpgradeDetails nextUpgrade;
                try
                {
                    nextUpgrade = upgradeAction.upgradeDetails.First(u => !origin.PlayerData.TechnologyData.HasTechnology(u.tech));
                }
                catch
                {
                    return null;
                }
                
                if (origin.PlayerData.TechnologyData.HasTechnology(nextUpgrade.tech)) return null;
                if (!origin.PlayerData.TechnologyData.HasTechnologies(nextUpgrade.requiredTech)) return null;
                    
                var turnCount = GetTurnsUntilAffordable(origin, nextUpgrade.cost);
                if (turnCount < 0) return null;

                var costForHeuristic = targetCell == null ? targetUpgrade.cost : targetCell.Cost;
                return new TurnCalculatorNode(origin, nextUpgrade, turnCount, costForHeuristic);
            }

            return null;
        }

        private static int GetTurnsUntilAffordable(TurnCalculatorNode node, Cost cost)
        {
            if (node.PlayerData.CurrencyData.CanAfford(cost)) return 0;
            if (node.PlayerData.CurrencyData.AvailableProduction < cost.production) return -1;
            
            var suppliers = Cells.GetSuppliers(node.Buildings.Select(b => b as HexCell).ToList());
            var suppliesPerTurn = suppliers.Sum(c => c.GetSupplies(node.PlayerData));
            if (suppliesPerTurn <= 0) return -1;

            var requiredSupplies = cost.supplies - node.PlayerData.CurrencyData.Supplies;
            var numTurns = Mathf.CeilToInt((float)requiredSupplies / suppliesPerTurn);
                
            const float maxTurnCount = 10;
            if (numTurns > maxTurnCount) return -1;

            return numTurns;
        }

        private PlayerData CreatePlayerData(HomeCell homeCell)
        {
            var playerData = new PlayerData(null);
            playerData.Init(config);
            
            playerData.TurnNumber = 1;
            playerData.TechnologyData.AddTechnology(homeCell.Technology);
            playerData.CurrencyData.Supplies = config.startingSupply;
            playerData.CurrencyData.MaxProduction = homeCell.production;

            return playerData;
        }
    }
}