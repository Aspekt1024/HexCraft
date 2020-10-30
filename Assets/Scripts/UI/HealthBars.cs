using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class HealthBars : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private HealthBar healthBarPrefab;
#pragma warning restore 649
        
        private readonly List<HealthBar> bars = new List<HealthBar>();

        public void LinkHealthBar(HexCell cell)
        {
            var bar = GetFreeHealthBar();
            bar.Init(cell.transform, 1f);
            cell.OnHealthUpdated += bar.SetHealth;
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
            return newBar;
        }
    }
}