using Runtime.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.Events
{
    public class InputEvents : MonoSingleton<InputEvents>
    {
        public UnityAction<Vector2> OnTap = delegate { };
    }
}