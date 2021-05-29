using System;
using System.Linq;
using System.Reflection;
using Aspekt.Hex.Actions;
using Aspekt.Hex.Config;
using UnityEditor;
using UnityEngine;

namespace Aspekt.Hex.Upgrades
{
    public static class NodeDependencies
    {
        public static bool CreateDependency(Node from, Node to, TechConfig techConfig, UpgradeDependencyMode mode)
        {
            if (from.GetHashCode() == to.GetHashCode()) return false;

            switch (mode)
            {
                case UpgradeDependencyMode.AddAction:
                    return AddAction(from, to, techConfig);
                case UpgradeDependencyMode.CreateTechRequirement:
                    return AddTechRequirement(from, to, techConfig);
                case UpgradeDependencyMode.RemoveAction:
                    return RemoveAction(from, to, techConfig);
                case UpgradeDependencyMode.RemoveTechRequirement:
                    return RemoveTechRequirement(from, to, techConfig);
                default:
                    Debug.LogWarning($"Unhandled dependency mode: {mode}");
                    return false;
            }
        }

        private static bool AddAction(Node from, Node to, TechConfig techConfig)
        {
            switch (from)
            {
                case CellNode fromCell:
                    switch (to)
                    {
                        case CellNode _:
                            return AddActionToCell(fromCell.GetCell(), to.GetAction(techConfig));
                        case UpgradeGroupNode _:
                            return AddActionToCell(fromCell.GetCell(), to.GetAction(techConfig));
                        default:
                            Debug.LogWarning($"Unable to add action of type {to.GetType().Name} to cell.");
                            return false;
                    }
                default:
                    Debug.LogWarning($"Add action failed. Only cell nodes can have actions. Selected node was {from.GetType().Name}");
                    return false;
            }
        }

        private static bool RemoveAction(Node from, Node to, TechConfig techConfig)
        {
            switch (from)
            {
                case CellNode fromCell:
                    switch (to)
                    {
                        case CellNode _:
                            return RemoveActionFromCell(fromCell.GetCell(), to.GetAction(techConfig));
                        case UpgradeGroupNode _:
                            return RemoveActionFromCell(fromCell.GetCell(), to.GetAction(techConfig));
                        default:
                            Debug.LogWarning($"Unable to remove action of type {to.GetType().Name} from cell.");
                            return false;
                    }
                default:
                    Debug.LogWarning($"Remove action failed. Only cell nodes can have actions. Selected node was {from.GetType().Name}");
                    return false;
            }
        }

        private static bool AddTechRequirement(Node from, Node to, TechConfig techConfig)
        {
            switch (from)
            {
                case CellNode fromCell:
                    switch (to)
                    {
                        case CellNode _:
                            return AddRequirementToAction(to.GetAction(techConfig), fromCell.GetCell().Technology);
                        case UpgradeGroupNode _:
                            Debug.LogWarning("Cannot add tech requirements to upgrade groups. This is handled by the sub upgrade itself.");
                            return false;
                        case UpgradeSubNode upgradeSubNode:
                            var action = (UpgradeAction) to.GetAction(techConfig);
                            var dependentTech = upgradeSubNode.GetTechnology();
                            var requiredTech = fromCell.GetCell().Technology;
                            return AddTechRequirementToUpgrade(action, dependentTech, requiredTech);
                        default:
                            Debug.LogError($"Unable to add tech requirement to node type {to.GetType().Name}");
                            return false;
                    }
                case UpgradeGroupNode _:
                    Debug.LogWarning($"{nameof(UpgradeGroupNode)} cannot be a tech dependency. Sub upgrades can be dependencies.");
                    return false;
                case UpgradeSubNode fromUpgrade:
                    switch (to)
                    {
                        case CellNode _:
                            return AddRequirementToAction(to.GetAction(techConfig), fromUpgrade.GetTechnology());
                        case UpgradeGroupNode _:
                            Debug.LogWarning("Cannot add tech requirements to upgrade groups. This is handled by the sub upgrade itself.");
                            return false;
                        case UpgradeSubNode toUpgrade:
                            var action = (UpgradeAction) to.GetAction(techConfig);
                            var dependentTech = toUpgrade.GetTechnology();
                            var requiredTech = fromUpgrade.GetTechnology();
                            return AddTechRequirementToUpgrade(action, dependentTech, requiredTech);
                        default:
                            Debug.LogError($"Unable to add tech requirement to node type {to.GetType().Name}");
                            return false;
                    }
                default:
                    Debug.LogWarning($"Add dependency failed. From {from.GetType().Name} to {from.GetType().Name}");
                    return false;
            }
        }
        
