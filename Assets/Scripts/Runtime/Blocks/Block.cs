using Runtime.Controllers;
using UnityEngine;

namespace Runtime.Blocks
{
    public abstract class Block: MonoBehaviour
    {
        [SerializeField] protected ScriptableObject BlockDataSO;
        
        protected int Health;
        protected int RowPos;
        protected int ColumnPos;
        
        protected abstract void GetData();
        
        public abstract void UpdateBlockVisual();
        
        public abstract void Blast();
        
        public virtual int TakeDamage()
        {
            Health--;
            return Health;
        }
        
        public void SetInitialBlockPosition(int xPos, int yPos)
        {
            RowPos = yPos;
            ColumnPos = xPos;
            transform.position = new Vector2(xPos, yPos);
        }

        public Vector2Int GetGridPosition()
        {
            return new Vector2Int(ColumnPos, RowPos);
        }

        public abstract void UpdateSortingOrder(int order);

        public abstract void ResetBlockData();
    }
}