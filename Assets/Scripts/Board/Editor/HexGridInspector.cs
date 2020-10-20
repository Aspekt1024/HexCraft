using System;
using UnityEditor;
using UnityEngine;

namespace Aspekt.Hex
{
    [CustomEditor(typeof(HexGrid))]
    public class HexGridInspector : Editor
    {
        private Tool lastTool = Tool.None;
 
        private void OnEnable()
        {
            lastTool = Tools.current;
            Tools.current = Tool.None;
        }
 
        private void OnDisable()
        {
            Tools.current = lastTool;
        }
        
        private void OnSceneGUI()
        {
            var grid = (HexGrid) target;
            
            Handles.color = Color.yellow;
            Handles.DrawSolidDisc(Vector3.zero, Vector3.up, 0.3f);

            grid.startLocation1 = ShowPositionHandles(grid.startLocation1, 1);
            grid.startLocation2 = ShowPositionHandles(grid.startLocation2, 2);
        }

        private HexCoordinates ShowPositionHandles(HexCoordinates coords, int player)
        {
            var pos = HexCoordinates.ToPosition(coords);
            
            Handles.color = Color.black;
            Handles.DrawSolidDisc(pos, Vector3.up, 1f);
            Handles.color = player == 1 ? Color.white : Color.black;
            Handles.DrawSolidDisc(pos, Vector3.up, 0.9f);
            
            var newPos = Handles.PositionHandle(pos, Quaternion.identity);
            var newCoord = HexCoordinates.FromPosition(newPos);

            return newCoord;
        }
        
    }
}