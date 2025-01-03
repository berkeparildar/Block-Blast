using System;

namespace Runtime.Data.ValueObjects
{
    [Serializable]
    public struct GridData
    {
        public int GridRowSize;
        public int GridColumnSize;
        public int[,] Grid;
        public int ColorCount;
    }
}