using System.Collections;
using System.Collections.Generic;
using Runtime.Controllers;
using Runtime.Data.ValueObjects;
using Runtime.Events;
using Runtime.Extensions;
using UnityEngine;

namespace Runtime.Managers
{
    public class GridManager : MonoBehaviour
    {
        private BlockManager[,] _grid;
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
            GameEvents.Instance.OnGridSizeSet.Invoke(_gridData.GridColumnSize, _gridData.GridRowSize);
        }

        public void ResetGrid()
        {
            for (int i = 0; i < _gridData.GridRowSize; i++)
            {
                for (int j = 0; j < _gridData.GridColumnSize; j++)
                {
                    if (_grid[i, j] is null) continue;
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
            GameEvents.Instance.OnBlockLanded += CheckMatchesOfLandedBlock;
            GameEvents.Instance.OnTap += CheckInput;
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
            if (_grid[row, col].GetColor() < 0) return;
            BlockManager tappedBlockManager = _grid[row, col];
            if (tappedBlockManager.GetColor() >= 0)
                ColoredBlockTapped(tappedBlockManager);
        }
        
        private void ColoredBlockTapped(BlockManager coloredBlockManager)
        {
            if (!coloredBlockManager.IsStationary() || coloredBlockManager.GetGroupID() < 0) return;
            
            int groupID = coloredBlockManager.GetGroupID();
            List<GridPosition> blocksToBeBlasted = new();
            List<GridPosition> blockGroup = _currentGroups[groupID];
            List<GridPosition> obstacleGroup = matchController.FindAdjacentObstacles(blockGroup);
            blocksToBeBlasted.AddRange(blockGroup);
            blocksToBeBlasted.AddRange(obstacleGroup);
            var blastedPositions = blastController.Blast(blocksToBeBlasted);
            _currentGroups.Remove(groupID);
            blastController.UpdateGridAfterBlast(blastedPositions);
            fillController.RefillEmptyCells(blastedPositions);
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
                    if (_grid[r, c] is null) continue;
                    if (_visited[r, c] == _lastVisitedID) continue;
                    if (_grid[r, c].GetColor() < 0) continue;
                    var matchedBlocks = matchController.CheckMatches(new GridPosition(r, c), _visited, _lastVisitedID);
                    
                    if (matchedBlocks.Count >= GameValues.MinimumMatchCount)
                    {
                        _currentGroups.Add(_blockGroupID, matchedBlocks);
                        matchController.SetGroupID(matchedBlocks, _blockGroupID);
                        visualController.UpdateAndChangeColoredBlockSprites(matchedBlocks);
                        _blockGroupID++;
                    }
                    else
                    {
                        _grid[r, c].SetGroupID(-1);
                    }
                }
            }
        }

        public void CheckMatchesOfLandedBlock(GridPosition newPos, GridPosition oldPos, int currentGroupID)
        {
            StopAllCoroutines();
            _lastVisitedID++;
            var matches = matchController.CheckMatches(new GridPosition(newPos.Row, newPos.Column), _visited, _lastVisitedID);
            
            if (matches.Count >= GameValues.MinimumMatchCount)
            {
                foreach (var match in matches)
                {
                    if (_currentGroups.ContainsKey(_grid[match.Row, match.Column].GetGroupID()))
                    {
                        _currentGroups.Remove(_grid[match.Row, match.Column].GetGroupID());
                    }
                }
                matchController.SetGroupID(matches, _blockGroupID);
                _currentGroups.Add(_blockGroupID, matches);
                visualController.UpdateAndChangeColoredBlockSprites(matches);
                _blockGroupID++;
            }
            else
            {
                if (_grid[newPos.Row, newPos.Column].GetGroupID() != -1)
                {
                    if (_currentGroups.ContainsKey(currentGroupID))
                    {
                        _currentGroups[currentGroupID].RemoveAll(pos => pos.Row == oldPos.Row && pos.Column == oldPos.Column);
                        if (_currentGroups[currentGroupID].Count < GameValues.MinimumMatchCount)
                        {
                            for (int i = 0; i < _currentGroups[currentGroupID].Count; i++)
                            {
                                var blockPos = _currentGroups[currentGroupID][i];
                                _grid[blockPos.Row, blockPos.Column].SetGroupID(-1);
                            }
                            _currentGroups.Remove(currentGroupID);
                        }
                    }   
                    
                    _grid[newPos.Row, newPos.Column].SetGroupID(-1);
                }
            }

            if (_currentGroups.Count != 0) return;
            StartCoroutine(ShuffleCoroutine());
            return;

            IEnumerator ShuffleCoroutine()
            {
                yield return new WaitForSeconds(2);
                shuffleController.ForceGuaranteedMatch();
                IdentifyMatchingGroups();
            }
        }
        
        public void SetBlockAtPosition(int row, int col, BlockManager blockManager)
        {
            _grid[row, col] = blockManager;
        }
        
        public BlockManager GetBlockAtPosition(int row, int col)
        {
            return _grid[row, col];
        }
    }
}
