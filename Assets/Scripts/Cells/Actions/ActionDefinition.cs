using System;
using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex.Actions
{
    public abstract class ActionDefinition : ScriptableObject
    {
        public Sprite icon;

        public virtual bool CanDisplay(int playerId) => true;
        
        /// <summary>
        /// Update the action definition before displaying 
        /// </summary>
        public virtual void Refresh(int playerId) {}

        public abstract bool CanAfford(int playerId);
        public abstract bool IsRequirementsMet(int playerId);
        
        protected abstract Tooltip.Details GetTooltipRequirementsMet();
        protected abstract Tooltip.Details GetTooltipRequirementsNotMet();

        protected GameData Data;
        
        public void Init(GameData data, int playerId)
        {
            Data = data;
        }

        public virtual Sprite GetIcon()
        {
            return icon;
        }

        public Tooltip.Details GetTooltipDetails(int playerId)
        {
            return IsRequirementsMet(playerId)
                ? GetTooltipRequirementsMet()
                : GetTooltipRequirementsNotMet();
        }

        protected bool IsTechAvailable(List<Technology> tech, int playerId)
        {
            return Data.IsTechAvailable(tech, playerId);
        }

        protected string GenerateRequirementsText(List<Technology> requiredTech)
        {
            var requirementsText = "";
            foreach (var technology in requiredTech)
            {
                if (requirementsText != "")
                {
                    requirementsText += ", ";
                }
                requirementsText += GetHumanReadableTechName(technology);
            }

            return "<color=red> Requires " + requirementsText + ".</color>";
        }

        private string GetHumanReadableTechName(Technology tech)
        {
            return tech.ToString();
        }
    }
}