        private static bool RemoveTechRequirement(Node from, Node to, TechConfig techConfig)
        {
            switch (from)
            {
                case CellNode fromCell:
                    switch (to)
                    {
                        case CellNode _:
                            return RemoveRequirementFromAction(to.GetAction(techConfig), fromCell.GetCell().Technology);
                        case UpgradeGroupNode _:
                            Debug.LogWarning("Cannot add tech requirements to upgrade groups. This is handled by the sub upgrade itself.");
                            return false;
                        case UpgradeSubNode upgradeSubNode:
                            var action = (UpgradeAction) to.GetAction(techConfig);
                            var dependentTech = upgradeSubNode.GetTechnology();
                            var requiredTech = fromCell.GetCell().Technology;
                            return RemoveTechRequirementFromUpgrade(action, dependentTech, requiredTech);
                        default:
                            Debug.LogError($"Unable to add tech requirement to node type {to.GetType().Name}");
                            return false;
                    }
                case UpgradeGroupNode _:
                    Debug.LogWarning($"{nameof(UpgradeGroupNode)} cannot be a tech dependency. Sub upgrades can be dependencies.");
                    return false;
                case UpgradeSubNode fromUpgrade:
                    switch (to)
                    {
                        case CellNode _:
                            return RemoveRequirementFromAction(to.GetAction(techConfig), fromUpgrade.GetTechnology());
                        case UpgradeGroupNode _:
                            Debug.LogWarning("Cannot add tech requirements to upgrade groups. This is handled by the sub upgrade itself.");
                            return false;
                        case UpgradeSubNode toUpgrade:
                            var action = (UpgradeAction) to.GetAction(techConfig);
                            var dependentTech = toUpgrade.GetTechnology();
                            var requiredTech = fromUpgrade.GetTechnology();
                            return RemoveTechRequirementFromUpgrade(action, dependentTech, requiredTech);
                        default:
                            Debug.LogError($"Unable to add tech requirement to node type {to.GetType().Name}");
                            return false;
                    }
                default:
                    Debug.LogWarning($"Add dependency failed. From {from.GetType().Name} to {from.GetType().Name}");
                    return false;
            }
        }
        
        private static bool AddActionToCell(HexCell cell, ActionDefinition action)
        {
            var fieldInfo = typeof(HexCell).GetField("actions", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null) return false;
            
            var actionList = ((ActionDefinition[]) fieldInfo.GetValue(cell)).ToList();
            if (actionList.Contains(action)) return false;
            
            Undo.RecordObject(cell, "Add cell action");
            actionList.Add(action);
            fieldInfo.SetValue(cell, actionList.ToArray());
            
            return true;
        }

        private static bool RemoveActionFromCell(HexCell cell, ActionDefinition action)
        {
            var fieldInfo = typeof(HexCell).GetField("actions", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null) return false;
            
            var actionList = ((ActionDefinition[]) fieldInfo.GetValue(cell)).ToList();
            if (!actionList.Contains(action)) return false;
            
            Undo.RecordObject(cell, "Remove cell action");
            actionList.Remove(action);
            fieldInfo.SetValue(cell, actionList.ToArray());
            
            return true;
        }

        private static bool AddRequirementToAction(ActionDefinition action, Technology tech)
        {
            if (action is BuildAction buildAction)
            {
                if (buildAction.techRequirements.Contains(tech)) return false;
                Undo.RecordObject(buildAction, "Add tech requirement");
                buildAction.techRequirements.Add(tech);
                return true;
            }
            
            // Only build actions can have dependencies. Upgrade action dependencies are handled by the
            // individual sub tech.
            
            return false;
        }

        private static bool RemoveRequirementFromAction(ActionDefinition action, Technology tech)
        {
            if (action is BuildAction buildAction)
            {
                if (!buildAction.techRequirements.Contains(tech)) return false;
                Undo.RecordObject(buildAction, "Remove tech requirement");
                buildAction.techRequirements.Remove(tech);
                return true;
            }

            // Only build actions can have dependencies. Upgrade action dependencies are handled by the
            // individual sub tech.

            return false;
        }

        private static bool AddTechRequirementToUpgrade(UpgradeAction action, Technology dependentTech, Technology requiredTech)
        {
            for (int i = 0; i < action.upgradeDetails.Length; i++)
            {
                if (action.upgradeDetails[i].tech == dependentTech)
                {
                    var details = action.upgradeDetails[i];
                    if (details.requiredTech.Contains(requiredTech)) return false;
                    
                    Undo.RecordObject(action, "Add tech requirement");
                    details.requiredTech.Add(requiredTech);
                    action.upgradeDetails[i] = details;
                    
                    return true;
                }
            }
            return false;
        }

        private static bool RemoveTechRequirementFromUpgrade(UpgradeAction action, Technology dependentTech, Technology requiredTech)
        {
            for (int i = 0; i < action.upgradeDetails.Length; i++)
            {
                if (action.upgradeDetails[i].tech == dependentTech)
                {
                    var details = action.upgradeDetails[i];
                    if (!details.requiredTech.Contains(requiredTech)) return false;
                    
                    Undo.RecordObject(action, "Remove tech requirement");
                    details.requiredTech.Remove(requiredTech);
                    action.upgradeDetails[i] = details;
                    
                    return true;
                }
            }
            return false;
        }
    }
}