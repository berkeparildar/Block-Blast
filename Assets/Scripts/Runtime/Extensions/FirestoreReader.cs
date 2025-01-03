using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Firestore;
using Runtime.Data.ValueObjects;
using UnityEngine;

namespace Runtime.Extensions
{
    public class FirestoreReader
    {
        private FirebaseFirestore _db;
        private const string CollectionName = "levels";

        public async Task<LevelData> GetLevelData(int level)
        {
            DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus != DependencyStatus.Available)
            {
                return default;
            }
            
            _db = FirebaseFirestore.DefaultInstance;
            try
            {
                DocumentSnapshot snapshot = await _db.Collection(CollectionName).Document(level.ToString())
                    .GetSnapshotAsync();
                if (!snapshot.Exists)
                {
                    int lastLevel = level - 1;
                    DocumentSnapshot oldSnapshot = await _db.Collection(CollectionName)
                        .Document(lastLevel.ToString()).GetSnapshotAsync();
                    LevelData oldLevelData = ParseLevelData(oldSnapshot.ToDictionary());
                    return oldLevelData;
                }
                LevelData levelData = ParseLevelData(snapshot.ToDictionary());
                return levelData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading level data: {ex}");
                return default;
            }
        }
        
        private static LevelData ParseLevelData(Dictionary<string, object> documentData)
        {
            int level = Convert.ToInt32(documentData["Level"]);
            var targetDataList = (List<object>)documentData["Targets"];
            int[] targets = targetDataList.Select(item => Convert.ToInt32(item)).ToArray();
            var targetCountsDataList = (List<object>)documentData["TargetCounts"];
            int[] targetCounts = targetCountsDataList.Select(item => Convert.ToInt32(item)).ToArray();
            int moveLimit = Convert.ToInt32(documentData["MoveLimit"]);
            
            Dictionary<string, object> gridDataDict = (Dictionary<string, object>) documentData["GridData"];
            int gridRowSize = Convert.ToInt32(gridDataDict["GridRowSize"]);
            int gridColumnSize = Convert.ToInt32(gridDataDict["GridColumnSize"]);
            int colorCount = Convert.ToInt32(gridDataDict["ColorCount"]);
            
            List<object> gridRows = (List<object>) gridDataDict["Grid"]; 
            int[,] gridArray = new int[gridRowSize, gridColumnSize];

            foreach (var rowObj in gridRows)
            {
                var rowDict = (Dictionary<string, object>) rowObj;

                int rowIndex = Convert.ToInt32(rowDict["rowIndex"]);
                var cells = (List<object>) rowDict["cells"];

                for (int colIndex = 0; colIndex < cells.Count; colIndex++)
                {
                    gridArray[rowIndex, colIndex] = Convert.ToInt32(cells[colIndex]);
                }
            }
            
            GridData gridData = new()
            {
                GridRowSize = gridRowSize,
                GridColumnSize = gridColumnSize,
                Grid = gridArray,
                ColorCount = colorCount
            };
            
            return new LevelData
            {
                GridData = gridData,
                Level = level,
                Targets = targets,
                TargetCounts = targetCounts,
                MoveLimit = moveLimit
            };
        }
    }
}
