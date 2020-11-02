using UnityEngine;

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