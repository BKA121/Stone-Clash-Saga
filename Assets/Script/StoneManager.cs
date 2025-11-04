using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class StoneManager : MonoBehaviour
{
    public GameObject redDiamonPrefab, blueDiamonPrefab, greenDiamonPrefab, icePrefab;
    public FirestoreReader firestoreReader;
    public int row, column;
    private int countMatch = 0;
    public StoneBehaviour[,] boardStone;
    private LevelData levalData;
    private bool isBoardReady = false;
    public static bool startFind = false;
    public int countStoneFall = 0;
    private string directionDeleteMatch = "";

    public void Init(LevelData data)
    {
        this.levalData = data;
        this.row = data.row;
        this.column = data.column;
        boardStone = new StoneBehaviour[row, column];
    }

    // Dang ky vao board
    public void RegisterStone(StoneBehaviour stone, int row, int col)
    {
        boardStone[row, col] = stone;
    }

    // Thoat dang ky de roi
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
            RegisterStone(ice.GetComponent<StoneBehaviour>(), pos.y, pos.x);
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
                    RegisterStone(stone.GetComponent<StoneBehaviour>(), i, j);
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
        if (!isBoardReady || countStoneFall > 0 || StoneBehaviour.isSwapping) return;

        // Chi cho tim match khi vua swap hoac dang con combo
        if (startFind || countMatch > 0)
        {
            countMatch = 0;
            startFind = false;
            FindMatch3();
            FallStone();
        }
    }

    public void FindMatch3()
    {
        for(int i=0; i<row; i++)
        {
            for(int j=0; j<column; j++)
            {
                if (boardStone[i, j] != null && boardStone[i, j].stoneType != StoneType.Ice)
                {
                    if (CheckMatch3(i, j)) DeleteMatch3(i, j, directionDeleteMatch);
                }
            }
        }
    }

    public void FallStone()
    {
        for (int j = 0; j < column; j++)
        {
            int countNullCell = 0;
            for (int i = 0; i < row; i++)
            {
                if (boardStone[i, j] == null) countNullCell += 1;
                else if (boardStone[i, j].stoneType == StoneType.Ice) countNullCell = 0;
                else if (countNullCell != 0 && boardStone[i, j].gameObject != null)
                {
                    StartCoroutine(boardStone[i, j].Fall(i, j, countNullCell));
                }
            }
        }
    }

    // Ham nay kiem tra 4 huong cua stone tai hang r, cot c
    public bool CheckMatch3(int r, int c)
    {
        if (r + 2 <= row - 1 && boardStone[r + 1, c]!=null && boardStone[r + 2, c]!=null &&
            boardStone[r, c].stoneType == boardStone[r + 1, c].stoneType &&
            boardStone[r + 1, c].stoneType == boardStone[r + 2, c].stoneType)
        {
            directionDeleteMatch = "up";
            return true;
        }

        else if (c + 2 <= column - 1 && boardStone[r, c + 1] != null && boardStone[r, c + 2] != null &&
            boardStone[r, c].stoneType == boardStone[r, c+1].stoneType &&
            boardStone[r, c+1].stoneType == boardStone[r, c + 2].stoneType)
        {
            directionDeleteMatch = "right";
            return true;
        }

        else if (r - 2 >= 0 && boardStone[r - 1, c] != null && boardStone[r - 2, c] != null &&
            boardStone[r, c].stoneType == boardStone[r - 1, c].stoneType &&
            boardStone[r - 1, c].stoneType == boardStone[r - 2, c].stoneType)
        {
            directionDeleteMatch = "down";
            return true;
        }

        else if (c - 2 >= 0 && boardStone[r, c - 1] != null && boardStone[r, c - 2] != null &&
            boardStone[r, c].stoneType == boardStone[r, c - 1].stoneType &&
            boardStone[r, c - 1].stoneType == boardStone[r, c - 2].stoneType)
        {
            directionDeleteMatch = "left";
            return true;
        }
        return false;
    }

    // Ham nay giup kiem tra match3 khi stone o trung tam chi dung de kiem tra sau swap
    public bool CheckMatch3IfStoneCenter(int r, int c)
    {
        if (r - 1 >= 0 && r + 1 <= row - 1 && boardStone[r + 1, c] != null && boardStone[r - 1, c] != null &&
            boardStone[r, c].stoneType == boardStone[r + 1, c].stoneType &&
            boardStone[r, c].stoneType == boardStone[r - 1, c].stoneType) return true;

        if (c - 1 >= 0 && c + 1 <= column - 1 && boardStone[r, c - 1] != null && boardStone[r, c + 1] != null &&
            boardStone[r, c].stoneType == boardStone[r, c + 1].stoneType &&
            boardStone[r, c].stoneType == boardStone[r, c - 1].stoneType) return true;

        return false;
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
        for (int i=0; i<3; i++)
        {
            Destroy(boardStone[r + d[i], c + d[i + 3]].gameObject);
            boardStone[r + d[i], c + d[i + 3]] = null;
        }

        countMatch += 1;

    }
}

