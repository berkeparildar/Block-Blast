using System;
using UnityEngine.AddressableAssets;

namespace Runtime.Data.ValueObjects
{
    [Serializable]
    public struct BlockData
    {
        public int Health;
        public AssetReferenceSprite[] SpriteReferences;
    }
}