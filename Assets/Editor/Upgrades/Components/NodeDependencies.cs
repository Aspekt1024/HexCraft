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
            
            var fromObj = from.GetObject();
            var toObj = to.GetObject();

            if (fromObj is ActionDefinition fromAction)
            {
                if (toObj is ActionDefinition toAction)
                {
                    return HandleDependency(fromAction, toAction, mode);
                }
                else if (toObj is HexCell toCell)
                {
                    return HandleDependency(fromAction, GetBuildAction(toCell, techConfig), mode);
                }
            }
            else if (fromObj is HexCell fromCell)
            {
                if (toObj is ActionDefinition toAction)
                {
                    return HandleDependency(fromCell, toAction, mode);
                }
                else if (toObj is HexCell toCell)
                {
                    return HandleDependency(fromCell, GetBuildAction(toCell, techConfig), mode);
                }
            }

            return false;
        }

        private static bool HandleDependency(ActionDefinition fromAction, ActionDefinition toAction, UpgradeDependencyMode mode)
        {
            var tech = GetTechFromAction(fromAction);
            if (toAction is BuildAction buildActionTo)
            {
                switch (mode)
                {
                    case UpgradeDependencyMode.CreateBuild:
                        // Not applicable
                        break;
                    case UpgradeDependencyMode.CreateTechRequirement:
                        if (buildActionTo.techRequirements.Contains(tech)) return false;
                        buildActionTo.techRequirements.Add(tech);
                        break;
                    case UpgradeDependencyMode.RemoveBuild:
                        // Not applicable
                        break;
                    case UpgradeDependencyMode.RemoveTechRequirement:
                        if (!buildActionTo.techRequirements.Contains(tech)) return false;
                        buildActionTo.techRequirements.Remove(tech);
                        break;
                    default:
                        return false;
                }
                
                EditorUtility.SetDirty(buildActionTo);
                return true;
            }
            else if (toAction is UpgradeAction upgradeActionTo)
            {
                // TODO expand upgrade actions
            }
            return false;
        }
        
        private static bool HandleDependency(HexCell fromCell, ActionDefinition toAction, UpgradeDependencyMode mode)
        {
            switch (mode)
            {
                case UpgradeDependencyMode.CreateBuild:
                    return AddActionToCell(fromCell, toAction);
                case UpgradeDependencyMode.CreateTechRequirement:
                    return AddRequirementToAction(toAction, fromCell.Technology);
                case UpgradeDependencyMode.RemoveBuild:
                    return RemoveActionFromCell(fromCell, toAction);
                case UpgradeDependencyMode.RemoveTechRequirement:
                    return RemoveRequirementFromAction(toAction, fromCell.Technology);
                default:
                    return false;
            }
        }

        private static BuildAction GetBuildAction(HexCell cell, TechConfig techConfig)
        {
            foreach (var buildAction in techConfig.buildActions)
            {
                if (buildAction.prefab.cellType == cell.cellType)
                {
                    return buildAction;
                }
            }
            return null;
        }

        private static Technology GetTechFromAction(ActionDefinition action)
        {
            if (action is BuildAction buildAction)
            {
                return buildAction.prefab.Technology;
            }
            else if (action is UpgradeAction upgradeAction)
            {
                // TODO expand upgrade tech
                Debug.LogWarning("Need to expand upgrade tech, Dan");
                //return upgradeAction
                return Technology.None;
            }

            return Technology.None;
        }

        private static bool AddActionToCell(HexCell cell, ActionDefinition action)
        {
            var fieldInfo = typeof(HexCell).GetField("actions", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null) return false;
            
            var actionList = ((ActionDefinition[]) fieldInfo.GetValue(cell)).ToList();
            if (actionList.Contains(action)) return false;
            
            actionList.Add(action);
            fieldInfo.SetValue(cell, actionList.ToArray());
            
            EditorUtility.SetDirty(cell);
            return true;
        }

        private static bool RemoveActionFromCell(HexCell cell, ActionDefinition action)
        {
            var fieldInfo = typeof(HexCell).GetField("actions", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null) return false;
            
            var actionList = ((ActionDefinition[]) fieldInfo.GetValue(cell)).ToList();
            if (!actionList.Contains(action)) return false;
            
            actionList.Remove(action);
            fieldInfo.SetValue(cell, actionList.ToArray());
            
            EditorUtility.SetDirty(cell);
            return true;
        }

        private static bool AddRequirementToAction(ActionDefinition action, Technology tech)
        {
            if (action is BuildAction buildAction)
            {
                if (buildAction.techRequirements.Contains(tech)) return false;
                buildAction.techRequirements.Add(tech);
                return true;
            }
            
            // TODO upgrades

            return false;
        }

        private static bool RemoveRequirementFromAction(ActionDefinition action, Technology tech)
        {
            if (action is BuildAction buildAction)
            {
                if (!buildAction.techRequirements.Contains(tech)) return false;
                buildAction.techRequirements.Remove(tech);
                return true;
            }
            
            // TODO upgrades

            return false;
        }
    }
}