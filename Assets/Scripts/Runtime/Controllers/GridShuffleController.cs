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
        private const int MaxAttempts = 100;
        private readonly Dictionary<int, int> _colorCounts = new();
        private readonly List<BlastableBlock> _coloredBlocks = new();
        [SerializeField] private GridManager gridManager;

        public void SetData(GridData data)
        {
            _gridData = data;
        }

        public void ForceGuaranteedMatch()
        {
            ExtractAllColoredBlocks();
            CountBlockColors(_coloredBlocks);

            int groupColorKey = -1;
            foreach (KeyValuePair<int, int> kvp in _colorCounts)
            {
                if (kvp.Value >= GameValues.MinimumMatchCount)
                {
                    groupColorKey = kvp.Key;
                    break;
                }
            }

            if (groupColorKey == -1)
            {
                groupColorKey = _colorCounts.OrderByDescending(x => x.Value).First().Key;
                int needed = GameValues.MinimumMatchCount - _colorCounts[groupColorKey];
                for (int i = 0; i < needed; i++)
                {
                    BlastableBlock blockToRecolor = _coloredBlocks
                        .First(b => b.GetColor() != groupColorKey);
                    blockToRecolor.SetColor(groupColorKey);
                }
            }
            _colorCounts.Clear();

            List<BlastableBlock> groupedBlocks = _coloredBlocks
                .Where(b => b.GetColor() == groupColorKey)
                .Take(GameValues.MinimumMatchCount)
                .ToList();

            // remove them from 'allColoredBlocks'
            foreach (BlastableBlock b in groupedBlocks)
                _coloredBlocks.Remove(b);

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
            }

            // Place the matched blocks in the chosen row/columns
            for (int i = 0; i < groupedBlocks.Count; i++)
            {
                int c = startCol + i;
                gridManager.SetBlockAtPosition(placementRow, c, groupedBlocks[i]);
                groupedBlocks[i].SetInitialBlockPosition(c, placementRow);
                groupedBlocks[i].UpdateSortingOrder();
            }

            // 7) Fill remaining grid positions
            for (int r = 0; r < _gridData.GridRowSize; r++)
            {
                for (int c = 0; c < _gridData.GridColumnSize; c++)
                {
                    // If this position is already occupied or an obstacle, skip it
                    if (gridManager.GetBlockAtPosition(r, c) != null)
                        continue;

                    // Place a leftover block or dequeue a new one
                    BlastableBlock blockToPlace;
                    if (_coloredBlocks.Count > 0)
                    {
                        blockToPlace = _coloredBlocks[0];
                        _coloredBlocks.RemoveAt(0);
                    }
                    else
                    {
                        blockToPlace = GridFillController.DequeueBlock();
                    }

                    gridManager.SetBlockAtPosition(r, c, blockToPlace);
                    blockToPlace.SetInitialBlockPosition(c, r);
                    blockToPlace.UpdateSortingOrder();
                }
            }
            _coloredBlocks.Clear();
        }

        private void ExtractAllColoredBlocks()
        {
            
            for (int r = 0; r < _gridData.GridRowSize; r++)
            {
                for (int c = 0; c < _gridData.GridColumnSize; c++)
                {
                    BlastableBlock b = gridManager.GetBlockAtPosition(r, c);
                    if (b.GetColor() >= 0)
                    {
                        _coloredBlocks.Add(b);
                    }
                }
            }
        }

        private void CountBlockColors(List<BlastableBlock> blocks)
        {
            foreach (BlastableBlock block in blocks)
            {
                int color = block.GetColor();
                if (color < 0) continue;
                _colorCounts.TryAdd(color, 0);
                _colorCounts[color]++;
            }
        }

        private void ClearGrid()
        {
            for (int r = 0; r < _gridData.GridRowSize; r++)
            {
                for (int c = 0; c < _gridData.GridColumnSize; c++)
                {
                    if (gridManager.GetBlockAtPosition(r, c).GetColor() >= 0)
                        gridManager.SetBlockAtPosition(r, c, null);
                }
            }
        }

        private bool TryGetRandomAdjacentPlacement(out int placementRow, out int startCol, int neededCount)
        {
            placementRow = -1;
            startCol = -1;

            for (int i = 0; i < MaxAttempts; i++)
            {
                int randomRow = Random.Range(0, _gridData.GridRowSize);
                int possibleStartColMax = _gridData.GridColumnSize - neededCount;
                if (possibleStartColMax < 0) continue; 

                int randomStartCol = Random.Range(0, possibleStartColMax + 1);
                
                bool canPlace = true;
                for (int offset = 0; offset < neededCount; offset++)
                {
                    BlastableBlock b = gridManager.GetBlockAtPosition(randomRow, randomStartCol + offset);
                    bool isObstacle = b && b.GetColor() < 0;
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