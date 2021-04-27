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
            var fromObj = from.GetObject();
            var toObj = to.GetObject();

            if (fromObj is ActionDefinition fromAction)
            {
                if (toObj is ActionDefinition toAction)
                {
                    return HandleDependency(fromAction, toAction, techConfig, mode);
                }
                else if (toObj is HexCell toCell)
                {
                    return HandleDependency(fromAction, toCell, techConfig, mode);
                }
            }
            else if (fromObj is HexCell fromCell)
            {
                if (toObj is ActionDefinition toAction)
                {
                    return HandleDependency(fromCell, toAction, techConfig, mode);
                }
                else if (toObj is HexCell toCell)
                {
                    return HandleDependency(fromCell, toCell, techConfig, mode);
                }
            }

            return false;
        }

        private static bool HandleDependency(ActionDefinition fromAction, ActionDefinition toAction, TechConfig techConfig, UpgradeDependencyMode mode)
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

        private static bool HandleDependency(ActionDefinition fromAction, HexCell toCell, TechConfig techConfig, UpgradeDependencyMode mode)
        {
            var buildActionTo = GetBuildAction(toCell, techConfig);
            var tech = GetTechFromAction(fromAction);
            buildActionTo.techRequirements.Add(tech);
            EditorUtility.SetDirty(buildActionTo);
            return true;
        }
        
        private static bool HandleDependency(HexCell fromCell, ActionDefinition toAction, TechConfig techConfig, UpgradeDependencyMode mode)
        {
            if (mode == UpgradeDependencyMode.CreateBuild)
            {
                return AddBuildAction(fromCell, toAction);
            }
            else if (mode == UpgradeDependencyMode.RemoveBuild)
            {
                return RemoveBuildAction(fromCell, toAction);
            }
            else if (mode == UpgradeDependencyMode.CreateTechRequirement)
            {
                // TODO
            }

            return false;
        }
        
        private static bool HandleDependency(HexCell fromCell, HexCell toCell, TechConfig techConfig, UpgradeDependencyMode mode)
        {
            var buildActionTo = GetBuildAction(toCell, techConfig);
            switch (mode)
            {
                case UpgradeDependencyMode.CreateBuild:
                    return AddBuildAction(fromCell, buildActionTo);
                case UpgradeDependencyMode.CreateTechRequirement:
                    // TODO
                    return false;
                case UpgradeDependencyMode.RemoveBuild:
                    return RemoveBuildAction(fromCell, buildActionTo);
                case UpgradeDependencyMode.RemoveTechRequirement:
                    // TODO
                    return false;
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

        private static bool AddBuildAction(HexCell cell, ActionDefinition action)
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

        private static bool RemoveBuildAction(HexCell cell, ActionDefinition action)
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
    }
}