using System.Collections.Generic;
using System.Linq;
using Runtime.Blocks;
using Runtime.Data.ValueObjects;
using Runtime.Enums;
using Runtime.Managers;
using UnityEngine;

namespace Runtime.Controllers
{
    public class GridVisualController : MonoBehaviour
    {
        private const int GroupSizeOne = 3;
        private const int GroupSizeTwo = 4;
        private const int GroupSizeThree = 5;
        
        [SerializeField] private GridManager gridManager;
        private GridData m_GridData;
        
        public void UpdateAndChangeColoredBlockSprites(List<ColoredBlock> connectedGroup)
        {
            bool instantChange = connectedGroup.TrueForAll(block => block.IsStationary()) || 
                                 connectedGroup.TrueForAll(block => !block.IsStationary());
            
            ColoredBlockSprite sprite = FindSpriteForColoredBlocks(connectedGroup.Count);
            foreach (ColoredBlock block in connectedGroup.Where(block => block))
            {
                block.SetSprite(sprite);
                if (instantChange) block.UpdateBlockVisual();
            }
        }
        
        private ColoredBlockSprite FindSpriteForColoredBlocks(int groupSize)
        {
            if (groupSize <= GroupSizeOne)
            {
                return ColoredBlockSprite.Default;
            }
            if (groupSize is > GroupSizeOne and <= GroupSizeTwo)
            {
                return ColoredBlockSprite.A;
            }
            if (groupSize is > GroupSizeTwo and <= GroupSizeThree)
            {
                return ColoredBlockSprite.B;
            }
            return ColoredBlockSprite.C;
        }
        
        public void SetData(GridData data) => m_GridData = data;
    }
}