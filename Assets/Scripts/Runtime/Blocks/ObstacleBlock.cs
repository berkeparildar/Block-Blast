using Runtime.Controllers;
using Runtime.Data.UnityObjects;
using Runtime.Data.ValueObjects;
using UnityEngine;

namespace Runtime.Blocks
{
    public class ObstacleBlock : Block
    {
        private BlockData _data;
        
        [SerializeField] private ObstacleBlockVisualController visualController;
        private void Awake()
        {
            GetData();
            visualController.SetData(_data);
        }

        protected override void GetData()
        {
            BlockSO so = (BlockSO)BlockDataSO;
            _data = so.BlockData;
            Health = _data.Health;
        }

        public override void UpdateBlockVisual()
        {
            visualController.UpdateBackground();
        }

        public override int TakeDamage()
        {
            int currentHealth = base.TakeDamage();
            if (Health > 0) UpdateBlockVisual();
            return currentHealth;
        }

        public override void UpdateSortingOrder(int order)
        {
            visualController.UpdateSortingOrder(order);
        }

        public override void ResetBlockData()
        {
            Health = _data.Health;
        }

        public override void Blast()
        {
            Destroy(gameObject);
        }
    }
}