using Runtime.Data.ValueObjects;
using UnityEngine;

namespace Runtime.Data.UnityObjects
{
    [CreateAssetMenu(fileName = "BlockSO", menuName = "SO/BlockSO", order = 0)]
    public class BlockSO : ScriptableObject
    {
        public BlockData BlockData;
    }
}