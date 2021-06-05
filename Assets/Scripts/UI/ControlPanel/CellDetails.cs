using TMPro;
using UI.ControlPanel;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class CellDetails : MonoBehaviour
    {
#pragma warning disable 649
        [Header("key details")]
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI health;
        [SerializeField] private TextMeshProUGUI owner;
        [SerializeField] private TextMeshProUGUI description;
        
        [Header("Stats")]
        [SerializeField] private CellStat attack;
        [SerializeField] private CellStat defense;
        [SerializeField] private CellStat shield;
        [SerializeField] private CellStat speed;

        [Header("Production")]
        [SerializeField] private CellStat supplies;
        [SerializeField] private CellStat production;
        [SerializeField] private CellStat population;
#pragma warning restore 649

        public void Init(Tooltip tooltip)
        {
            attack.RegisterObserver(tooltip);
            defense.RegisterObserver(tooltip);
            shield.RegisterObserver(tooltip);
            speed.RegisterObserver(tooltip);
            supplies.RegisterObserver(tooltip);
            production.RegisterObserver(tooltip);
        }
        
        public void DisplayCellDetails(HexCell cell)
        {
            title.text = cell.DisplayName;
            description.text = cell.BasicDescription;
            owner.text = $"Owned by {cell.Owner.DisplayName}";
            health.text = $"HP: {cell.CurrentHP} / {cell.MaxHP}";

            if (cell is UnitCell unit)
            {
                var unitStats = unit.GetStats();
                attack.SetStat(unitStats.Attack);
                defense.SetStat(unitStats.Defense);
                shield.SetStat(unitStats.Shield);
                speed.SetStat(unitStats.Speed);
                
                HideObjects(supplies, production, population);
            }
            else if (cell is BuildingCell building)
            {
                var currencyBonus = building.GetCurrencyBonus();
                supplies.SetStat(currencyBonus.supplies);
                production.SetStat(currencyBonus.production);
                population.SetStat(currencyBonus.population);
                
                HideObjects(attack, defense, shield, speed);
            }
            else
            {
                HideAllStats();
            }
        }

        public void Clear()
        {
            title.text = "";
            description.text = "";
            owner.text = "";
            health.text = "";
            
            HideAllStats();
        }

        private static void SetStat(GameObject obj, TextMeshProUGUI textObj, int value)
        {
            if (value == 0)
            {
                obj.SetActive(false);
            }
            else
            {
                obj.SetActive(true);
                textObj.text = value.ToString();
            }
        }

        private static void HideObjects(params CellStat[] stats)
        {
            foreach (var stat in stats)
            {
                stat.Hide();
            }
        }

        private void HideAllStats()
        {
            HideObjects(attack,
                defense,
                shield,
                speed,
                production,
                population,
                supplies);
        }
    }
}