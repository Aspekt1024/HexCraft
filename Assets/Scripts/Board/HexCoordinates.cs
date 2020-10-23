using System;
using UnityEngine;

namespace Aspekt.Hex
{
    [Serializable]
    public class HexCoordinates
    {
        [SerializeField]
        private int x, z;

        public int X => x;
        public int Z => z;

        public int Y => -X - Z;

        public HexCoordinates(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public static HexCoordinates FromOffsetCoordinates(int x, int z)
        {
            return new HexCoordinates(x - Mathf.FloorToInt(z / 2f), z);
        }

        public static HexCoordinates FromPosition(Vector3 position)
        {
            float x = position.x / (HexCell.InnerRadius * 2f);
            float y = -x;
            
            float offset = position.z / (HexCell.OuterRadius * 3f);
            x -= offset;
            y -= offset;

            int iX = Mathf.RoundToInt(x);
            int iY = Mathf.RoundToInt(y);
            int iZ = Mathf.RoundToInt(-x -y);

            if (iX + iY + iZ != 0)
            {
                float dX = Mathf.Abs(x - iX);
                float dY = Mathf.Abs(y - iY);
                float dZ = Mathf.Abs(-x -y - iZ);

                if (dX > dY && dX > dZ) {
                    iX = -iY - iZ;
                }
                else if (dZ > dY) {
                    iZ = -iX - iY;
                }
            }
            
            return new HexCoordinates(iX, iZ);
        }

        public static Vector3 ToPosition(HexCoordinates coordinates)
        {
            return new Vector3(
                (coordinates.x + coordinates.z / 2f) * HexCell.InnerRadius * 2f,
                0f,
                coordinates.z * HexCell.OuterRadius * 1.5f
            );
        }

        public static int Distance(HexCoordinates c1, HexCoordinates c2)
        {
            return (Mathf.Abs(c1.X - c2.X)
                    + Mathf.Abs(c1.Y - c2.Y)
                    + Mathf.Abs(c1.Z - c2.Z)) / 2;
        }
        
        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + Z + ")";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is HexCoordinates coords)) return false;
            return coords.x == x && coords.z == z;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Z;
            }
        }
    }
}