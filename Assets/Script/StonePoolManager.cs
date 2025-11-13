using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StonePoolManager : MonoBehaviour
{
    // Moi stoneType la mot pool
    private Dictionary<StoneType, StonePool> stonePools = new Dictionary<StoneType, StonePool>();

    public void InitPools(Dictionary<StoneType, GameObject> stonePrefab, int sizePool)
    {
        foreach(var i in stonePrefab)
        {
            stonePools[i.Key] = new StonePool(i.Value, sizePool, this.transform);
        }
    }

    public GameObject GetStoneByType(StoneType type, Vector2 position)
    {
        return stonePools[type].GetOutOfPool(position);
    }

    public void ReturnStoneByType(StoneType type, GameObject stone)
    {
        stonePools[type].ReturnPool(stone);
    }
}
