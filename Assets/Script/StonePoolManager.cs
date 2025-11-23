using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StonePoolManager : MonoBehaviour
{
    // Moi stoneType la mot pool
    private Dictionary<StoneType, StonePool> stonePools = new Dictionary<StoneType, StonePool>();
    private List<StoneType> keyStoneList = new List<StoneType>();
    public StoneManager stoneManager;

    public void Awake()
    {
        stoneManager = FindObjectOfType<StoneManager>();
    }

    public void InitPools(Dictionary<StoneType, GameObject> stonePrefab, int sizePool)
    {
        foreach(var i in stonePrefab)
        {
            stonePools[i.Key] = new StonePool(i.Value, sizePool, this.transform);
            keyStoneList.Add(i.Key);
        }
    }

    public GameObject GetStoneByType(StoneType type, Vector2 position)
    {
        return stonePools[type].GetOutOfPool(position);
    }

    public GameObject GetRandomStone(Vector2 position)
    {
        int col = (int)position.x;
        int row = (int)position.y;
        StoneType type = StoneType.Red;

        // Kiem tra de khong spwan match trong mot cot
        List<StoneType> availableStone = new List<StoneType>(keyStoneList);
        while (availableStone.Count > 0)
        {
            int index = Random.Range(0, availableStone.Count);
            type = availableStone[index];
            if (row < stoneManager.row - 1 && stoneManager.boardStone[row+1, col].stoneType == type)
            {
                availableStone.RemoveAt(index);
            }
            else break;
        }
        return stonePools[type].GetOutOfPool(position);
    }

    public void ReturnStoneByType(StoneType type, GameObject stone)
    {
        stonePools[type].ReturnPool(stone);
    }
}
