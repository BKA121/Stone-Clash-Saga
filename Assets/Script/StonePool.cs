using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StonePool
{
    private GameObject stonePrefab;
    private Queue<GameObject> stoneQueue;
    private Transform parentTransform;

    public StonePool(GameObject stonePrefab, int sizePool, Transform poolManager)
    {
        this.stonePrefab = stonePrefab;
        stoneQueue = new Queue<GameObject>();
        this.parentTransform = poolManager;

        for(int i=1; i<=sizePool; i++)
        {
            GameObject obj = GameObject.Instantiate(stonePrefab, parentTransform);
            obj.SetActive(false);
            stoneQueue.Enqueue(obj);
        }
    }

    public GameObject GetOutOfPool(Vector2 position)
    {
        GameObject obj = stoneQueue.Dequeue();
        obj.SetActive(true);
        obj.transform.position = position;
        return obj;
    }

    public void ReturnPool(GameObject obj)
    {
        obj.SetActive(false);
        stoneQueue.Enqueue(obj);
    }
}
