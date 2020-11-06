using UnityEngine;

namespace Aspekt.Hex
{
    public class MountedUnitModel : UnitModel
    {
#pragma warning disable 649
        [SerializeField] private GameObject[] mounts;
#pragma warning restore 649
        
        public void SetMountLevel(int level)
        {
            for (int i = 0; i < mounts.Length; i++)
            {
                mounts[i].SetActive(i == level);
            }
        }
    }
}