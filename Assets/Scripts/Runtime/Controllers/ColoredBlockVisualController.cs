using System.Collections.Generic;
using Runtime.Data.ValueObjects;
using Runtime.Enums;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Runtime.Controllers
{
    public class ColoredBlockVisualController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer backgroundRenderer;
        [SerializeField] private SpriteRenderer symbolRenderer;
        
        private AssetReferenceSprite[] m_BackgroundSpriteReferences;
        private AssetReferenceSprite[] m_DefaultSpriteReferences;
        private AssetReferenceSprite[] m_SymbolSpriteReferences;

        private static readonly Dictionary<AssetReference, Sprite> SpriteCache = new();
        private AsyncOperationHandle<Sprite> m_CurrentBackgroundSpriteHandle;
        private AsyncOperationHandle<Sprite> m_CurrentSymbolSpriteHandle;

        public void SetData(ColoredBlockData data)
        {
            m_BackgroundSpriteReferences = data.BlockData.SpriteReferences;
            m_SymbolSpriteReferences = data.SymbolSprites;
            m_DefaultSpriteReferences = data.DefaultSprites;
        }
        
        internal void UpdateBackground(int colorIndex)
        {
            AssetReferenceSprite backgroundSpriteRef = m_BackgroundSpriteReferences[colorIndex];
            
            if (SpriteCache.TryGetValue(backgroundSpriteRef, out Sprite cachedSprite))
            {
                backgroundRenderer.sprite = cachedSprite;
                return;
            }
            
            if (m_CurrentBackgroundSpriteHandle.IsValid())
                Addressables.Release(m_CurrentBackgroundSpriteHandle);
            
            m_CurrentBackgroundSpriteHandle = Addressables.LoadAssetAsync<Sprite>(backgroundSpriteRef);

            m_CurrentBackgroundSpriteHandle.Completed += handle =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded) return;
                backgroundRenderer.sprite = handle.Result;
                SpriteCache[backgroundSpriteRef] = handle.Result;
            };
        }

        internal void UpdateSymbol(int colorIndex, ColoredBlockSprite symbol)
        {
            if (symbol == ColoredBlockSprite.Default)
            {
                AssetReferenceSprite defaultSpriteRef = m_DefaultSpriteReferences[colorIndex];
                
                if (m_CurrentSymbolSpriteHandle.IsValid())
                    Addressables.Release(m_CurrentSymbolSpriteHandle);
                
                m_CurrentSymbolSpriteHandle = Addressables.LoadAssetAsync<Sprite>(defaultSpriteRef);
                m_CurrentSymbolSpriteHandle.Completed += handle =>
                {
                    if (handle.Status != AsyncOperationStatus.Succeeded) return;
                    symbolRenderer.sprite = handle.Result;
                    SpriteCache[defaultSpriteRef] = handle.Result;
                };
            }
            else
            {
                int symbolIndex = (int)symbol - 1;
                AssetReferenceSprite symbolSpriteRef = m_SymbolSpriteReferences[symbolIndex];
                if (m_CurrentSymbolSpriteHandle.IsValid())
                    Addressables.Release(m_CurrentSymbolSpriteHandle);
                m_CurrentSymbolSpriteHandle = Addressables.LoadAssetAsync<Sprite>(symbolSpriteRef);
                m_CurrentSymbolSpriteHandle.Completed += handle =>
                {
                    if (handle.Status != AsyncOperationStatus.Succeeded) return;
                    symbolRenderer.sprite = handle.Result;
                    SpriteCache[symbolSpriteRef] = handle.Result;
                };
            }
        }

        internal void UpdateSortingOrder(int order) => backgroundRenderer.sortingOrder = order;
    }
}