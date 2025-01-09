using System;

namespace Runtime.Data.ValueObjects
{
    public struct GridPosition : IEquatable<GridPosition>
    {
        public int Row;
        public int Column;

        public GridPosition(int row, int column)
        {
            Row = row;
            Column = column;
        }
        
        public static bool operator ==(GridPosition left, GridPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GridPosition left, GridPosition right)
        {
            return !(left == right);
        }
        
        public bool Equals(GridPosition other)
        {
            return Row == other.Row && Column == other.Column;
        }
    }
}