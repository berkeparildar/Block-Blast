using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.Blocks;
using Runtime.Controllers;
using Runtime.Data.UnityObjects;
using Runtime.Data.ValueObjects;
using Runtime.Events;
using Runtime.Extensions;
using UnityEngine;

namespace Runtime.Managers
{
    public class GridManager : MonoBehaviour
    {
        
        private BlastableBlock[,] _grid;
        private int[,] _visited;
        private GridData _gridData;
        private readonly Dictionary<int, List<GridPosition>> _currentGroups = new();
        private int _blockGroupID;
        private int _lastVisitedID;

        [SerializeField] private GridFillController fillController;
        [SerializeField] private GridMatchController matchController;
        [SerializeField] private GridVisualController visualController;
        [SerializeField] private GridBlastController blastController;
        [SerializeField] private GridBackgroundController backgroundController;
        [SerializeField] private GridShuffleController shuffleController;

        private readonly List<GridPosition> _connectedBlocks = new();
        private readonly Queue<GridPosition> _matchQueue = new();
        
        private void Awake()
        {
            fillController.InitializePool();
        }

        public void SetGridData(GridData gridData)
        {
            _gridData = gridData;
            _visited = new int[_gridData.GridRowSize, _gridData.GridColumnSize];
            SendDataToControllers();
            InitializeGrid();
            GridEvents.Instance.OnGridSizeSet.Invoke(_gridData.GridColumnSize, _gridData.GridRowSize);
        }

        public void ResetGrid()
        {
            for (int i = 0; i < _gridData.GridRowSize; i++)
            {
                for (int j = 0; j < _gridData.GridColumnSize; j++)
                {
                    if (_grid[i, j] == null) continue;
                    GridFillController.EnqueueBlock(_grid[i, j]);
                    _visited[i, j] = 0;
                    _blockGroupID = 0;
                    _lastVisitedID = 0;
                    _currentGroups.Clear();
                }
            }
            _grid = null;
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            GridEvents.Instance.OnBlockLanded += UpdateBlockGroupIcons;
            InputEvents.Instance.OnTap += CheckInput;
        }
        
        private void SendDataToControllers()
        {
            matchController.SetData(_gridData);
            blastController.SetData(_gridData);
            fillController.SetData(_gridData);
            backgroundController.SetData(_gridData);
            shuffleController.SetData(_gridData);
        }

        private void InitializeGrid()
        {
            _grid = fillController.CreateGrid();
            IdentifyMatchingGroups();
            backgroundController.SetGridBackground();
            foreach (List<GridPosition> group in _currentGroups.Values)
            {
                visualController.UpdateAndChangeColoredBlockSprites(group);
            }
        }
        
        private void CheckInput(Vector2 tapPosition)
        {
            int col = Mathf.RoundToInt(tapPosition.x);
            int row = Mathf.RoundToInt(tapPosition.y);
            if (IsWithinGridBounds(row, col))
                GridTapped(row, col);
        }
        
        private bool IsWithinGridBounds(int row, int col)
        {
            return row >= 0 && row < _gridData.GridRowSize && col >= 0 && col < _gridData.GridColumnSize;
        }

        private void GridTapped(int row, int col)
        {
            if (_grid[row, col] == null) return; 
            BlastableBlock tappedBlock = _grid[row, col];
            if (tappedBlock.GetColor() >= 0)
                ColoredBlockTapped(tappedBlock);
        }
        
        private void ColoredBlockTapped(BlastableBlock coloredBlock)
        {
            if (!coloredBlock.IsStationary() || coloredBlock.GetGroupID() < 0) return;
            
            int groupID = coloredBlock.GetGroupID();
            List<GridPosition> blockGroup = _currentGroups[groupID];
            
            List<int> blastedPositions = blastController.Blast(blockGroup);
            
            blastController.UpdateGridAfterBlast(blastedPositions);
            fillController.RefillEmptyCells(blastedPositions);
            
            IdentifyMatchingGroups();
        }

