using System;
using Runtime.Extensions;
using UnityEngine.Events;

namespace Runtime.Events
{
    public class LevelEvents : MonoSingleton<LevelEvents>
    {
        public UnityAction OnBlast = delegate { };
        public UnityAction<int> OnBlockBlasted = delegate { };
        public UnityAction OnLevelInitialized = delegate { };
        public UnityAction OnLevelFinished = delegate { };
        public Func<bool> IsLevelFinished = delegate { return false; };
    }
}