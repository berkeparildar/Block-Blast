using System.Collections.Generic;
using Runtime.Data.ValueObjects;
using Runtime.Managers;
using UnityEngine;

namespace Runtime.Controllers
{
    public class GridMatchController : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        private readonly List<GridPosition> _connectedBlocks = new();
        private readonly Queue<GridPosition> _matchQueue = new();
        private readonly List<GridPosition> _adjacentObstacles = new();

        private GridData _gridData;
        
        public List<GridPosition> CheckMatches(GridPosition pos, int[,] visited, int lastVisitedID)
        {
            _matchQueue.Clear();
            _connectedBlocks.Clear();
            _matchQueue.Enqueue(pos);

            while (_matchQueue.Count > 0)
            {
                GridPosition currentPos = _matchQueue.Dequeue();
                if (visited[currentPos.Row, currentPos.Column] == lastVisitedID) continue;
                visited[currentPos.Row, currentPos.Column] = lastVisitedID;
                BlockManager currentBlockManager = gridManager.GetBlockAtPosition(currentPos.Row, currentPos.Column);
                if (!currentBlockManager.IsStationary()) return _connectedBlocks;
                _connectedBlocks.Add(currentPos);
                foreach (GridPosition neighborPos in GetNeighbors(currentPos))
                {
                    if (visited[neighborPos.Row, neighborPos.Column] == lastVisitedID) continue;
                    BlockManager neighbor = gridManager.GetBlockAtPosition(neighborPos.Row, neighborPos.Column);
                    if (neighbor is null) continue;
                    if (neighbor.GetColor() == currentBlockManager.GetColor())
                    {
                        _matchQueue.Enqueue(neighborPos);
                    }
                }
            }

            return new List<GridPosition>(_connectedBlocks);
        }
        
        public List<GridPosition> FindAdjacentObstacles(List<GridPosition> matchedBlocks)
        {
            _adjacentObstacles.Clear();
            foreach (GridPosition blockPos in matchedBlocks)
            {
                foreach (GridPosition neighbor in GetNeighbors(blockPos))
                {
                    BlockManager neighborBlockManager = gridManager.GetBlockAtPosition(neighbor.Row, neighbor.Column);
                    if (neighborBlockManager is null) continue;
                    if (neighborBlockManager.GetColor() < 0 && !_adjacentObstacles.Contains(neighbor))
                    { 
                        _adjacentObstacles.Add(neighbor);
                    }
                }
            }

            return new List<GridPosition>(_adjacentObstacles);
        }

        public void SetGroupID(List<GridPosition> matchedBlocks, int groupID)
        {
            foreach (GridPosition pos in matchedBlocks)
            {
                gridManager.GetBlockAtPosition(pos.Row, pos.Column).SetGroupID(groupID);
            }
        }

        private IEnumerable<GridPosition> GetNeighbors(GridPosition pos)
        {
            int rows = _gridData.GridRowSize;
            int cols = _gridData.GridColumnSize;

            if (pos.Row > 0) yield return new GridPosition(pos.Row - 1, pos.Column);
            if (pos.Row < rows - 1) yield return new GridPosition(pos.Row + 1, pos.Column);
            if (pos.Column > 0) yield return new GridPosition(pos.Row, pos.Column - 1);
            if (pos.Column < cols - 1) yield return new GridPosition(pos.Row, pos.Column + 1);
        }

        public void SetData(GridData data) => _gridData = data;
    }
}