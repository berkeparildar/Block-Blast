using System.Collections.Generic;
using System.Linq;
using Runtime.Blocks;
using Runtime.Data.ValueObjects;
using Runtime.Extensions;
using Runtime.Managers;
using UnityEngine;

namespace Runtime.Controllers
{
    public class GridShuffleController : MonoBehaviour
    {
        private GridData _gridData;
        [SerializeField] private GridManager gridManager;

        public void SetData(GridData data)
        {
            _gridData = data;
        }

        public void ForceGuaranteedMatch()
        {
            int rows = _gridData.GridRowSize;
            int cols = _gridData.GridColumnSize;

            List<ColoredBlock> allColoredBlocks = ExtractAllColoredBlocks();

            Dictionary<int, int> colorCounts = CountBlockColors(allColoredBlocks);

            int groupColorKey = -1;
            foreach (KeyValuePair<int, int> kvp in colorCounts)
            {
                if (kvp.Value >= GameValues.MinimumMatchCount)
                {
                    groupColorKey = kvp.Key;
                    break;
                }
            }

            if (groupColorKey == -1)
            {
                groupColorKey = colorCounts.OrderByDescending(x => x.Value).First().Key;
                int needed = GameValues.MinimumMatchCount - colorCounts[groupColorKey];
                for (int i = 0; i < needed; i++)
                {
                    ColoredBlock blockToRecolor = allColoredBlocks
                        .First(b => b.GetColorIndex() != groupColorKey);
                    blockToRecolor.SetColorIndex(groupColorKey);
                }
            }

            // 4) Grab exactly MinimumMatchCount blocks of that color
            List<ColoredBlock> groupedBlocks = allColoredBlocks
                .Where(b => b.GetColorIndex() == groupColorKey)
                .Take(GameValues.MinimumMatchCount)
                .ToList();

            // remove them from 'allColoredBlocks'
            foreach (ColoredBlock b in groupedBlocks)
                allColoredBlocks.Remove(b);

            // 5) Clear grid
            ClearGrid();

            // 6) Find a random placement for the matched blocks
            bool foundPlacement = TryGetRandomAdjacentPlacement(
                out int placementRow,
                out int startCol,
                GameValues.MinimumMatchCount
            );

            // If we failed to find an adjacent area, you might just place them
            // all in some valid positions anywhere. For example:
            if (!foundPlacement)
            {
                placementRow = 0;
                startCol = 0;
                Debug.LogWarning("Failed to find random adjacent placement, using fallback.");
            }

            // Place the matched blocks in the chosen row/columns
            for (int i = 0; i < groupedBlocks.Count; i++)
            {
                int c = startCol + i;
                gridManager.SetBlockAtPosition(placementRow, c, groupedBlocks[i]);
                groupedBlocks[i].SetInitialBlockPosition(c, placementRow);
                groupedBlocks[i].UpdateSortingOrder(placementRow);
            }

            // 7) Fill remaining grid positions
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    // If this position is already occupied or an obstacle, skip it
                    if (gridManager.GetBlockAtPosition(r, c) != null)
                        continue;

                    // Place a leftover block or dequeue a new one
                    ColoredBlock blockToPlace;
                    if (allColoredBlocks.Count > 0)
                    {
                        blockToPlace = allColoredBlocks[0];
                        allColoredBlocks.RemoveAt(0);
                    }
                    else
                    {
                        blockToPlace = GridFillController.DequeueBlock();
                    }

                    gridManager.SetBlockAtPosition(r, c, blockToPlace);
                    blockToPlace.SetInitialBlockPosition(c, r);
                    blockToPlace.UpdateSortingOrder(r);
                }
            }
        }

        private List<ColoredBlock> ExtractAllColoredBlocks()
        {
            int rows = _gridData.GridRowSize;
            int cols = _gridData.GridColumnSize;
            List<ColoredBlock> blocks = new();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Block b = gridManager.GetBlockAtPosition(r, c);
                    if (b is ColoredBlock cb)
                    {
                        blocks.Add(cb);
                    }
                }
            }

            return blocks;
        }

        private Dictionary<int, int> CountBlockColors(List<ColoredBlock> blocks)
        {
            Dictionary<int, int> colorCounts = new();
            foreach (ColoredBlock block in blocks)
            {
                int color = block.GetColorIndex();
                colorCounts.TryAdd(color, 0);
                colorCounts[color]++;
            }

            return colorCounts;
        }

        private void ClearGrid()
        {
            int rows = (int)_gridData.GridRowSize;
            int cols = (int)_gridData.GridColumnSize;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (gridManager.GetBlockAtPosition(r, c) is ColoredBlock)
                        gridManager.SetBlockAtPosition(r, c, null);
                }
            }
        }

        private List<(int row, int col)> GetValidPositions()
        {
            List<(int row, int col)> validPositions = new();
            int rows = _gridData.GridRowSize;
            int cols = _gridData.GridColumnSize;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Block b = gridManager.GetBlockAtPosition(r, c);
                    bool isObstacle = b != null && (b is ObstacleBlock);

                    if (!isObstacle)
                    {
                        validPositions.Add((r, c));
                    }
                }
            }

            return validPositions;
        }

        private bool TryGetRandomAdjacentPlacement(
            out int placementRow,
            out int startCol,
            int neededCount
        )
        {
            placementRow = -1;
            startCol = -1;

            int rows = _gridData.GridRowSize;
            int cols = _gridData.GridColumnSize;

            // Up to some max attempts to find a suitable row
            const int MAX_ATTEMPTS = 100;
            for (int i = 0; i < MAX_ATTEMPTS; i++)
            {
                int randomRow = Random.Range(0, rows);

                // We want to find a consecutive set of columns for neededCount blocks
                int possibleStartColMax = cols - neededCount;
                if (possibleStartColMax < 0) continue; // not enough columns at all

                int randomStartCol = Random.Range(0, possibleStartColMax + 1);

                // Check if each position (randomRow, randomStartCol..randomStartCol+neededCount-1)
                // is valid (not an obstacle).
                bool canPlace = true;
                for (int offset = 0; offset < neededCount; offset++)
                {
                    Block b = gridManager.GetBlockAtPosition(randomRow, randomStartCol + offset);
                    bool isObstacle = b != null && !(b is ColoredBlock);
                    if (isObstacle)
                    {
                        canPlace = false;
                        break;
                    }
                }

                if (canPlace)
                {
                    placementRow = randomRow;
                    startCol = randomStartCol;
                    return true;
                }
            }

            return false;
        }
    }
}