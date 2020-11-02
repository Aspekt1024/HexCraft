using TMPro;
using UnityEngine;

namespace Aspekt.Hex.UI
{
    public class CurrencyUI : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI currency1;
        [SerializeField] private TextMeshProUGUI currency2;
        [SerializeField] private TextMeshProUGUI currency3;
#pragma warning restore 649

        public void SetCredits(int amount)
        {
            currency1.text = amount.ToString();
        }
        
        public void SetCredits2(int amount)
        {
            currency2.text = amount.ToString();
        }
        
        public void SetCredits3(int amount)
        {
            currency3.text = amount.ToString();
        }
    }
}