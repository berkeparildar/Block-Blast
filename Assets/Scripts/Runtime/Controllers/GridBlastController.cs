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
        
        public List<int> Blast(List<Block> matchedBlocks)
        {
            List<int> blastedColumns = new();
            
            foreach (Block block in matchedBlocks)
            {
                Vector2Int gridPosition = block.GetGridPosition();
                
                if (!blastedColumns.Contains(block.GetGridPosition().x))
                {
                    blastedColumns.Add(block.GetGridPosition().x);
                }

                if (block.TakeDamage() > 0) continue;
                _gridManager.SetBlockAtPosition(gridPosition.y, gridPosition.x, null);
                block.Blast();
                
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
                        Block aboveBlock = _gridManager.GetBlockAtPosition(nr, c);
                        if (aboveBlock == null) continue;
                        if (aboveBlock is ObstacleBlock) break;
                        
                        _gridManager.SetBlockAtPosition(r, c, aboveBlock);
                        _gridManager.SetBlockAtPosition(nr, c, null);
                        
                        ColoredBlock coloredBlock = (ColoredBlock)aboveBlock;
                        StartCoroutine(coloredBlock.MoveToTargetGridPosition(new Vector2Int(c, r)));
                        
                        break;
                    }
                }
            }
        }
        
        public void SetData(GridData data) => m_GridData = data;
    }
}