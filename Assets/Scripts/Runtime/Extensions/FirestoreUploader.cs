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
                            { "cells", new List<int> { -1, -1, -1, -1 } }
                        },
                        new()
                        {
                            { "rowIndex", 1 },
                            { "cells", new List<int> { 0, 2, 3, 1 } }
                        },
                        new()
                        {
                            { "rowIndex", 2 },
                            { "cells", new List<int> { 2, 1, 0, 0 } }
                        },
                        new()
                        {
                            { "rowIndex", 3 },
                            { "cells", new List<int> { 3, 3, 1, 2 } }
                        },
                    };
                    
                    Dictionary<string, object> gridData = new()
                    {
                        { "GridRowSize", 4 },
                        { "GridColumnSize", 4 },
                        { "ColorCount", 4 },
                        { "Grid", gridRows } 
                    };
                    Dictionary<string, object> levelData = new Dictionary<string, object>
                    {
                        { "GridData", gridData },
                        { "Level", 1 },
                        { "Targets", new List<int> {  0,  1 } },
                        { "TargetCounts", new List<int> { 80, 122 } },
                        { "MoveLimit", 40 }
                    };
                    /*
                    var gridRows = new List<Dictionary<string, object>>
                    {
                        new()
                        {
                            { "rowIndex", 0 },
                            { "cells", new List<int> { -1, -1, -1, -1, -1, -1 } }
                        },
                        new()
                        {
                            { "rowIndex", 1 },
                            { "cells", new List<int> { -1, -1, -1, -1, -1, -1 } }
                        },
                        new()
                        {
                            { "rowIndex", 2 },
                            { "cells", new List<int> {  0,  1,  1,  1,  2,  1 } }
                        },
                        new()
                        {
                            { "rowIndex", 3 },
                            { "cells", new List<int> {  2,  2,  3,  2,  1,  0 } }
                        },
                        new()
                        {
                            { "rowIndex", 4 },
                            { "cells", new List<int> {  0,  1,  1,  0,  1,  0 } }
                        },
                        new()
                        {
                            { "rowIndex", 5 },
                            { "cells", new List<int> {  2,  2,  1,  2,  1,  0 } }
                        },
                        new()
                        {
                            { "rowIndex", 6 },
                            { "cells", new List<int> {  3,  2,  3,  3,  0,  1 } }
                        },
                        new()
                        {
                            { "rowIndex", 7 },
                            { "cells", new List<int> {  2,  3,  0,  3,  0,  1 } }
                        }
                    };

                    Dictionary<string, object> gridData = new()
                    {
                        { "GridRowSize", 8 },
                        { "GridColumnSize", 6 },
                        { "ColorCount", 4 },
                        { "Grid", gridRows } 
                    };

                    Dictionary<string, object> levelData = new Dictionary<string, object>
                    {
                        { "GridData", gridData },
                        { "Level", 1 },
                        { "Targets", new List<int> {  0,  1} },
                        { "TargetCounts", new List<int> {8, 12} },
                        { "MoveLimit", 40 }
                    };
                    */

                    UploadLevelData("levels", "1", levelData);
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
