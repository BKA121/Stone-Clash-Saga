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
    public Transform stoneContainer;

    public void Awake()
    {
        stoneManager = FindObjectOfType<StoneManager>();
    }

    public void InitPools(Dictionary<StoneType, GameObject> stonePrefab, int sizePool)
    {
        foreach(var i in stonePrefab)
        {
            stonePools[i.Key] = new StonePool(i.Value, sizePool, stoneContainer);
            keyStoneList.Add(i.Key);
        }
    }

    public GameObject GetStoneByType(StoneType type, Vector2 position, int c, int r)
    {
        return stonePools[type].GetOutOfPool(position, c, r);
    }

    public GameObject GetRandomStone(int c, int r)
    {
        StoneType type = StoneType.Red;

        // Kiem tra de khong spawn match trong mot cot
        List<StoneType> availableStone = new List<StoneType>(keyStoneList);
        while (availableStone.Count > 0)
        {
            int index = Random.Range(0, availableStone.Count);
            type = availableStone[index];
            if (r < stoneManager.row - 1 && stoneManager.boardStone[r+1, c].stoneType == type)
            {
                availableStone.RemoveAt(index);
            }
            else break;
        }
        Vector2 position = stoneManager.UpdatePositionStone(c, r);
        return stonePools[type].GetOutOfPool(position, c, r);
    }

    public void ReturnStoneByType(StoneType type, GameObject stone)
    {
        stonePools[type].ReturnPool(stone);
    }
}
