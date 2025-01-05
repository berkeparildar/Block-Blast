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
        #region Variables

        #region Private Variables

        private BlastableBlock[,] _grid;
        private int[,] _visited;
        private GridData _gridData;
        private readonly Dictionary<int, List<BlastableBlock>> _currentGroups = new();
        private int _blockGroupID;
        private int _lastVisitedID;

        #endregion

        #region Serialized Variables

        [SerializeField] private GridFillController fillController;
        [SerializeField] private GridMatchController matchController;
        [SerializeField] private GridVisualController visualController;
        [SerializeField] private GridBlastController blastController;
        [SerializeField] private GridBackgroundController backgroundController;
        [SerializeField] private GridShuffleController shuffleController;

        #endregion

        #endregion

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
            foreach (List<BlastableBlock> group in _currentGroups.Values)
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
            List<BlastableBlock> blockGroup = _currentGroups[groupID];
            List<BlastableBlock> adjacentObstacles = matchController.FindAdjacentObstacles(blockGroup);
            
            List<BlastableBlock> blocksToBlast = new();
            blocksToBlast.AddRange(blockGroup);
            blocksToBlast.AddRange(adjacentObstacles);
            
            List<int> blastedPositions = blastController.Blast(blocksToBlast);
            
            blastController.UpdateGridAfterBlast(blastedPositions);
            fillController.RefillEmptyCells(blastedPositions);
            
            IdentifyMatchingGroups();
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
                    if (_grid[r, c] == null) continue;
                    List<BlastableBlock> matchedBlocks = matchController.CheckMatches(r, c, _visited, _lastVisitedID);
                    if (matchedBlocks.Count >= GameValues.MinimumMatchCount)
                    {
                        _currentGroups.Add(_blockGroupID, matchedBlocks);
                        visualController.UpdateAndChangeColoredBlockSprites(matchedBlocks);
                        GridMatchController.SetGroupID(matchedBlocks, _blockGroupID);
                        _blockGroupID++;
                    }
                    else
                    {
                        if (_grid[r, c].GetColor() < 0) continue;
                        _grid[r, c].SetGroupID(-1);
                        matchedBlocks = new List<BlastableBlock>() { _grid[r, c] };
                        visualController.UpdateAndChangeColoredBlockSprites(matchedBlocks);
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
    }
}
