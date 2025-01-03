using System.Collections.Generic;
using Runtime.Data.ValueObjects;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Runtime.Controllers
{
    public class ObstacleBlockVisualController : MonoBehaviour
    {
        private AssetReferenceSprite _damagedSpriteRef;
        private static readonly Dictionary<AssetReference, Sprite> SpriteCache = new();
        private AsyncOperationHandle<Sprite> _currentSpriteHandle;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void SetData(BlockData data)
        {
            _damagedSpriteRef = data.SpriteReferences[0];
        }
        
        internal void UpdateBackground()
        {
            if (SpriteCache.TryGetValue(_damagedSpriteRef, out Sprite cachedSprite))
            {
                spriteRenderer.sprite = cachedSprite;
                return;
            }
            
            if (_currentSpriteHandle.IsValid())
                Addressables.Release(_currentSpriteHandle);
            
            _currentSpriteHandle = Addressables.LoadAssetAsync<Sprite>(_damagedSpriteRef);

            _currentSpriteHandle.Completed += handle =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded) return;
                spriteRenderer.sprite = handle.Result;
                SpriteCache[_damagedSpriteRef] = handle.Result;
            };
        }
        
        internal void UpdateSortingOrder(int order) => spriteRenderer.sortingOrder = order;
    }
}