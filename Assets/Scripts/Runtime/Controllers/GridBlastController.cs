using System.Collections.Generic;
using Runtime.Blocks;
using Runtime.Data.ValueObjects;
using Runtime.Events;
using Runtime.Managers;
using UnityEngine;

namespace Runtime.Controllers
{
    public class GridBlastController : MonoBehaviour
    {
        [SerializeField] private GridManager _gridManager;
        private GridData m_GridData;

        public (GridPosition min, GridPosition max) Blast(List<GridPosition> matchedBlocks)
        {
            int minColumn = 10;
            int maxColumn = -1;
            int minRow = 10;
            int maxRow = -1;

            foreach (GridPosition pos in matchedBlocks)
            {
                if (pos.Column < minColumn) minColumn = pos.Column;
                if (pos.Column > maxColumn) maxColumn = pos.Column;
                if (pos.Row < minRow) minRow = pos.Row;
                if (pos.Row > maxRow) maxRow = pos.Row;
                if (_gridManager.GetBlockAtPosition(pos.Row, pos.Column).TakeDamage() > 0) continue;
                _gridManager.GetBlockAtPosition(pos.Row, pos.Column).Blast();
                _gridManager.SetBlockAtPosition(pos.Row, pos.Column, null);
            }

            LevelEvents.Instance.OnBlast.Invoke();
            return (new GridPosition(minRow, minColumn), new GridPosition(maxRow, maxColumn));
        }


        public void UpdateGridAfterBlast((GridPosition min, GridPosition max) gridBounds)
        {
            int rows = m_GridData.GridRowSize;
            
            for (int c = gridBounds.min.Column; c <= gridBounds.max.Column; c++)
            {
                for (int r = gridBounds.min.Row; r < rows; r++)
                {
                    if (_gridManager.GetBlockAtPosition(r, c) != null) continue;
                    for (int nr = r + 1; nr < rows; nr++)
                    {
                        BlastableBlock aboveBlock = _gridManager.GetBlockAtPosition(nr, c);
                        if (aboveBlock == null) continue;
                        if (aboveBlock.GetColor() < 0) break;
                        
                        _gridManager.SetBlockAtPosition(r, c, aboveBlock);
                        _gridManager.SetBlockAtPosition(nr, c, null);
                        
                        StartCoroutine(aboveBlock.MoveToTargetGridPosition(new Vector2Int(c, r)));
                        
                        break;
                    }
                }
            }
        }

        public void SetData(GridData data) => m_GridData = data;
    }
}