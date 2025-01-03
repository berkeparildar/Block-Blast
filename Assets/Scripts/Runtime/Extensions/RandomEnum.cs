using UnityEngine;

namespace Runtime.Extensions
{
    public static class RandomEnum
    {
        public static T GetRandomEnum<T>(int limit)
        {
            System.Array values = System.Enum.GetValues(typeof(T));
            limit = Mathf.Clamp(values.Length, 0, limit);
            return (T)values.GetValue(Random.Range(0, limit));
        }
    }
}