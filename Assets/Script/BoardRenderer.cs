using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BoardRenderer : MonoBehaviour
{
    public GameObject tilePerfab;
    public GameObject wallPerfab, floorPerfab;
    public GameObject icePrefab;
    private float distanceTile = 1.05f;

    public FirestoreReader firestoreReader;
    public StoneManager stoneManager;

    public void RenderBoard(LevelData levelData)
    {
        DrawBoard(levelData.row, levelData.column, levelData.positionBlockList);
        stoneManager.SpawnStone(levelData.row, levelData.column, levelData.positionBlockList, levelData.ruleList);
    }
    private void DrawBoard(int row, int column, List<(int x, int y)> positionBlock)
    {
        for (int i = 0; i < column; i++)
        {
            for (int j = 0; j < row; j++)
            {
                Vector2 position = new Vector2(i * distanceTile, j * distanceTile);
                GameObject tile = Instantiate(tilePerfab, position, Quaternion.identity);
                tile.transform.SetParent(this.transform);

                if (j == 0)
                {
                    Vector2 positionFloor = new Vector2(i * distanceTile, -distanceTile / 2);
                    GameObject floor = Instantiate(floorPerfab, positionFloor, Quaternion.identity);
                    floor.transform.SetParent(this.transform);
                }

                if (i == 0)
                {
                    Vector2 positionWallLeft = new Vector2(-distanceTile / 2, j * distanceTile);
                    GameObject wallLeft = Instantiate(wallPerfab, positionWallLeft, Quaternion.identity);
                    wallLeft.transform.SetParent(this.transform);
                }

                if (i == column - 1)
                {
                    Vector2 positionWallRight = new Vector2(i * distanceTile + distanceTile / 2, j * distanceTile);
                    GameObject wallRight = Instantiate(wallPerfab, positionWallRight, Quaternion.identity);
                    wallRight.transform.SetParent(this.transform);
                }
            }
        }
        foreach (var a in positionBlock)
        {
            Vector2 pos = new Vector2(a.x * distanceTile, a.y * distanceTile);
            Instantiate(icePrefab, pos, Quaternion.identity);
        }
    }
}
