using System.Collections.Generic;
using Runtime.Blocks;
using Runtime.Data.ValueObjects;
using Runtime.Managers;
using UnityEngine;

namespace Runtime.Controllers
{
    public class GridMatchController : MonoBehaviour
    {
        [SerializeField] private GridManager _gridManager;
        
        private GridData m_GridData;
        
        public List<BlastableBlock> CheckMatches(int row, int col, int[,] visited, int currentSearchID)
        {
            List<BlastableBlock> connectedBlocks = new();
            Queue<(int row, int col)> queue = new();
            BlastableBlock blockAtPosition = _gridManager.GetBlockAtPosition(row, col);
            if (blockAtPosition is not null && blockAtPosition.GetColor() < 0)
                return connectedBlocks;
            
            queue.Enqueue((row, col));

            while (queue.Count > 0)
            {
                (int r, int c) = queue.Dequeue();
                if (visited[r, c] == currentSearchID) continue;
                visited[r, c] = currentSearchID;
                blockAtPosition = _gridManager.GetBlockAtPosition(r, c);
                if (blockAtPosition is null || blockAtPosition.GetColor() < 0) continue;
                connectedBlocks.Add(blockAtPosition);
                foreach ((int nr, int nc) in GetNeighbors(r, c))
                {
                    if (visited[nr, nc] == currentSearchID) continue;
                    BlastableBlock neighbor = _gridManager.GetBlockAtPosition(nr, nc);
                    if (neighbor is null || neighbor.GetColor() < 0) continue;
                    if (neighbor.GetColor() != blockAtPosition.GetColor()) continue;
                    queue.Enqueue((nr, nc));
                }
            }

            return connectedBlocks;
        }
        
        public List<BlastableBlock> FindAdjacentObstacles(List<BlastableBlock> matchedBlocks)
        {
            List<BlastableBlock> adjacentObstacles = new();

            foreach (BlastableBlock block in matchedBlocks)
            {
                Vector2Int position = block.GetGridPosition();

                int row = position.y;
                int col = position.x;

                foreach ((int neighborRow, int neighborCol) in GetNeighbors(row, col))
                {
                    BlastableBlock neighborBlock = _gridManager.GetBlockAtPosition(neighborRow, neighborCol);
                    if (neighborBlock == null) continue;
                    if (neighborBlock.GetColor() < 0 && !adjacentObstacles.Contains(neighborBlock))
                    {
                        adjacentObstacles.Add(neighborBlock);
                    }
                }
            }

            return adjacentObstacles;
        }


        public static void SetGroupID(List<BlastableBlock> matchedBlocks, int groupID)
        {
            foreach (BlastableBlock block in matchedBlocks)
            {
                block.SetGroupID(groupID);
            }
        }
        
        private IEnumerable<(int, int)> GetNeighbors(int row, int col)
        {
            int rows = m_GridData.GridRowSize;
            int cols = m_GridData.GridColumnSize;

            if (row > 0) yield return (row - 1, col);
            if (row < rows - 1) yield return (row + 1, col);
            if (col > 0) yield return (row, col - 1);
            if (col < cols - 1) yield return (row, col + 1);
        }
        
        public void SetData(GridData data) => m_GridData = data;
    }
}