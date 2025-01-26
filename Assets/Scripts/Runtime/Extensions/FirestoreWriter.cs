using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Runtime.Data.ValueObjects;
using UnityEngine;

namespace Runtime.Extensions
{
    public class FirestoreWriter
    {
        private string filePath = "Assets/Resources/Levels.json";
        public void WriteLevelDataToJson(LevelData[] levelDataArray)
        {
            try
            {
                string json = JsonConvert.SerializeObject(levelDataArray, Formatting.Indented);
                File.WriteAllText(filePath, json, Encoding.UTF8);
                Debug.Log("Level data successfully written to JSON.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to write level data to JSON: {ex.Message}");
            }
        }

        public void SaveLevelData(LevelData levelData)
        {
            List<LevelData> levelDataList = new List<LevelData>();
            if (File.Exists(filePath))
            {
                string existingJson = File.ReadAllText(filePath);
                if (levelDataList.Exists(data => data.Level == levelData.Level))
                {
                    return;
                }
                levelDataList = JsonConvert.DeserializeObject<List<LevelData>>(existingJson) ?? new List<LevelData>();
                levelDataList.Add(levelData);
                WriteLevelDataToJson(levelDataList.ToArray());
            }
        }
        
        public LevelData GetRandomLevelData()
        {
            if (File.Exists(filePath))
            {
                string existingJson = File.ReadAllText(filePath);
                var levelDataList = JsonConvert.DeserializeObject<List<LevelData>>(existingJson) ?? new List<LevelData>();
                if (levelDataList.Count > 0)
                {
                    return levelDataList[Random.Range(0, levelDataList.Count)];
                }
            }
            return new LevelData();
        }
    }
}