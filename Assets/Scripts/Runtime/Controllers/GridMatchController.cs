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

        public void SetGroupID(List<GridPosition> matchedBlocks, int groupID)
        {
            foreach (GridPosition pos in matchedBlocks)
            {
                _gridManager.GetBlockAtPosition(pos.Row, pos.Column).SetGroupID(groupID);
            }
        }
        public void SetData(GridData data) => m_GridData = data;
    }
}