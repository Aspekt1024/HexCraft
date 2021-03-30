namespace Aspekt.Hex
{
    public class StablesCell : BuildingCell
    {
        public override Technology Technology { get; } = Technology.Stables;

        public override void OnTechAdded(Technology tech)
        {
            
        }
    }
}