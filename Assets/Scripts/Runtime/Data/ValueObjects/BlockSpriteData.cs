using System;
using UnityEngine.AddressableAssets;

namespace Runtime.Data.ValueObjects
{
    [Serializable]
    public struct BlockSpriteData
    {
        public AssetReferenceSprite[] ColoredBlockSprites;
    }
}