using System;
using Runtime.Data.ValueObjects;
using Runtime.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.Events
{
    public class GameEvents : MonoSingleton<GameEvents>
    {
        public UnityAction<int, int> OnGridSizeSet = delegate { };
        public UnityAction<GridPosition, GridPosition, int> OnBlockLanded = delegate { };
        public UnityAction<Vector2> OnTap = delegate { };
        public UnityAction OnBlast = delegate { };
        public UnityAction<int> OnBlockBlasted = delegate { };
        public UnityAction OnLevelInitialized = delegate { };
        public UnityAction OnLevelFinished = delegate { };
        public Func<bool> IsLevelFinished = delegate { return false; };
    }
}