using System;
using UnityEngine;

namespace Aspekt.Hex
{
    public class UnitModel : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private ArmorSet[] armorSets;
        [SerializeField] private GameObject[] weapons;
        [SerializeField] private GameObject[] shields;
#pragma warning restore 649

        [Serializable]
        public struct ArmorSet
        {
            public GameObject body;
            public GameObject helm;
        }

        public void SetEnabled()
        {
            gameObject.SetActive(true);
        }

        public void SetDisabled()
        {
            gameObject.SetActive(false);
        }

        public void SetArmor(int level)
        {
            for (int i = 0; i < armorSets.Length; i++)
            {
                var isActive = i == level;
                armorSets[i].body.SetActive(isActive);
                armorSets[i].helm.SetActive(isActive);
            }
        }

        public void SetWeapon(int level)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].SetActive(i == level);
            }
        }

        public void SetShield(int level)
        {
            for (int i = 0; i < shields.Length; i++)
            {
                shields[i].SetActive(i == level);
            }
        }
    }
}