using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class StoneManager : MonoBehaviour
{
    public GameObject redDiamonPrefab, blueDiamonPrefab, greenDiamonPrefab, icePrefab;
    public FirestoreReader firestoreReader;
    public int row, column;
    public StoneBehaviour[,] boardStone;
    private LevelData levalData;
    private bool isBoardReady = false;

    public void Init(LevelData data)
    {
        this.levalData = data;
        this.row = data.row;
        this.column = data.column;
        boardStone = new StoneBehaviour[row, column];
    }

    public bool IsCellEmpty(int row, int col)
    {
        return boardStone[row, col] == null;
    }

    public void RegisterStone(StoneBehaviour stone, int row, int col)
    {
        boardStone[row, col] = stone;
    }

    public void UnRegisterStone(int row, int col)
    {
        boardStone[row, col] = null;
    }

    public async Task SpawnStone(int row, int column, List<(int x, int y)> positionBlockList, List<string> ruleList)
    {
        foreach (string rule in ruleList)
        {
            switch (rule)
            {
                case "spawnNormalStone":
                    List<StoneType> stoneList = await firestoreReader.LoadRule();
                    if (stoneList != null)
                    {
                        SpawnStoneForNewGame(row, column, positionBlockList, stoneList);
                    }
                    break;
            }
        }
    }
    
    public GameObject GetStonePrefabByName(StoneType type)
    {
        switch (type)
        {
            case StoneType.Red: return redDiamonPrefab;
            case StoneType.Blue: return blueDiamonPrefab;
            case StoneType.Green: return greenDiamonPrefab;
            default: return null;
        }
    }
    
    public void SpawnStoneForNewGame(int row, int column, List<(int x, int y)> positionBlockList, List<StoneType> stoneList)
    {
        foreach (var pos in positionBlockList)
        {
            Vector2 posIce = new Vector2(pos.x, pos.y);
            GameObject ice = Instantiate(icePrefab, posIce, Quaternion.identity);
            boardStone[pos.y, pos.x] = ice.GetComponent<IceBehaviour>();
        }

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                if (boardStone[i, j] != null) continue;
                List<StoneType> availableStone = new List<StoneType>(stoneList);
                StoneType type = StoneType.Red;
                while (availableStone.Count > 0)
                {
                    int index = Random.Range(0, availableStone.Count);
                    type = availableStone[index];
                    if (!PreventInitialMatch3(i, j, type))
                    {
                        availableStone.RemoveAt(index);
                    }
                    else break;
                }
                GameObject stonePrefab = GetStonePrefabByName(type);
                Vector2 positionStone = new Vector2(j, i);
                if (stonePrefab != null)
                {
                    GameObject stone = Instantiate(stonePrefab, positionStone, Quaternion.identity);
                    boardStone[i, j] = stone.GetComponent<StoneBehaviour>();
                }
            }
        }
        isBoardReady = true;
    }

    public bool PreventInitialMatch3(int i, int j, StoneType type)
    {
        if (i < 2 && j < 2) return true;
        if (j >= 2 && boardStone[i, j - 1].stoneType == type
                   && boardStone[i, j - 2].stoneType == type) return false;
        if (i >= 2 && boardStone[i - 1, j].stoneType == type
                   && boardStone[i - 2, j].stoneType == type) return false;
        return true;
    }

    private void Update()
    {
        if (!isBoardReady || StoneBehaviour.isSwap) return;
        FindMatch3();
    }

    public void FindMatch3()
    {
        for(int i=0; i<row; i++)
        {
            for(int j=0; j<column; j++)
            {
                if (boardStone[i, j] != null && boardStone[i, j].stoneType != StoneType.Ice) CheckMatch3(i, j);
            }
        }
    }

    public void CheckMatch3(int r, int c)
    {
        if (r + 2 <= row - 1 && boardStone[r + 1, c]!=null && boardStone[r + 2, c]!=null &&
            boardStone[r, c].stoneType == boardStone[r + 1, c].stoneType &&
            boardStone[r + 1, c].stoneType == boardStone[r + 2, c].stoneType)
        {
            DeleteMatch3(r, c, "up");
            return;
        }

        else if (c + 2 <= column - 1 && boardStone[r, c + 1] != null && boardStone[r, c + 2] != null &&
            boardStone[r, c].stoneType == boardStone[r, c+1].stoneType &&
            boardStone[r, c+1].stoneType == boardStone[r, c + 2].stoneType)
        {
            DeleteMatch3(r, c, "right");
            return;
        }

        else if (r - 2 >= 0 && boardStone[r - 1, c] != null && boardStone[r - 2, c] != null &&
            boardStone[r, c].stoneType == boardStone[r - 1, c].stoneType &&
            boardStone[r - 1, c].stoneType == boardStone[r - 2, c].stoneType)
        {
            DeleteMatch3(r, c, "down");
            return;
        }

        else if (c - 2 >= 0 && boardStone[r, c - 1] != null && boardStone[r, c - 2] != null &&
            boardStone[r, c].stoneType == boardStone[r, c - 1].stoneType &&
            boardStone[r, c - 1].stoneType == boardStone[r, c - 2].stoneType)
        {
            DeleteMatch3(r, c, "left");
            return;
        }

        return;
    }

    private void DeleteMatch3(int r, int c, string direction)
    {
        int []d = {0, 0, 0, 0, 0, 0 };
        switch(direction)
        {
            case "up":
                {
                    d[0] = 0; d[1] = 1; d[2] = 2;
                    break;
                }
            case "down":
                {
                    d[0] = 0; d[1] = -1; d[2] = -2;
                    break;
                }
            case "left":
                {
                    d[3] = 0; d[4] = -1; d[5] = -2;
                    break;
                }
            case "right":
                {
                    d[3] = 0; d[4] = 1; d[5] = 2;
                    break;
                }
        }
        for(int i=0; i<3; i++)
        {
            Destroy(boardStone[r + d[i], c + d[i + 3]].gameObject);
            boardStone[r + d[i], c + d[i + 3]] = null;
        }
    }
}

