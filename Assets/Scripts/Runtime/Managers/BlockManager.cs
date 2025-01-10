using System.Collections;
using Runtime.Controllers;
using Runtime.Data.UnityObjects;
using Runtime.Data.ValueObjects;
using Runtime.Events;
using UnityEngine;


namespace Runtime.Managers
{
    public class BlockManager : MonoBehaviour
    {
        private BlockData _data;
        private int _health;
        private int _rowPosition;
        private int _columnPosition;
        private int _colorIndex;
        [SerializeField] private int _groupID;
        private bool _isStationary;
     
        [SerializeField] private BlockMovementController movementController;
        [SerializeField] private BlockVisualController visualController;
        [SerializeField] private BlockSO blockSO;
        public GridManager gridManager;
        
        public void SetColor(int colorIndex)
        {
            _colorIndex = colorIndex;
            _groupID = -1;
            GetData();
            visualController.UpdateBackground(_data.BackgroundSpriteReference);
            visualController.UpdateSymbol(_data.ForegroundSpriteReference);
            _isStationary = true;
        } 
        
        private void GetData()
        {
            _data = blockSO.GetBlockData(_colorIndex);
            _health = _data.Health;
        }
        
        public void SetInitialBlockPosition(int xPos, int yPos)
        {
            transform.position = new Vector2(xPos, yPos);
            visualController.UpdateSortingOrder(yPos);
        }
        
        public int TakeDamage()
        {
            _health--;
            return _health;
        }

        public void Blast()
        {
            ParticleManager.DequeueParticle(_colorIndex, transform.position);
            GameEvents.Instance.OnBlockBlasted.Invoke(_colorIndex);
            GridFillController.EnqueueBlock(this);
        }
        
        public void ResetBlockData()
        {
            _health = _data.Health;
            _groupID = -1;
            _isStationary = true;
        }
        
        public IEnumerator MoveToTargetGridPosition(Vector2Int targetPos)
        {
            var oldPos = movementController.GetTargetPos();
            _isStationary = false;
            yield return StartCoroutine(movementController.FallCoroutine(targetPos));
            visualController.UpdateSortingOrder(targetPos.y);
            _isStationary = true;
            gridManager.CheckMatchesOfLandedBlock(new GridPosition(targetPos.y, targetPos.x),
                new GridPosition((int)oldPos.y, (int)oldPos.x), _groupID);
        }        
        public int GetColor() => _colorIndex;
        public int GetGroupID() => _groupID;
        public void SetGroupID(int id) => _groupID = id;
        public bool IsStationary() => _isStationary;

        public void UpdateSymbol(int symbolIndex)
        {
            if (symbolIndex < 1)
            {
                var spriteRef = _data.ForegroundSpriteReference;
                visualController.UpdateSymbol(spriteRef);
                return;
            }
            else
            {
                var spriteRef = blockSO.GroupSpriteReferences[symbolIndex - 1];
                visualController.UpdateSymbol(spriteRef);
                return;
            }
        }
    }
}