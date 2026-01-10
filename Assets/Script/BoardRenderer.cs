using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BoardRenderer : MonoBehaviour
{
    public GameObject boardTilePrefab;

    public FirestoreReader firestoreReader;
    public StoneManager stoneManager;
    public Transform stoneContainer;

    public void RenderBoard(LevelData levelData)
    {
        stoneManager.SpawnStone(levelData.row, levelData.column, levelData.positionBlockList, levelData.ruleList);
    }

    public void ClearBoard()
    {
        foreach (Transform stone in stoneContainer)
        {
            Destroy(stone.gameObject);
        }
    }
}