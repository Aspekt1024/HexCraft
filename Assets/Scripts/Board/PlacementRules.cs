using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.Hex
{
    public static class PlacementRules
    {
        public static int GetRadius(Cells.CellTypes type)
        {
            switch (type)
            {
                case Cells.CellTypes.Base:
                    return 0;
                case Cells.CellTypes.Training:
                    return 4;
                case Cells.CellTypes.Income:
                    return 2;
                case Cells.CellTypes.UnitT1:
                    return 1;
                case Cells.CellTypes.UnitT2:
                    return 1;
                default:
                    return 0;
            }
        }

        public static bool IsValidPlacementDistance(Cells.CellTypes type, HexCoordinates cellCoords, HexCoordinates origin)
        {
            var radius = GetRadius(type);
            var distance = (Mathf.Abs(cellCoords.X - origin.X)
                           + Mathf.Abs(cellCoords.Y - origin.Y)
                           + Mathf.Abs(cellCoords.Z - origin.Z)) / 2;

            return distance <= radius;
        }
    }
}