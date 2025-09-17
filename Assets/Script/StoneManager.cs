using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class StoneManager : MonoBehaviour
{
    public GameObject redDiamonPrefab, blueDiamonPrefab, greenDiamonPrefab;
    [SerializeField] private LayerMask stoneLayer;
    [SerializeField] private LayerMask blockLayer;
    private float distanceTile = 1.05f;

    public FirestoreReader firestoreReader;

    public async Task SpawnStone(int row, int column, List<(int x, int y)> positionBlockList, List<string> ruleList)
    {
        foreach (string rule in ruleList)
        {
            switch (rule)
            {
                case "spawnNormalStone":
                    List<string> stoneList = await firestoreReader.LoadRule();
                    if (stoneList != null)
                    {
                        StartCoroutine(SpawnUntilTopRowFull(row, column, positionBlockList, stoneList));
                    }
                    break;
            }
        }
    }
    private GameObject GetStonePrefabByName(string nameStone)
    {
        switch (nameStone)
        {
            case "red": return redDiamonPrefab;
            case "blue": return blueDiamonPrefab;
            case "green": return greenDiamonPrefab;
            default: return null;
        }
    }
    private IEnumerator SpawnUntilTopRowFull(int row, int column, List<(int x, int y)> positionBlockList, List<string> stoneList)
    {
        while (!CheckFullBoard(row, column))
        {
            while (true)
            {
                bool anySpawned = false;

                for (int i = 0; i < column; i++)
                {
                    int j = row - 1;
                    Vector2 positionStone = new Vector2(i * distanceTile, j * distanceTile);

                    // Kiem tra o trong
                    if (!Physics2D.OverlapCircle(positionStone, 0.45f, stoneLayer))
                    {
                        string nameStone = stoneList[UnityEngine.Random.Range(0, stoneList.Count)].ToString();
                        GameObject stonePrefab = GetStonePrefabByName(nameStone);

                        if (stonePrefab != null)
                        {
                            Instantiate(stonePrefab, positionStone, Quaternion.identity);
                            anySpawned = true;
                        }
                    }
                }

                if (!anySpawned)
                {
                    break;
                }

                yield return new WaitForSeconds(0.29f);
            }

            if (!CheckFullBoard(row, column))
            {
                foreach (var pos in positionBlockList)
                {
                    SlideStoneLeft(pos.x - 1, pos.y);
                }
                yield return new WaitForSeconds(0.23f);
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
                Collider2D candy = Physics2D.OverlapCircle(pos, 0.45f, stoneLayer);
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
    private void SlideStoneLeft(int x, int y)
    {
        Vector2 stoneLeftPos = new Vector2(x * distanceTile, y * distanceTile);
        Collider2D stoneLeft = Physics2D.OverlapCircle(stoneLeftPos, 0.45f, stoneLayer);

        if (stoneLeft == null) return;

        Vector2 targetPos = new Vector2((x + 1) * distanceTile, (y - 1) * distanceTile);

        if (Physics2D.OverlapCircle(targetPos, 0.4f, stoneLayer) != null) return;

        stoneLeft.transform.position = targetPos;
    }
}
