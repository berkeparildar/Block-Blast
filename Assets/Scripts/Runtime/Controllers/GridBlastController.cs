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
        
        public List<int> Blast(List<GridPosition> matchedBlocks)
        {
            List<int> blastedColumns = new();
            
            foreach (GridPosition pos in matchedBlocks)
            {
                if (!blastedColumns.Contains(pos.Column))
                {
                    blastedColumns.Add(pos.Column);
                }

                if (_gridManager.GetBlockAtPosition(pos.Row, pos.Column).TakeDamage() > 0) continue;
                _gridManager.GetBlockAtPosition(pos.Row, pos.Column).Blast();
                _gridManager.SetBlockAtPosition(pos.Row, pos.Column, null);
                
            }
            LevelEvents.Instance.OnBlast.Invoke();
            return blastedColumns;
        }


        public void UpdateGridAfterBlast(List<int> affectedColumns)
        {
            int rows = m_GridData.GridRowSize;
            
            foreach (int c in affectedColumns)
            {
                for (int r = 0; r < rows; r++)
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