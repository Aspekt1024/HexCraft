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

            switch (from)
            {
                case CellNode fromCell:
                    switch (to)
                    {
                        case UpgradeGroupNode toUpgrade:
                            return HandleUpgradeDependency(fromCell.GetCell(), toUpgrade, techConfig, mode);
                        case CellNode _:
                            return HandleCellActionModification(fromCell.GetCell(), to.GetAction(techConfig), mode);
                        default:
                            Debug.LogWarning($"Unhandled dependency creation: cell node to {to}");
                            return false;
                    }
                case UpgradeGroupNode upgradeGroupNode:
                    return HandleUpgradeDependencyModification(upgradeGroupNode, to.GetAction(techConfig), mode);
                default:
                    Debug.LogWarning($"Unhandled dependency creation: {from}");
                    return false;
            }
        }

        private static bool HandleUpgradeDependency(HexCell cell, UpgradeGroupNode toUpgrade, TechConfig techConfig, UpgradeDependencyMode mode)
        {
            var toSubNode = toUpgrade.GetDependentSubNode();
            if (toSubNode == null)
            {
                return HandleCellActionModification(cell, toUpgrade.GetAction(techConfig), mode);
            }

            var upgrade = toUpgrade.GetUpgradeAction();
            var dependentTech = toSubNode.GetTechnology();
            var requiredTech = cell.Technology;
            
            switch (mode)
            {
                case UpgradeDependencyMode.AddAction:
                    // Cannot add sub-upgrade as an action
                    return false;
                case UpgradeDependencyMode.CreateTechRequirement:
                    return AddTechRequirementToUpgrade(upgrade, dependentTech, requiredTech);
                case UpgradeDependencyMode.RemoveAction:
                    // Cannot remove sub-upgrade as an action
                    return false;
                case UpgradeDependencyMode.RemoveTechRequirement:
                    return RemoveTechRequirementFromUpgrade(upgrade, dependentTech, requiredTech);
                default:
                    return false;
            }
        }
        
        private static bool HandleCellActionModification(HexCell cell, ActionDefinition action, UpgradeDependencyMode mode)
        {
            switch (mode)
            {
                case UpgradeDependencyMode.AddAction:
                    return AddActionToCell(cell, action);
                case UpgradeDependencyMode.CreateTechRequirement:
                    return AddRequirementToAction(action, cell.Technology);
                case UpgradeDependencyMode.RemoveAction:
                    return RemoveActionFromCell(cell, action);
                case UpgradeDependencyMode.RemoveTechRequirement:
                    return RemoveRequirementFromAction(action, cell.Technology);
                default:
                    return false;
            }
        }

        private static bool HandleUpgradeDependencyModification(UpgradeGroupNode group, ActionDefinition action, UpgradeDependencyMode mode)
        {
            var tech = group.GetSelectedTech();
            if (tech == Technology.None) return false;
            
            switch (mode)
            {
                case UpgradeDependencyMode.CreateTechRequirement:
                    return AddRequirementToAction(action, tech);
                case UpgradeDependencyMode.RemoveTechRequirement:
                    return RemoveRequirementFromAction(action, tech);
                case UpgradeDependencyMode.AddAction:
                    // Not applicable - upgrades don't have actions
                    return false;
                case UpgradeDependencyMode.RemoveAction:
                    // Not applicable - upgrades don't have actions
                    return false;
                default:
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