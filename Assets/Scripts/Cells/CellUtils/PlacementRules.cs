using UnityEngine;

namespace Aspekt.Hex
{
    public static class PlacementRules
    {
        public static bool IsValidPlacementDistance(int radius, HexCoordinates cellCoords, HexCoordinates origin)
        {
            var distance = (Mathf.Abs(cellCoords.X - origin.X)
                           + Mathf.Abs(cellCoords.Y - origin.Y)
                           + Mathf.Abs(cellCoords.Z - origin.Z)) / 2;

            return distance <= radius;
        }
    }
}