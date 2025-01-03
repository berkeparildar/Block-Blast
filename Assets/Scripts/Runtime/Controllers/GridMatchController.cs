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
        
        public List<ColoredBlock> CheckMatches(int row, int col, int[,] visited, int currentSearchID)
        {
            List<ColoredBlock> connectedBlocks = new();
            Queue<(int row, int col)> queue = new();

            ColoredBlock blockAtPosition = _gridManager.GetColoredBlockAtPosition(row, col);

            if (blockAtPosition == null)
                return connectedBlocks;
            
            queue.Enqueue((row, col));

            while (queue.Count > 0)
            {
                (int r, int c) = queue.Dequeue();
                if (visited[r, c] == currentSearchID) continue;
                visited[r, c] = currentSearchID;
                blockAtPosition = _gridManager.GetColoredBlockAtPosition(r, c);
                if (blockAtPosition == null) continue;
                connectedBlocks.Add(blockAtPosition);
                foreach ((int nr, int nc) in GetNeighbors(r, c))
                {
                    if (visited[nr, nc] == currentSearchID) continue;
                    ColoredBlock neighbor = _gridManager.GetColoredBlockAtPosition(nr, nc);
                    if (neighbor == null) continue;
                    if (neighbor.GetColorIndex() != blockAtPosition.GetColorIndex()) continue;
                    queue.Enqueue((nr, nc));
                }
            }

            return connectedBlocks;
        }
        
        public List<ObstacleBlock> FindAdjacentObstacles(List<ColoredBlock> matchedBlocks)
        {
            List<ObstacleBlock> adjacentObstacles = new();

            foreach (ColoredBlock block in matchedBlocks)
            {
                Vector2Int position = block.GetGridPosition();

                int row = position.y;
                int col = position.x;

                foreach ((int neighborRow, int neighborCol) in GetNeighbors(row, col))
                {
                    Block neighborBlock = _gridManager.GetBlockAtPosition(neighborRow, neighborCol);
                    if (neighborBlock is ObstacleBlock obstacle && !adjacentObstacles.Contains(obstacle))
                    {
                        adjacentObstacles.Add(obstacle);
                    }
                }
            }

            return adjacentObstacles;
        }


        public void SetGroupID(List<ColoredBlock> matchedBlocks, int groupID)
        {
            foreach (ColoredBlock block in matchedBlocks)
            {
                block.SetGroupID(groupID);
                block.SetGroupStatus(true);
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