using System;
using System.Collections.Generic;
using Aspekt.Hex.UI;
using UnityEngine;

namespace Aspekt.Hex.Actions
{
    public abstract class ActionDefinition : ScriptableObject
    {
        public Sprite icon;

        public virtual bool CanDisplay() => true;
        
        /// <summary>
        /// Update the action definition before displaying 
        /// </summary>
        public virtual void Update() {}
        
        protected abstract bool IsRequirementsMet();
        protected abstract Tooltip.Details GetTooltipRequirementsMet();
        protected abstract Tooltip.Details GetTooltipRequirementsNotMet();

        protected  GameData Data;
        protected int PlayerId;
        
        public void Init(GameData data, int playerId)
        {
            Data = data;
            PlayerId = playerId;
        }

        public virtual Sprite GetIcon()
        {
            return icon;
        }

        public Tooltip.Details GetTooltipDetails()
        {
            return IsRequirementsMet()
                ? GetTooltipRequirementsMet()
                : GetTooltipRequirementsNotMet();
        }

        protected bool IsTechAvailable(List<Technology> tech)
        {
            return Data.IsTechAvailable(tech, PlayerId);
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

            return "<color=red> Requires" + requirementsText + ".</color>";
        }

        private string GetHumanReadableTechName(Technology tech)
        {
            return tech.ToString();
        }
    }
}