using Newtonsoft.Json;
using Runtime.Data.ValueObjects;
using UnityEngine;

namespace Runtime.Extensions
{
    public class LevelDataDeserializer
    {
        public static LevelData DeserializeLevelData(string json)
        {
            LevelData levelData = JsonConvert.DeserializeObject<LevelData>(json);
            return levelData;
        }
    }
}