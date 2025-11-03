using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BoardRenderer : MonoBehaviour
{
    public GameObject boardTilePrefab;
    public GameObject floorPrefab, wallPrefab;

    public FirestoreReader firestoreReader;
    public StoneManager stoneManager;

    public void RenderBoard(LevelData levelData)
    {
        DrawBoard(levelData.row, levelData.column);
        stoneManager.SpawnStone(levelData.row, levelData.column, levelData.positionBlockList, levelData.ruleList);
    }
    private void DrawBoard(int row, int column)
    {
        for (int i = 0; i < column; i++)
        {
            for (int j = 0; j < row; j++)
            {
                Vector2 position = new Vector2(i, j);
                GameObject tile = Instantiate(boardTilePrefab, position, Quaternion.identity);
                tile.transform.SetParent(this.transform);

                if (j == 0)
                {
                    Vector2 positionFloor = new Vector2(i, -0.538f);
                    GameObject floor = Instantiate(floorPrefab, positionFloor, Quaternion.identity);
                    floor.transform.SetParent(this.transform);
                }

                if (i == 0)
                {
                    Vector2 positionFloor = new Vector2(-0.540f, j);
                    GameObject floor = Instantiate(wallPrefab, positionFloor, Quaternion.identity);
                    floor.transform.SetParent(this.transform);
                }

                if (i == column - 1)
                {
                    Vector2 positionFloor = new Vector2(i + 0.540f, j);
                    GameObject floor = Instantiate(wallPrefab, positionFloor, Quaternion.identity);
                    floor.transform.SetParent(this.transform);
                }
            }
        }
    }
}

//Check
//for (int i = 0; i < row; i++)
//{
//    for (int j = 0; j < column; j++)
//    {
//        if (boardStone[i, j] == null) Debug.Log("Toa do: " + i + ", " + j + ": null");
//        else Debug.Log("Toa do: " + i + ", " + j + ": " + boardStone[i, j].stoneType);
//    }
//}