        private void CheckMatches(GridPosition pos)
        {
            _matchQueue.Clear();
            _connectedBlocks.Clear();
            
            _matchQueue.Enqueue(pos);

            while (_matchQueue.Count > 0)
            {
                GridPosition currentPos = _matchQueue.Dequeue();
                if (_visited[currentPos.Row, currentPos.Column] == _lastVisitedID) continue;
                _visited[currentPos.Row, currentPos.Column] = _lastVisitedID;
                BlastableBlock currentBlock = _grid[currentPos.Row, currentPos.Column];
                _connectedBlocks.Add(currentPos);
                foreach (GridPosition neighborPos in GetNeighbors(currentPos))
                {
                    if (_visited[neighborPos.Row, neighborPos.Column] == _lastVisitedID) continue;
                    BlastableBlock neighbor = _grid[neighborPos.Row, neighborPos.Column];
                    if (neighbor is null) continue;
                    if (neighbor.GetColor() < 0)
                    {
                        if (!_connectedBlocks.Contains(neighborPos)) 
                            _connectedBlocks.Add(neighborPos);
                    }
                    else if (neighbor.GetColor() == currentBlock.GetColor())
                    {
                        _matchQueue.Enqueue(neighborPos);
                    }
                }
            }
        }
        
        private void IdentifyMatchingGroups()
        {
            _lastVisitedID++;
            _currentGroups.Clear();
            
            int rows = _gridData.GridRowSize;
            int cols = _gridData.GridColumnSize; 
            
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (_visited[r, c] == _lastVisitedID) continue;
                    if (_grid[r, c].GetColor() < 0) continue;
                    if (!_grid[r, c]) continue;
                    CheckMatches(new GridPosition(r, c));
                    int coloredBlockCount = 0;
                    
                    foreach (GridPosition pos in _connectedBlocks)
                    {
                        if (_grid[pos.Row, pos.Column].GetColor() >= 0) coloredBlockCount++;
                    }
                    
                    if (coloredBlockCount >= GameValues.MinimumMatchCount)
                    {
                        _currentGroups.Add(_blockGroupID, new List<GridPosition>(_connectedBlocks));
                        visualController.UpdateAndChangeColoredBlockSprites(_connectedBlocks);
                        matchController.SetGroupID(_connectedBlocks, _blockGroupID);
                        _blockGroupID++;
                    }
                    else
                    {
                        if (_grid[r, c].GetColor() < 0) continue;
                        _grid[r, c].SetGroupID(-1);
                        visualController.UpdateAndChangeColoredBlockSprites( new List<GridPosition>() { new GridPosition(r, c) } );
                    }
                }
            }

            if (_currentGroups.Count == 0)
            {
                StartCoroutine(ShuffleCoroutine());
                IEnumerator ShuffleCoroutine()
                {
                    yield return new WaitForSeconds(2);
                    shuffleController.ForceGuaranteedMatch();
                    IdentifyMatchingGroups();
                }
            }
        }

        private void UpdateBlockGroupIcons(int groupID)
        {
            visualController.UpdateAndChangeColoredBlockSprites(_currentGroups[groupID]);
        }
        
        public void SetBlockAtPosition(int row, int col, BlastableBlock block)
        {
            _grid[row, col] = block;
        }
        
        public BlastableBlock GetBlockAtPosition(int row, int col)
        {
            return _grid[row, col];
        }
        
        private IEnumerable<GridPosition> GetNeighbors(GridPosition pos)
        {
            int rows = _gridData.GridRowSize;
            int cols = _gridData.GridColumnSize;

            if (pos.Row > 0) yield return new GridPosition(pos.Row- 1, pos.Column);
            if (pos.Row < rows - 1) yield return  new GridPosition(pos.Row + 1, pos.Column);
            if (pos.Column > 0) yield return  new GridPosition(pos.Row, pos.Column - 1);
            if (pos.Column < cols - 1) yield return  new GridPosition(pos.Row, pos.Column + 1);
        }

    }
}
