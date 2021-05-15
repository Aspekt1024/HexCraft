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
            gamePlan = null;
            if (cell == null) return;
            
            var homeCell = (BuildingCell)cells.GetPrefab(Cells.CellTypes.Base);
            var playerData = CreatePlayerData(homeCell as HomeCell);
            var buildings = new List<BuildingCell> { homeCell };

            if (cell is BuildingCell buildingCell && buildings.Contains(buildingCell)) return;
            if (playerData.TechnologyData.HasTechnology(cell.Technology)) return;

            openSet.Clear();
            closedSet.Clear();
            targetCell = cell;
            
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
                    if (neighbour.PlayerData.TechnologyData.HasTechnology(targetCell.Technology))
                    {
                        return ConstructGamePlan(neighbour);
                    }
                    AddToOpenSet(neighbour);
                }
                closedSet.Add(node);
            }

            Debug.LogWarning("Failed to find game plan for " + targetCell.DisplayName);
            return null;
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
            var consecutiveNextTurns = 0;
            var prevNode = node;
            while (prevNode.PreviousNode != null && prevNode.PreviousNode.PlayerData.TurnNumber < prevNode.PlayerData.TurnNumber)
            {
                prevNode = prevNode.PreviousNode;
                consecutiveNextTurns++;
                if (consecutiveNextTurns > 2) return;
            }
            
            if (node.PlayerData.TurnNumber > 12) return;
            
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
                    if (!(targetCell is UnitCell) && action is BuildAction {prefab: UnitCell _}) continue;
                    var newNode = TryCreateNewNode(node, action);
                    if (newNode != null)
                    {
                        neighbours.Add(newNode);
                    }
                }
            }
            
            neighbours.Add(new TurnCalculatorNode(node, 1));
            return neighbours;
        }

        private TurnCalculatorNode TryCreateNewNode(TurnCalculatorNode origin, ActionDefinition action)
        {
            if (action is BuildAction buildAction)
            {
                if (origin.Buildings.Contains(buildAction.prefab)) return null;
                if (origin.PlayerData.TechnologyData.HasTechnology(buildAction.prefab.Technology)) return null;
                if (!origin.PlayerData.TechnologyData.HasTechnologies(buildAction.techRequirements)) return null;
                if (!origin.PlayerData.CurrencyData.CanAfford(buildAction.prefab.Cost)) return null;

                return new TurnCalculatorNode(origin, buildAction.prefab);
            }
            else if (action is UpgradeAction upgradeAction)
            {
                try
                {
                    var nextUpgrade = upgradeAction.upgradeDetails.First(u => !origin.PlayerData.TechnologyData.HasTechnology(u.tech));
                    if (origin.PlayerData.TechnologyData.HasTechnology(nextUpgrade.tech)) return null;
                    if (!origin.PlayerData.TechnologyData.HasTechnologies(nextUpgrade.requiredTech)) return null;
                    if (!origin.PlayerData.CurrencyData.CanAfford(nextUpgrade.cost)) return null;

                    return new TurnCalculatorNode(origin, nextUpgrade);
                }
                catch
                {
                    
                }
            }

            return null;
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