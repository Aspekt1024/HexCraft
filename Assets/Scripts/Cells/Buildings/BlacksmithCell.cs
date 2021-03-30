namespace Aspekt.Hex
{
    public class BlacksmithCell : BuildingCell
    {
        public override Technology Technology { get; } = Technology.Blacksmith;

        public override void OnTechAdded(Technology tech)
        {
            
        }
    }
}