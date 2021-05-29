using System;
using UnityEngine;

namespace Aspekt.Hex
{
    public class UnitModel : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private ArmorSet[] armorSets;
        [SerializeField] private GameObject[] weapons;
        [SerializeField] private GameObject[] shields;
        [SerializeField] private GameObject[] back;
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

        public void SetBack(int level)
        {
            for (int i = 0; i < back.Length; i++)
            {
                back[i].SetActive(i == level);
            }
        }

        public void SetShield(int level)
        {
            level--; // Shield level 0 = no shield, so make it match index -1 (nothing)
            for (int i = 0; i < shields.Length; i++)
            {
                shields[i].SetActive(i == level);
            }
        }

        public void SetMaterial(Material material)
        {
            foreach (var r in renderers)
            {
                r.materials = new[] {material};
            }
        }

        public void SetColor(Color color)
        {
            foreach (var r in renderers)
            {
                var material = r.material;
                material.color = color;
                r.material = material;
            }
        }
    }
}