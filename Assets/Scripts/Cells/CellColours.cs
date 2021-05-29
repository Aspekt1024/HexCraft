using UnityEngine;

namespace Aspekt.Hex
{
    [CreateAssetMenu(menuName = "Hex/Cell Colour Profile", fileName = "NewCellColourProfile")]
    public class CellColours : ScriptableObject
    {
        public Material black;
        public Material white;
        public Material red;
        public Material blue;
        public Material green;
        public Material yellow;
        public Material brown;
    }
}