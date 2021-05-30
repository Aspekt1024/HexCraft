using System.Linq;

namespace Aspekt.Hex
{
    public static class CellsExtensions
    {

        public static BuildingCell[] GetHomeCells(this Cells cells, int playerId)
        {
            return cells.GetCells<BuildingCell>(playerId, Cells.CellTypes.Home);
        }

        public static T[] GetCells<T>(this Cells cells, int playerId, Cells.CellTypes cellType) where T : HexCell
        {
            var playerOwnedCells = cells.AllCells.Where(c => c.PlayerId == playerId);
            var matchedCells = playerOwnedCells as T[] ?? playerOwnedCells
                .OfType<T>()
                .Where(c => c.cellType == cellType).ToArray();
            
            return !matchedCells.Any() ? new T[0] : matchedCells.ToArray();
        }
    }
}