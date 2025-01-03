using System.Collections;
using Runtime.Controllers;
using Runtime.Data.UnityObjects;
using Runtime.Data.ValueObjects;
using Runtime.Enums;
using Runtime.Events;
using UnityEngine;

namespace Runtime.Blocks
{
    public class ColoredBlock : Block
    {
        [SerializeField] private ColoredBlockMovementController movementController;
        [SerializeField] private ColoredBlockVisualController visualController;
        
        private ColoredBlockSprite _sprite;
        private ColoredBlockData _coloredBlockData;
        private int _colorIndex;
        private bool _hasGroup;
        private bool _isStationary;
        private int _groupID;

        private void Awake()
        {
            GetData();
            visualController.SetData(_coloredBlockData);
        }

        public override void UpdateSortingOrder(int order)
        {
            visualController.UpdateSortingOrder(order);
        }

        public override void ResetBlockData()
        {
            Health = _coloredBlockData.BlockData.Health;
            _sprite = ColoredBlockSprite.Default;
            _isStationary = true;
        }

        protected override void GetData()
        {
            ColoredBlockSO so = (ColoredBlockSO)BlockDataSO;
            _coloredBlockData = so.ColoredBlockData;
        }

        public override void UpdateBlockVisual()
        {
            visualController.UpdateSymbol(_colorIndex, _sprite);
        }

        public override int TakeDamage()
        {
            return _isStationary ? base.TakeDamage() : Health;
        }

        public override void Blast()
        {
            LevelEvents.Instance.OnBlockBlasted.Invoke(_colorIndex);
            GridFillController.EnqueueBlock(this);
        }

        public IEnumerator MoveToTargetGridPosition(Vector2Int targetPos)
        {
            _isStationary = false;
            ColumnPos = targetPos.x;
            RowPos = targetPos.y;
            yield return StartCoroutine(movementController.MoveToTargetPosition(targetPos));
            visualController.UpdateSortingOrder(RowPos);
            _isStationary = true;
            if (_hasGroup)
            {
                GridEvents.Instance.OnBlockLanded.Invoke(_groupID);
            }
            else
            {
                _sprite = ColoredBlockSprite.Default;
                UpdateBlockVisual();
            }
        }

        public void SetColorIndex(int color)
        {
            _colorIndex = color;
            visualController.UpdateBackground(_colorIndex);
        }

        public bool IsStationary() => _isStationary;

        public void SetSprite(ColoredBlockSprite sp) => _sprite = sp;

        public void SetGroupStatus(bool status) => _hasGroup = status;

        public int GetGroupID() => _groupID;

        public int GetColorIndex() => _colorIndex;

        public bool HasGroup() => _hasGroup;

        public void SetGroupID(int id) => _groupID = id;
        
        public void SetIsStationary(bool isStationary) => _isStationary = isStationary;
    }
}