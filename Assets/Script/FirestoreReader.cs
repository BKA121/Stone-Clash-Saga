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
            levelData.moves = Convert.ToInt32(data["moves"]);

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

            if (data.ContainsKey("target") && data["target"] is Dictionary<string, object> targetData)
            {
                foreach (var entry in targetData)
                {
                    levelData.targetDict.Add(entry.Key, Convert.ToInt32(entry.Value));
                }
            }
        }
        return levelData;
    }
    public async Task<List<StoneType>> LoadRuleSpawn_x_Type(string typeRule)
    {
        var docRef = db.Collection("rules").Document(typeRule);
        DocumentSnapshot s = await docRef.GetSnapshotAsync();
        if(s.Exists && s.ContainsField("spawnType"))
        {
            Dictionary<string, object> ruleData = s.ToDictionary();
            var stoneList = ruleData["spawnType"] as List<object>;
            List<StoneType> spawnStoneList = new List<StoneType>();
            foreach(object a in stoneList)
            {
                string nameStone = a.ToString();
                StoneType type = Enum.Parse<StoneType>(nameStone, true);
                spawnStoneList.Add(type);
            }
            return spawnStoneList;
        }
        return null;
    }
}
