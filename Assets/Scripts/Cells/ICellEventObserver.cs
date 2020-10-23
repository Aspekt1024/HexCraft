namespace Aspekt.Hex
{
    public interface ICellEventObserver
    {
        void IndicateBuildCell(Cells.CellTypes type, HexCell originator);
        void UpgradeCell(HexCell cell);
    }
}