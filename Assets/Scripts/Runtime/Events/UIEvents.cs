using System.Collections.Generic;
using Runtime.Extensions;
using UnityEngine.Events;

namespace Runtime.Events
{
    public class UIEvents : MonoSingleton<UIEvents>
    {
        public UnityAction<int[], int[], int> OnTargetsSet = delegate { };
        public UnityAction<int> OnPlayerMove = delegate { };
        public UnityAction<int> OnBlockBlasted = delegate { };
        public UnityAction OnLevelWin  = delegate { };
        public UnityAction OnLevelLose = delegate { };
        public UnityAction ResetLevel = delegate { };
    }
}