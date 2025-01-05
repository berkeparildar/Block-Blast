using System.Collections;
using System.Collections.Generic;
using Runtime.Controllers;
using Runtime.Data.UnityObjects;
using Runtime.Data.ValueObjects;
using Runtime.Events;
using Runtime.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Runtime.Blocks
{
    public class BlastableBlock : MonoBehaviour
    {
        private static readonly Dictionary<AssetReference, Sprite> SpriteCache = new();
        private const float BounceOffset = 0.05f;
        private const float BounceUpDuration = 0.1f;
        private const float BounceDownDuration = 0.05f;
        private const float FallSpeed = 0.2f;
        
        private BlockData _data;
        private int _health;
        private int _rowPosition;
        private int _columnPosition;
        private int _colorIndex;
        private int _groupID;
        private bool _isStationary;
        private AsyncOperationHandle<Sprite> _currentBackgroundSpriteHandle;
        private AsyncOperationHandle<Sprite> _currentForegroundSpriteHandle;
        private float _baseSpeedModifier;
        private Vector2 _targetPos;
        private bool _movementInProgress;

        [SerializeField] private SpriteRenderer backgroundSpriteRenderer;
        [SerializeField] private SpriteRenderer foregroundSpriteRenderer;
        [SerializeField] private BlockSO blockSO;
        
        public void SetColor(int colorIndex)
        {
            _colorIndex = colorIndex;
            GetData();
            UpdateBackground();
        } 
        
        private void GetData()
        {
            _data = blockSO.GetBlockData(_colorIndex);
            _health = _data.Health;
        }
        
        public void SetInitialBlockPosition(int xPos, int yPos)
        {
            _rowPosition = yPos;
            _columnPosition = xPos;
            transform.position = new Vector2(xPos, yPos);
        }
        
        public int TakeDamage()
        {
            _health--;
            return _health;
        }
        
        public Vector2Int GetGridPosition()
        {
            return new Vector2Int(_columnPosition, _rowPosition);
        }
        
        public void Blast()
        {
            ParticleManager.DequeueParticle(_colorIndex, transform.position);
            LevelEvents.Instance.OnBlockBlasted.Invoke(_colorIndex);
            GridFillController.EnqueueBlock(this);
        }
        
        public void ResetBlockData()
        {
            _health = _data.Health;
            _groupID = -1;
            _isStationary = true;
            _movementInProgress = false;
            foregroundSpriteRenderer.sprite = null;
        }

        // MARK : - VISUAL METHODS -
        #region Visual Methods
        private void UpdateBackground()
        {
            if (SpriteCache.TryGetValue(_data.BackgroundSpriteReference, out Sprite cachedSprite))
            {
                backgroundSpriteRenderer.sprite = cachedSprite;
                return;
            }
            
            if (_currentBackgroundSpriteHandle.IsValid())
                Addressables.Release(_currentBackgroundSpriteHandle);
            
            _currentBackgroundSpriteHandle = Addressables.LoadAssetAsync<Sprite>(_data.BackgroundSpriteReference);

            _currentBackgroundSpriteHandle.Completed += handle =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded) return;
                backgroundSpriteRenderer.sprite = handle.Result;
                SpriteCache[_data.BackgroundSpriteReference] = handle.Result;
            };
        }
        
        public void UpdateSymbol(int symbolIndex)
        {
            if (_colorIndex < 0 ) return;
            if (_currentForegroundSpriteHandle.IsValid())
                Addressables.Release(_currentForegroundSpriteHandle);
            AssetReferenceSprite spriteRef;
            if (symbolIndex == 0)
            {
                spriteRef = _data.ForegroundSpriteReference;
                _currentForegroundSpriteHandle = Addressables.LoadAssetAsync<Sprite>(spriteRef);
               
            }
            else
            {
                spriteRef = blockSO.GroupSpriteReferences[symbolIndex - 1];
                _currentForegroundSpriteHandle = Addressables.LoadAssetAsync<Sprite>(spriteRef);
            }
            
            _currentForegroundSpriteHandle.Completed += handle =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded) return;
                foregroundSpriteRenderer.sprite = handle.Result;
                SpriteCache[spriteRef] = handle.Result;
            };
        }

        public void UpdateSortingOrder()
        {
            backgroundSpriteRenderer.sortingOrder = _rowPosition;
        }
        #endregion
        
        // MARK: - MOVEMENT METHODS - 
        #region Movement Methods
        public IEnumerator MoveToTargetGridPosition(Vector2Int targetPos)
        {
            _isStationary = false;
            _columnPosition = targetPos.x;
            _rowPosition = targetPos.y;
            _targetPos = targetPos;
            yield return StartCoroutine(FallCoroutine());
            UpdateSortingOrder();
            _isStationary = true;
            if (_groupID >= 0)
            {
                GridEvents.Instance.OnBlockLanded.Invoke(_groupID);
            }
            else
            {
                UpdateSymbol(0);
            }
        }

        private IEnumerator FallCoroutine()
        {
            if (_movementInProgress) yield return null;
            float timeElapsed = 0f;
            _movementInProgress = true;

            while ((Vector2)transform.position != _targetPos)
            {
                timeElapsed += Time.deltaTime;
                _baseSpeedModifier = GetSineValue(timeElapsed, 5);
                float step = FallSpeed * _baseSpeedModifier;
                transform.position = Vector2.MoveTowards(transform.position, _targetPos, step);
                yield return null;
            }

            transform.position = _targetPos;
            StartCoroutine(ApplyBounceEffect());
            _baseSpeedModifier = 0;
            _movementInProgress = false;
        }
        
        private IEnumerator ApplyBounceEffect()
        {
            Vector3 originalPosition = transform.position;
            Vector3 targetUpPosition = new(originalPosition.x, originalPosition.y + BounceOffset);

            yield return StartCoroutine(
                MoveTowardsTarget(originalPosition, targetUpPosition, BounceUpDuration, false));

            transform.position = targetUpPosition;
            
            yield return StartCoroutine(
                MoveTowardsTarget(targetUpPosition, originalPosition, BounceDownDuration, true));
        }

        private IEnumerator MoveTowardsTarget(Vector3 current, Vector3 target, float duration, bool easeIn)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float easeValue = easeIn ? Mathf.Pow(t, 3) : 1 - Mathf.Pow(1 - t, 3);
                transform.position = Vector3.Lerp(current, target, easeValue);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = target;
        }

        private static float GetSineValue(float time, float maxTime)
        {
            float t = Mathf.Clamp01(time / maxTime);
            return Mathf.Sin(t * Mathf.PI) * 2f;
        }

        #endregion
        
        public int GetColor() => _colorIndex;
        public int GetGroupID() => _groupID;
        public void SetGroupID(int id) => _groupID = id;
        public bool IsStationary() => _isStationary;
    }
}