using System.Collections.Generic;
using Runtime.Data.ValueObjects;
using Runtime.Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Controllers
{
    public class GridFillController : MonoBehaviour
    {
        private GridData _gridData;
        private static readonly Queue<BlockManager> BlockPool = new();
        private int _spawnPos;
        private const int MaxGridSize = 100;

        [SerializeField] private GridManager gridManager;
        [SerializeField] private BlockManager blockManagerPrefab;
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
                BlockManager block = Instantiate(blockManagerPrefab, blockContainer.transform, true);
                block.gridManager = gridManager;
                EnqueueBlock(block);
            }
        }

        public static void EnqueueBlock(BlockManager blockManager)
        {
            blockManager.gameObject.SetActive(false);
            blockManager.transform.localPosition = Vector3.zero;
            BlockPool.Enqueue(blockManager);
        }

        public static BlockManager DequeueBlock()
        {
            BlockManager deQueuedPoolObject = BlockPool.Dequeue();
            if (deQueuedPoolObject.gameObject.activeSelf) DequeueBlock();
            deQueuedPoolObject.ResetBlockData();
            deQueuedPoolObject.gameObject.SetActive(true);
            return deQueuedPoolObject;
        }

        public BlockManager[,] CreateGrid()
        {
            BlockManager[,] grid = new BlockManager[_gridData.GridRowSize, _gridData.GridColumnSize];
            Vector2Int origin = Vector2Int.zero;

            for (int r = 0; r < _gridData.GridRowSize; r++)
            {
                for (int c = 0; c < _gridData.GridColumnSize; c++)
                {
                    int colorIndex = _gridData.Grid[r, c];
                    BlockManager block = DequeueBlock();
                    block.SetColor(colorIndex);
                    Vector2Int blockSpawnPos = origin + new Vector2Int(c, r);
                    block.SetInitialBlockPosition(blockSpawnPos.x, blockSpawnPos.y);
                    grid[r, c] = block;
                }
            }
            return grid;
        }

        public void RefillEmptyCells((int min, int max) columnRange)
        {
            int rows = _gridData.GridRowSize;

            for (int c = columnRange.min; c <= columnRange.max; c++)
            {
                for (int r = rows - 1; r >= 0; r--)
                {
                    if (gridManager.GetBlockAtPosition(r, c) != null)
                    {
                        BlockManager blockManager = gridManager.GetBlockAtPosition(r, c);
                        if (blockManager.GetColor() < 0) break;
                    }
                    else
                    {
                        BlockManager newBlock = DequeueBlock();
                        int randomColor = Random.Range(0, _gridData.ColorCount);
                        newBlock.SetColor(randomColor);
                        newBlock.transform.position = new Vector3(c, _spawnPos + r);
                        gridManager.SetBlockAtPosition(r, c, newBlock);
                        StartCoroutine(newBlock.MoveToTargetGridPosition(new Vector2Int(c, r)));
                    }
                }
            }
        }
    }
}