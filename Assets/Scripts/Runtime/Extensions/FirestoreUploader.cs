using System.Collections.Generic;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

namespace Runtime.Extensions
{
    public class FirestoreUploader : MonoBehaviour
    {
        private FirebaseFirestore db;

        public void Start()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    db = FirebaseFirestore.DefaultInstance;
                    var gridRows = new List<Dictionary<string, object>>
                    {
                        new()
                        {
                            { "rowIndex", 0 },
                            { "cells", new List<int> { 0, 1, 2, 0, 1 } }
                        },
                        new()
                        {
                            { "rowIndex", 1 },
                            { "cells", new List<int> { 2, 2, 0, 1, 1 } }
                        },
                        new()
                        {
                            { "rowIndex", 2 },
                            { "cells", new List<int> { 0, 0, 1, 2, 2 } }
                        },
                        new()
                        {
                            { "rowIndex", 3 },
                            { "cells", new List<int> { 1, 1, 0, 2, 0 } }
                        },
                        new()
                        {
                            { "rowIndex", 4 },
                            { "cells", new List<int> { 2, 0, 0, 1, 1 } }
                        },
                    };
                    
                    Dictionary<string, object> gridData = new()
                    {
                        { "GridRowSize", 5},
                        { "GridColumnSize", 5 },
                        { "ColorCount", 3 },
                        { "Grid", gridRows } 
                    };
                    Dictionary<string, object> levelData = new Dictionary<string, object>
                    {
                        { "GridData", gridData },
                        { "Level", 15 },
                        { "Targets", new List<int> { 0, 1, 2 } },
                        { "TargetCounts", new List<int> { 64, 64, 64 } },
                        { "MoveLimit", 32 }
                    };
                    UploadLevelData("levels", "15", levelData);
                }
                else
                {
                    Debug.LogError("Could not resolve Firebase dependencies: " + task.Result);
                }
            });
        }

        private void UploadLevelData(string collectionName, string documentId, Dictionary<string, object> data)
        {
            db.Collection(collectionName).Document(documentId).SetAsync(data).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError($"Error uploading data: {task.Exception}");
                }
                else
                {
                    Debug.Log($"Successfully uploaded data to {collectionName}/{documentId}");
                }
            });
        }
    }
}
