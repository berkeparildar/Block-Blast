using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Runtime.Controllers
{
    public class BlockVisualController : MonoBehaviour
    {
        private static readonly Dictionary<AssetReference, Sprite> SpriteCache = new();
        private AsyncOperationHandle<Sprite> _currentBackgroundSpriteHandle;
        private AsyncOperationHandle<Sprite> _currentForegroundSpriteHandle;

        [SerializeField] private SpriteRenderer backgroundSpriteRenderer;
        [SerializeField] private SpriteRenderer foregroundSpriteRenderer;

        public void UpdateBackground(AssetReferenceSprite backgroundSpriteRef)
        {
            if (SpriteCache.TryGetValue(backgroundSpriteRef, out Sprite cachedSprite))
            {
                backgroundSpriteRenderer.sprite = cachedSprite;
                return;
            }

            if (_currentBackgroundSpriteHandle.IsValid())
                Addressables.Release(_currentBackgroundSpriteHandle);

            _currentBackgroundSpriteHandle = Addressables.LoadAssetAsync<Sprite>(backgroundSpriteRef);

            _currentBackgroundSpriteHandle.Completed += handle =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded) return;
                backgroundSpriteRenderer.sprite = handle.Result;
                SpriteCache[backgroundSpriteRef] = handle.Result;
            };
        }
        
        public void UpdateSymbol(AssetReferenceSprite symbolRef)
        {
            if (symbolRef.AssetGUID.Length == 0)
            {
                foregroundSpriteRenderer.sprite = null;
                return;
            }
            
            if (SpriteCache.TryGetValue(symbolRef, out Sprite cachedSprite))
            {
                foregroundSpriteRenderer.sprite = cachedSprite;
                return;
            }
            
            if (_currentForegroundSpriteHandle.IsValid())
                Addressables.Release(_currentForegroundSpriteHandle);
            
            _currentForegroundSpriteHandle = Addressables.LoadAssetAsync<Sprite>(symbolRef);
            
            _currentForegroundSpriteHandle.Completed += handle =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded) return;
                foregroundSpriteRenderer.sprite = handle.Result;
                SpriteCache[symbolRef] = handle.Result;
            };
        }

        public void DeactivateSymbol()
        {
            foregroundSpriteRenderer.sprite = null;
        }
        
        public void UpdateSortingOrder(int order)
        {
            backgroundSpriteRenderer.sortingOrder = order;
        }
    }
}