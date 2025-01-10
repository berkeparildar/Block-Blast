using System.Collections.Generic;
using Runtime.Data.ValueObjects;
using Runtime.Events;
using Runtime.Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Controllers
{
    public class GridBlastController : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        private GridData _gridData;

        public (int min, int max) Blast(List<GridPosition> matchedBlocks)
        {
            int minColumn = 10;
            int maxColumn = -1;

            foreach (GridPosition pos in matchedBlocks)
            {
                if (pos.Column < minColumn) minColumn = pos.Column;
                if (pos.Column > maxColumn) maxColumn = pos.Column;
                if (gridManager.GetBlockAtPosition(pos.Row, pos.Column).TakeDamage() > 0) continue;
                gridManager.GetBlockAtPosition(pos.Row, pos.Column).Blast();
                gridManager.SetBlockAtPosition(pos.Row, pos.Column, null);
            }

            GameEvents.Instance.OnBlast.Invoke();
            return (minColumn, maxColumn);
        }


        public void UpdateGridAfterBlast((int min, int max) columnRange)
        {
            int rows = _gridData.GridRowSize;
            
            for (int c = columnRange.min; c <= columnRange.max; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    if (gridManager.GetBlockAtPosition(r, c) != null) continue;
                    for (int nr = r + 1; nr < rows; nr++)
                    {
                        BlockManager aboveBlockManager = gridManager.GetBlockAtPosition(nr, c);
                        if (aboveBlockManager == null) continue;
                        if (aboveBlockManager.GetColor() < 0) break;
                        
                        gridManager.SetBlockAtPosition(r, c, aboveBlockManager);
                        gridManager.SetBlockAtPosition(nr, c, null);
                        
                        StartCoroutine(aboveBlockManager.MoveToTargetGridPosition(new Vector2Int(c, r)));
                        
                        break;
                    }
                }
            }
        }

        public void SetData(GridData data) => _gridData = data;
    }
}