using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aspekt.Hex
{
    public class Cells : MonoBehaviour
    {
        public enum CellTypes
        {
            Base = 1000,
            Training = 1200,
            Income = 1300,
            UnitT1 = 2010,
            UnitT2 = 2020,
        }
        
#pragma warning disable 649
        [Header("Buildings")]
        [SerializeField] private HomeCell home;
        [SerializeField] private TrainingCell training;
        [SerializeField] private IncomeCell income;
        
        [Header("Units")]
        [SerializeField] private HexCell unit1;
        [SerializeField] private HexCell unit2;
        
        [Header("Display")]
        [SerializeField] private HexCell indicator;
        [SerializeField] private Material standardMaterial;
        [SerializeField] private Material holoMaterial;
        [SerializeField] private Color blackColour;
        [SerializeField] private Color whiteColour;
#pragma warning restore 649
        
        private readonly List<HexCell> cells = new List<HexCell>();

        public HexCell Create(CellTypes type)
        {
            HexCell cell = null;
            switch (type)
            {
                case CellTypes.Base:
                    cell = Instantiate(home);
                    break;
                case CellTypes.Training:
                    cell = Instantiate(training);
                    break;
                case CellTypes.Income:
                    cell = Instantiate(income);
                    break;
                case CellTypes.UnitT1:
                    cell = Instantiate(unit1);
                    break;
                case CellTypes.UnitT2:
                    cell = Instantiate(unit2);
                    break;
                default:
                    return null;
            }

            if (cell != null)
            {
                cells.Add(cell);
            }

            return cell;
        }

        public Material HoloMaterial => holoMaterial;
        
        public Color GetColour(int playerId)
        {
            return playerId == 1 ? whiteColour : blackColour;
        }
        
        public bool IsPieceInCell(HexCoordinates coordinates)
        {
            return cells.Any(c => c.IsPlaced && c.Coordinates.Equals(coordinates));
        }

        public HexCell GetCellAtPosition(HexCoordinates coordinates)
        {
            return cells.FirstOrDefault(c => c.IsPlaced && c.Coordinates.Equals(coordinates));
        }

        public void RemoveCell(HexCoordinates coordinates)
        {
            var cell = cells.FirstOrDefault(c => c.Coordinates.Equals(coordinates));
            if (cell != null)
            {
                cells.Remove(cell);
                cell.Remove();
            }
        }
    }
}