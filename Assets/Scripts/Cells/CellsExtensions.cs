using System.Linq;

namespace Aspekt.Hex
{
    public static class CellsExtensions
    {
        public static IncomeCell[] GetIncomeCells(this Cells cells, int playerId)
        {
            return cells.GetCells<IncomeCell>(playerId);
        }

        public static HomeCell[] GetHomeCells(this Cells cells, int playerId)
        {
            return cells.GetCells<HomeCell>(playerId);
        }

        public static T[] GetCells<T>(this Cells cells, int playerId) where T : HexCell
        {
            var playerOwnedCells = cells.AllCells.Where(c => c.PlayerId == playerId);
            var matchedCells = playerOwnedCells as T[] ?? playerOwnedCells.OfType<T>().ToArray();
            return !matchedCells.Any() ? new T[0] : matchedCells.ToArray();
        }
    }
}