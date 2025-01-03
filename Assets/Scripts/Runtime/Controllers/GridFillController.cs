using System.Collections.Generic;
using Runtime.Blocks;
using Runtime.Data.ValueObjects;
using Runtime.Managers;
using UnityEngine;

namespace Runtime.Controllers
{
    public class GridFillController : MonoBehaviour
    {
        private GridData _gridData;
        private static Queue<ColoredBlock> _blockPool = new();
        private int _spawnPos;

        [SerializeField] private GridManager gridManager;
        [SerializeField] private ColoredBlock coloredPrefab;
        [SerializeField] private ObstacleBlock obstaclePrefab;
        [SerializeField] private Transform poolHolder;

        public void SetData(GridData data)
        {
            _gridData = data;
            _spawnPos = _gridData.GridRowSize + (Mathf.RoundToInt(_gridData.GridRowSize / 2f)) + 1;
        }
        
        public void InitializePool()
        {
            int poolAmount = _gridData.GridRowSize * _gridData.GridColumnSize;
            _blockPool = new Queue<ColoredBlock>();

            for (int i = 0; i < poolAmount; i++)
            {
                ColoredBlock block = Instantiate(coloredPrefab, poolHolder.transform, true);
                block.gameObject.SetActive(false);
                _blockPool.Enqueue(block);
            }
        }
        
        public static void EnqueueBlock(ColoredBlock poolObject)
        {
            poolObject.gameObject.SetActive(false);
            poolObject.transform.localPosition = Vector3.zero;
            _blockPool.Enqueue(poolObject);
        }

        public static ColoredBlock DequeueBlock()
        {
            ColoredBlock deQueuedPoolObject = _blockPool.Dequeue(); 
            if (deQueuedPoolObject.gameObject.activeSelf) DequeueBlock();
            deQueuedPoolObject.ResetBlockData();
            deQueuedPoolObject.gameObject.SetActive(true);
            return deQueuedPoolObject;
        }
        
        public Block[,] CreateGrid()
        {
            int rows = _gridData.GridRowSize;
            int cols = _gridData.GridColumnSize;
            
            Block[,] grid = new Block[rows, cols];
            Vector2Int origin = Vector2Int.zero;
            
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (_gridData.Grid[r, c] < 0)
                    {
                        ObstacleBlock block = Instantiate(obstaclePrefab, poolHolder.transform, true);
                        Vector2Int blockSpawnPos = origin + new Vector2Int(c, r);
                        block.SetInitialBlockPosition(blockSpawnPos.x, blockSpawnPos.y);
                        grid[r, c] = block;
                        block.UpdateSortingOrder(r);
                    }
                    else
                    {
                        int colorIndex = _gridData.Grid[r, c];
                        ColoredBlock coloredBlock = DequeueBlock();
                        coloredBlock.SetColorIndex(colorIndex);
                        coloredBlock.UpdateBlockVisual();
                        Vector2Int blockSpawnPos = origin + new Vector2Int(c, r);
                        coloredBlock.SetInitialBlockPosition(blockSpawnPos.x, blockSpawnPos.y);
                        grid[r, c] = coloredBlock;
                        coloredBlock.UpdateSortingOrder(r);
                    }
                }
            }
            return grid;
        }
        
        public void RefillEmptyCells(List<int> affectedColumns)
        {
            int rows = _gridData.GridRowSize;
            
            foreach (int c in affectedColumns)
            {
                for (int r = rows - 1; r >= 0; r--)
                {
                    if (gridManager.GetBlockAtPosition(r, c) != null)
                    {
                        Block block = gridManager.GetBlockAtPosition(r, c);
                        if (block is ObstacleBlock) break;
                    }
                    else
                    {
                        Block newBlock = DequeueBlock();
                        if (newBlock is not ColoredBlock coloredBlock) continue;
                        int randomColor = Random.Range(0, _gridData.ColorCount);
                        coloredBlock.SetColorIndex(randomColor);
                        newBlock.transform.position = new Vector3(c, _spawnPos + r);
                        gridManager.SetBlockAtPosition(r, c, newBlock);
                        StartCoroutine(coloredBlock.MoveToTargetGridPosition(new Vector2Int(c, r)));
                        newBlock.UpdateSortingOrder(_spawnPos + r);
                    }
                }
            }
        }
    }
}