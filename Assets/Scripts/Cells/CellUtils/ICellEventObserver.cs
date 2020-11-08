
using Aspekt.Hex.Actions;

namespace Aspekt.Hex
{
    public interface ICellEventObserver
    {
        void IndicateBuildCell(Cells.CellTypes type, HexCell originator);

        void IndicateUnitMove(UnitCell unit);
        void IndicateUnitAttack(UnitCell unit);
        void TryPurchaseTech(Technology tech);
    }

    public interface ICellLifecycleObserver
    {
        void OnCellCreated(HexCell cell);
    }

    public interface ICellHealthObserver
    {
        void OnCellHealthChanged(HexCell cell, float prevPercent, float newPercent);
    }

    public interface IUnitActionObserver
    {
        void OnFinishedMove(UnitCell unit);
        void OnFinishedAttack(UnitCell unit);
    }
}