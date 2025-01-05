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
        
        public void UpdateAndChangeColoredBlockSprites(List<BlastableBlock> connectedGroup)
        { ;
            
            int symbolIndex = GameValues.GetGroupSymbolIndex(connectedGroup.Count);
            foreach (BlastableBlock block in connectedGroup.Where(block => block))
            {
                block.UpdateSymbol(symbolIndex);
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