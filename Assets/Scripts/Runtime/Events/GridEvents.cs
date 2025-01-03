using Runtime.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.Events
{
    public class GridEvents : MonoSingleton<GridEvents>
    {
        public UnityAction<int, int> OnGridSizeSet = delegate { };
        public UnityAction<int> OnBlockLanded = delegate { };
    }
}