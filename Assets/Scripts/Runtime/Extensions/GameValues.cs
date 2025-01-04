using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Extensions
{
    public static class GameValues
    {
        public const int MinimumMatchCount = 2;

        public static Color GetColor(int colorIndex)
        {
            switch (colorIndex)
            {
                case 0:
                    return new Color(0.0f, 0.478f, 1.0f);
                case 1:
                    return new Color(0.204f, 0.780f, 0.349f);
                case 2:
                    return new Color(1.0f, 0.176f, 0.333f);
                case 3:
                    return new Color(0.686f, 0.322f, 0.871f);
                case 4:
                    return new Color(1.0f, 0.231f, 0.188f);
                case 5:
                    return new Color(1.0f, 0.800f, 0.0f);
                case -1:
                    return new Color(0.110f, 0.110f, 0.118f);
            }
            return new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
    }
}