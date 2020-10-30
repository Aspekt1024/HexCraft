namespace Aspekt.Hex
{
    public interface ICellEventObserver
    {
        void IndicateBuildCell(Cells.CellTypes type, HexCell originator);
        void UpgradeCell(HexCell cell);

        void IndicateUnitMove(UnitCell unit);
        void IndicateUnitAttack(UnitCell unit);
    }

    public interface ICellLifecycleObserver
    {
        void OnCellCreated(HexCell cell);
    }
}