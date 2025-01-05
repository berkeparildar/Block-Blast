using Runtime.Data.ValueObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Runtime.Data.UnityObjects
{
    [CreateAssetMenu(fileName = "BlockSO", menuName = "SO/BlockSO", order = 0)]
    public class BlockSO : ScriptableObject
    {
        public BlockData[] AllBlockDatas;
        public AssetReferenceSprite[] GroupSpriteReferences;

        public BlockData GetBlockData(int colorIndex)
        {
            return colorIndex < 0 ? AllBlockDatas[^1] : AllBlockDatas[colorIndex];
        }
    }
}