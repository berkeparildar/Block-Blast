using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Extensions
{
    public static class GameValues
    {
        public const int MinimumMatchCount = 2;
        private const int GroupSizeOne = 3;
        private const int GroupSizeTwo = 4;
        private const int GroupSizeThree = 5;

        public static Color GetColor(int colorIndex)
        {
            return colorIndex switch
            {
                0 => new Color(0.0f, 0.478f, 1.0f),
                1 => new Color(0.204f, 0.780f, 0.349f),
                2 => new Color(1.0f, 0.176f, 0.333f),
                3 => new Color(0.686f, 0.322f, 0.871f),
                4 => new Color(1.0f, 0.231f, 0.188f),
                5 => new Color(1.0f, 0.800f, 0.0f),
                -1 => new Color(0.110f, 0.110f, 0.118f),
                _ => new Color(1.0f, 1.0f, 1.0f, 1.0f)
            };
        }

        public static int GetGroupSymbolIndex(int groupSize)
        {
            return groupSize switch
            {
                <= GroupSizeOne => 0,
                > GroupSizeOne and <= GroupSizeTwo => 1,
                > GroupSizeTwo and <= GroupSizeThree => 2,
                _ => 3
            };
        }
    }
}