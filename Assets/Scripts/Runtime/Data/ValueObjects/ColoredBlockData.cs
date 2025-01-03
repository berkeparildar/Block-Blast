using System;
using Runtime.Enums;
using UnityEngine.AddressableAssets;

namespace Runtime.Data.ValueObjects
{
    [Serializable]
    public struct ColoredBlockData
    {
        public BlockData BlockData;
        public AssetReferenceSprite[] DefaultSprites;
        public AssetReferenceSprite[] SymbolSprites;
    }
}