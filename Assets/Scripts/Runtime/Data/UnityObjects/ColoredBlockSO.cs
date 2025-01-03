using Runtime.Data.ValueObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Runtime.Data.UnityObjects
{
    [CreateAssetMenu(fileName = "ColoredBlockSO", menuName = "SO/ColoredBlockSO", order = 0)]
    public class ColoredBlockSO : ScriptableObject
    {
        public ColoredBlockData ColoredBlockData;
       
    }
}