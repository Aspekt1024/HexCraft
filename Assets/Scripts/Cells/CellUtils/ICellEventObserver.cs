
using Aspekt.Hex.Actions;

namespace Aspekt.Hex
{
    public enum UnitActions
    {
        None = 0,
        Move = 1000,
        Attack = 2000,
    }
    
    public interface ICellEventObserver
    {
        void IndicateBuildCell(BuildAction buildAction, HexCell originator);
        void IndicateUnitAction(UnitCell unit, UnitAction unitAction);
        void AddTech(Technology tech);
    }

    public interface ICellLifecycleObserver
    {
        void OnCellCreated(HexCell cell);
        void OnCellRemoved(HexCell cell);
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