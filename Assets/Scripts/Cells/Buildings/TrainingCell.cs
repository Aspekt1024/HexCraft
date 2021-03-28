
namespace Aspekt.Hex
{
    public class TrainingCell : BuildingCell
    {
        public override Technology Technology { get; } = Technology.Barracks;
        
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return cellType == Cells.CellTypes.MeleeUnit
                || cellType == Cells.CellTypes.Stables;
        }
    }
}