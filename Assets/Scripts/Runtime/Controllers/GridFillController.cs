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
        private static readonly Queue<BlastableBlock> BlockPool = new();
        private int _spawnPos;
        private const int MaxGridSize = 100;

        [SerializeField] private GridManager gridManager;
        [SerializeField] private BlastableBlock blockPrefab;
        [SerializeField] private GameObject blockContainer;

        public void SetData(GridData data)
        {
            _gridData = data;
            _spawnPos = _gridData.GridRowSize;
        }

        public void InitializePool()
        {
            for (int i = 0; i < MaxGridSize; i++)
            {
                BlastableBlock block = Instantiate(blockPrefab, blockContainer.transform, true);
                EnqueueBlock(block);
            }
        }

        public static void EnqueueBlock(BlastableBlock block)
        {
            block.gameObject.SetActive(false);
            block.transform.localPosition = Vector3.zero;
            BlockPool.Enqueue(block);
        }

        public static BlastableBlock DequeueBlock()
        {
            BlastableBlock deQueuedPoolObject = BlockPool.Dequeue();
            if (deQueuedPoolObject.gameObject.activeSelf) DequeueBlock();
            deQueuedPoolObject.ResetBlockData();
            deQueuedPoolObject.gameObject.SetActive(true);
            return deQueuedPoolObject;
        }

        public BlastableBlock[,] CreateGrid()
        {
            BlastableBlock[,] grid = new BlastableBlock[_gridData.GridRowSize, _gridData.GridColumnSize];
            Vector2Int origin = Vector2Int.zero;

            for (int r = 0; r < _gridData.GridRowSize; r++)
            {
                for (int c = 0; c < _gridData.GridColumnSize; c++)
                {
                    int colorIndex = _gridData.Grid[r, c];
                    BlastableBlock block = DequeueBlock();
                    block.SetColor(colorIndex);
                    block.UpdateSymbol(0);
                    Vector2Int blockSpawnPos = origin + new Vector2Int(c, r);
                    block.SetInitialBlockPosition(blockSpawnPos.x, blockSpawnPos.y);
                    grid[r, c] = block;
                    block.UpdateSortingOrder();
                }
            }
            return grid;
        }

        public void RefillEmptyCells((GridPosition min, GridPosition max) gridBounds)
        {
            int rows = _gridData.GridRowSize;
            int affectedRowCount = gridBounds.max.Row - gridBounds.min.Row + 1;

            for (int c = gridBounds.min.Column; c <= gridBounds.max.Column; c++)
            {
                int r = rows - 1;
                for (int i = 0; i < affectedRowCount; i++)
                {
                    if (gridManager.GetBlockAtPosition(r, c) != null)
                    {
                        BlastableBlock block = gridManager.GetBlockAtPosition(r, c);
                        if (block.GetColor() < 0) break;
                    }
                    else
                    {
                        BlastableBlock newBlock = DequeueBlock();
                        int randomColor = Random.Range(0, _gridData.ColorCount);
                        newBlock.SetColor(randomColor);
                        newBlock.transform.position = new Vector3(c, _spawnPos + (affectedRowCount - i));
                        gridManager.SetBlockAtPosition(r, c, newBlock);
                        StartCoroutine(newBlock.MoveToTargetGridPosition(new Vector2Int(c, r)));
                        newBlock.UpdateSortingOrder();
                    }
                    r--;
                }
            }
        }
    }
}