namespace Aspekt.Hex
{
    public interface ICellEventObserver
    {
        void BuildCell(Cells.CellTypes type, HexCell originator);
        void UpgradeCell(HexCell cell);
    }
}