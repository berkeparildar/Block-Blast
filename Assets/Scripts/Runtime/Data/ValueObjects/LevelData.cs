namespace Runtime.Data.ValueObjects
{
    public struct LevelData
    {
        public GridData GridData;
        public int Level;
        public int[] Targets;
        public int[] TargetCounts;
        public int MoveLimit;
    }
}