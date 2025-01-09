using System.Collections.Generic;
using System.Linq;
using Runtime.Blocks;
using Runtime.Data.ValueObjects;
using Runtime.Enums;
using Runtime.Extensions;
using Runtime.Managers;
using UnityEngine;

namespace Runtime.Controllers
{
    public class GridVisualController : MonoBehaviour
    {
        
        [SerializeField] private GridManager gridManager;
        
        public void UpdateAndChangeColoredBlockSprites(List<GridPosition> connectedGroup)
        { ;
            
            int symbolIndex = GameValues.GetGroupSymbolIndex(connectedGroup.Count);
            foreach (GridPosition pos in connectedGroup)
            {
                gridManager.GetBlockAtPosition(pos.Row, pos.Column).UpdateSymbol(symbolIndex);
            }
        }

        /*private bool CheckGroupMovementStatus(List<BlastableBlock> block)
        {
            foreach (BlastableBlock b in block)
            {

            }
        }*/
    }
}