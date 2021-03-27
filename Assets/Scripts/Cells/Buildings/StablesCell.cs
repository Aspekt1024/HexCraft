namespace Aspekt.Hex
{
    public class StablesCell : BuildingCell
    {
        public override Technology Technology { get; } = Technology.Stables;
            
        public override bool CanCreate(Cells.CellTypes cellType)
        {
            return false;
        }

        public override void OnTechAdded(Technology tech)
        {
            
        }
    }
}