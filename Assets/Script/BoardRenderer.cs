using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BoardRenderer : MonoBehaviour
{
    public GameObject tilePerfab;
    public GameObject wallPerfab, floorPerfab;
    public GameObject candyRedPrefab, candyBluePrefab, candyGreenPrefab, blockPrefab;
    private float distanceTile = 1.05f;
    [SerializeField] private LayerMask candyLayer;
    [SerializeField] private LayerMask blockLayer;
    public FirestoreReader firestoreReader;

    public void RenderBoard(LevelData levelData)
    {
        DrawBoard(levelData.row, levelData.column, levelData.positionBlockList);
        DrawCandy(levelData.row, levelData.column, levelData.positionBlockList, levelData.ruleList);
    }

    private async Task DrawCandy(int row, int column, List<(int x, int y)> positionBlockList, List<string> ruleList)
    {
        foreach(string rule in ruleList)
        {
            switch (rule)
            {
                case "SpawnNormalCandy":
                    List<string> candyList = await firestoreReader.LoadRule();
                    if(candyList != null)
                    {
                        StartCoroutine(SpawnUntilTopRowFull(row, column, positionBlockList, candyList));
                    }
                    break;
            }
        }
    }
    private IEnumerator SpawnUntilTopRowFull(int row, int column, List<(int x, int y)> positionBlockList, List<string> candyList)
    {
        while (!CheckFullBoard(row, column))
        {
            while (true)
            {
                bool anySpawned = false;

                for (int i = 0; i < column; i++)
                {
                    int j = row - 1;
                    Vector2 positionCandy = new Vector2(i * distanceTile, j * distanceTile);

                    // Kiem tra o trong
                    if (!Physics2D.OverlapCircle(positionCandy, 0.45f, candyLayer))
                    {
                        string candyName = candyList[UnityEngine.Random.Range(0, candyList.Count)].ToString();
                        GameObject candyPrefab = GetCandyPrefabByName(candyName);

                        if (candyPrefab != null)
                        {
                            Instantiate(candyPrefab, positionCandy, Quaternion.identity);
                            anySpawned = true;
                        }
                    }
                }

                if (!anySpawned)
                {
                    break;
                }

                yield return new WaitForSeconds(0.3f);
            }

            if (!CheckFullBoard(row, column))
            {
                foreach (var pos in positionBlockList)
                {
                    SlideCandyLeft(pos.x - 1, pos.y);
                }
                yield return new WaitForSeconds(0.25f);
            }
        }
    }
    private bool CheckFullBoard(int row, int column)
    {
        bool check = true;
        for (int i = 0; i < column; i++)
        {
            for (int j = 0; j < row; j++)
            {
                Vector2 pos = new Vector2(i * distanceTile, j * distanceTile);
                Collider2D candy = Physics2D.OverlapCircle(pos, 0.45f, candyLayer);
                Collider2D block = Physics2D.OverlapCircle(pos, 0.45f, blockLayer);
                if (candy == null && block == null)
                {
                    check = false;
                    return check;
                }
            }
        }
        return check;
    }
    private void SlideCandyLeft(int x, int y)
    {
        // Vị trí hiện tại của viên kẹo bên trái block
        Vector2 candyLeftPos = new Vector2(x * distanceTile, y * distanceTile);
        Collider2D candyLeft = Physics2D.OverlapCircle(candyLeftPos, 0.45f, candyLayer);

        if (candyLeft == null) return;

        Vector2 targetPos = new Vector2((x + 1) * distanceTile, (y - 1) * distanceTile);

        // Đảm bảo vị trí đích không có kẹo
        if (Physics2D.OverlapCircle(targetPos, 0.4f, candyLayer) != null) return;

        candyLeft.transform.position = targetPos;
    }
    private GameObject GetCandyPrefabByName(string candyName)
    {
        switch (candyName)
        {
            case "Red": return candyRedPrefab;
            case "Blue": return candyBluePrefab;
            case "Green": return candyGreenPrefab;
            default: return null;
        }
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
            Instantiate(blockPrefab, pos, Quaternion.identity);
        }
    }
}
