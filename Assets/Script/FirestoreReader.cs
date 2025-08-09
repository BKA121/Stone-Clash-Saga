using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class FirestoreReader : MonoBehaviour
{
    private FirebaseFirestore db;
    private void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;
    }
    public async Task<LevelData> LoadLevelData(string idLevel)
    {
        var docRef = db.Collection("levels").Document(idLevel);
        DocumentSnapshot s = await docRef.GetSnapshotAsync();
        LevelData levelData = new LevelData();
        if (s.Exists)
        {
            Dictionary<string, object> data = s.ToDictionary();

            levelData.row = Convert.ToInt32(data["row"]);
            levelData.column = Convert.ToInt32(data["column"]);

            var posBlockList = data["position_block"] as List<object>;
            foreach (object pos in posBlockList)
            {
                var position = pos as Dictionary<string, object>;
                int x = Convert.ToInt32(position["x"]);
                int y = Convert.ToInt32(position["y"]);
                levelData.positionBlockList.Add((x, y));
            }

            var ruleList = data["rules"] as List<object>;
            foreach (object ruleObj in ruleList)
            {
                string rule = ruleObj.ToString();
                levelData.ruleList.Add(rule);
            }
        }
        return levelData;
    }
    public async Task<List<string>> LoadRule()
    {
        var docRef = db.Collection("rules").Document("spawnNormalCandy");
        DocumentSnapshot s = await docRef.GetSnapshotAsync();
        if(s.Exists && s.ContainsField("spawnCandy"))
        {
            Dictionary<string, object> ruleData = s.ToDictionary();
            var candyList = ruleData["spawnCandy"] as List<object>;
            List<string> spawnCandyList = new List<string>();
            foreach(object a in candyList)
            {
                string nameCandy = a.ToString();
                spawnCandyList.Add(nameCandy);
            }
            return spawnCandyList;
        }
        return null;
    }
}
