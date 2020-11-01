using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class HealthBars : MonoBehaviour, ICellHealthObserver, HealthBar.IObserver
    {
#pragma warning disable 649
        [SerializeField] private HealthBar healthBarPrefab;
#pragma warning restore 649
        
        private readonly List<HealthBar> bars = new List<HealthBar>();
        private readonly Dictionary<int, HealthBar> linkedBars = new Dictionary<int, HealthBar>();
        
        public void OnCellHealthChanged(HexCell cell, float prevPercent, float newPercent)
        {
            if (!linkedBars.TryGetValue(cell.GetInstanceID(), out var bar))
            {
                bar = GetFreeHealthBar();
                linkedBars.Add(cell.GetInstanceID(), bar);
            }
            
            bar.SetHealth(cell.transform, prevPercent, newPercent);
        }
        
        private HealthBar GetFreeHealthBar()
        {
            foreach (var bar in bars)
            {
                if (!bar.gameObject.activeSelf)
                {
                    return bar;
                }
            }

            var newBar = Instantiate(healthBarPrefab, transform);
            bars.Add(newBar);
            newBar.RegisterObserver(this);
            return newBar;
        }

        public void OnHealthbarHidden(HealthBar bar)
        {
            var cellId = (from barKV in linkedBars
                where barKV.Value == bar
                select barKV.Key).FirstOrDefault();

            if (cellId > 0)
            {
                linkedBars.Remove(cellId);
            }
        }
    }
}