using Runtime.Data.ValueObjects;
using UnityEngine;

namespace Runtime.Data.UnityObjects
{
    [CreateAssetMenu(fileName = "GridSO", menuName = "SO/GridSO", order = 0)]
    public class GridSO : ScriptableObject
    {
        public GridData GridData;
    }
}