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

        private Block[,] m_Grid;
        private int[,] m_Visited;
        private GridData _gridData;
        private readonly Dictionary<int, List<ColoredBlock>> m_CurrentGroups = new();
        private int m_BlockGroupID;
        private int m_LastVisitedID;

        #endregion

        #region Serialized Variables

        [SerializeField] private GridSO _gridSO;
        [SerializeField] private GridFillController fillController;
        [SerializeField] private GridMatchController matchController;
        [SerializeField] private GridVisualController visualController;
        [SerializeField] private GridBlastController blastController;
        [SerializeField] private GridBackgroundController backgroundController;

        #endregion

        #endregion
    
        public void SetGridData(GridData gridData)
        {
            _gridData = gridData;
            m_Visited = new int[_gridData.GridRowSize, _gridData.GridColumnSize];
            SendDataToControllers();
            InitializeGrid();
            GridEvents.Instance.OnGridSizeSet.Invoke(_gridData.GridColumnSize, _gridData.GridRowSize);
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

        private void UnsubscribeEvents()
        {
            GridEvents.Instance.OnBlockLanded -= UpdateBlockGroupIcons;
            InputEvents.Instance.OnTap -= CheckInput;
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }
        
        private void SendDataToControllers()
        {
            matchController.SetData(_gridData);
            blastController.SetData(_gridData);
            fillController.SetData(_gridData);
            visualController.SetData(_gridData);
            backgroundController.SetData(_gridData);
        }

        private void InitializeGrid()
        {
            fillController.InitializePool();
            m_Grid = fillController.CreateGrid();
            IdentifyMatchingGroups();
            backgroundController.SetGridBackground();
            foreach (List<ColoredBlock> group in m_CurrentGroups.Values)
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
            if (m_Grid[row, col] == null) return; 
            Block tappedBlock = m_Grid[row, col];
            if (tappedBlock is ColoredBlock block)
                ColoredBlockTapped(block);
        }
        
        private void ColoredBlockTapped(ColoredBlock coloredBlock)
        {
            if (!coloredBlock.IsStationary() || !coloredBlock.HasGroup()) return;
            
            int groupID = coloredBlock.GetGroupID();
            List<ColoredBlock> blockGroup = m_CurrentGroups[groupID];
            List<ObstacleBlock> adjacentObstacles = matchController.FindAdjacentObstacles(blockGroup);
            
            List<Block> blocksToBlast = new();
            blocksToBlast.AddRange(blockGroup);
            blocksToBlast.AddRange(adjacentObstacles);
            
            List<int> blastedPositions = blastController.Blast(blocksToBlast);
            
            blastController.UpdateGridAfterBlast(blastedPositions);
            fillController.RefillEmptyCells(blastedPositions);
            
            IdentifyMatchingGroups();
        }
        
        private void IdentifyMatchingGroups()
        {
            m_LastVisitedID++;
            m_CurrentGroups.Clear();
            
            int rows = _gridData.GridRowSize;
            int cols = _gridData.GridColumnSize; 
            
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (m_Visited[r, c] == m_LastVisitedID) continue;
                    List<ColoredBlock> matchedBlocks = matchController.CheckMatches(r, c, m_Visited, m_LastVisitedID);
                    if (matchedBlocks.Count >= GameValues.MinimumMatchCount)
                    {
                        m_CurrentGroups.Add(m_BlockGroupID, matchedBlocks);
                        visualController.UpdateAndChangeColoredBlockSprites(matchedBlocks);
                        matchController.SetGroupID(matchedBlocks, m_BlockGroupID);
                        m_BlockGroupID++;
                    }
                    else
                    {
                        if (m_Grid[r, c] is not ColoredBlock block) continue;
                        block.SetGroupStatus(false);
                        matchedBlocks = new List<ColoredBlock>() { block };
                        visualController.UpdateAndChangeColoredBlockSprites(matchedBlocks);
                    }
                }
            }
        }

        private void UpdateBlockGroupIcons(int groupID)
        {
            visualController.UpdateAndChangeColoredBlockSprites(m_CurrentGroups[groupID]);
        }
        
        public void SetBlockAtPosition(int row, int col, Block block)
        {
            m_Grid[row, col] = block;
        }
        
        public Block GetBlockAtPosition(int row, int col)
        {
            return m_Grid[row, col];
        }
        
        public ColoredBlock GetColoredBlockAtPosition(int row, int col)
        {
            if (m_Grid[row, col] is ColoredBlock) return m_Grid[row, col] as ColoredBlock;
            return null;
        }
    }
}
