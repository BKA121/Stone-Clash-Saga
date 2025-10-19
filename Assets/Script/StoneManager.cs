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
                    if (!CheckStone(i, j, type))
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
    }
    public bool CheckStone(int i, int j, StoneType type)
    {
        if (i < 2 && j < 2) return true;
        if (j >= 2 && boardStone[i, j - 1].stoneType == type
                   && boardStone[i, j - 2].stoneType == type) return false;
        if (i >= 2 && boardStone[i - 1, j].stoneType == type
                   && boardStone[i - 2, j].stoneType == type) return false;
        return true;
    }
}

