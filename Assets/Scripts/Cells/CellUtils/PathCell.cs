namespace Aspekt.Hex
{
    public class PathCell
    {
        public readonly int X;
        public readonly int Z;
        public readonly int Dist;
        public readonly PathCell PreviousCell;

        public PathCell(int x, int z, int dist, PathCell previousCell)
        {
            X = x;
            Z = z;
            Dist = dist;
            PreviousCell = previousCell;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PathCell c)) return false;
            return c.X == X && c.Z == Z;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X;
                hashCode = (hashCode * 397) ^ Z;
                hashCode = (hashCode * 397) ^ Dist;
                return hashCode;
            }
        }
    }
}