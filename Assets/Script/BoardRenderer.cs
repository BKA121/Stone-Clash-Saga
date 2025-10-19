using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BoardRenderer : MonoBehaviour
{
    public GameObject tilePerfab;
    public GameObject floorPerfab;

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
                GameObject tile = Instantiate(tilePerfab, position, Quaternion.identity);
                tile.transform.SetParent(this.transform);

                if (j == 0)
                {
                    Vector2 positionFloor = new Vector2(i, -0.5f);
                    GameObject floor = Instantiate(floorPerfab, positionFloor, Quaternion.identity);
                    floor.transform.SetParent(this.transform);
                }
            }
        }
    }
}
