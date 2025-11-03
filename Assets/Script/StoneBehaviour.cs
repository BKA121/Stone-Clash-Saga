using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public enum StoneType
{
    Red, Green, Blue, Ice
}

public class StoneBehaviour : MonoBehaviour
{
    public StoneType stoneType;
    public StoneManager stoneManager;

    public static bool isSwapping = false;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private float swipeAngle = 0;

    public void Start()
    {
        stoneManager = FindObjectOfType<StoneManager>();
    }

    public IEnumerator Fall(int row, int col, int distanceFall)
    {
        StoneManager.isBoardDeleteStone = true;
        // Go dky khoi bang
        stoneManager.UnRegisterStone(row, col);

        Vector3 targetPos = new Vector3(col, row - distanceFall, 0);

        float speedFall = 0f, acceleration = 0.25f;
        while(Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            speedFall += acceleration;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * speedFall);
            yield return null;
        }
        transform.position = targetPos;

        // Dky vao vi tri moi sau khi roi
        stoneManager.RegisterStone(this, row - distanceFall, col);
        yield return new WaitForSeconds(0.4f);
        StoneManager.isBoardDeleteStone = false;
    }

    public void OnMouseDown()
    {
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void OnMouseUp()
    {
        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        swipeAngle = CaculateAngle();
        SwapStone(swipeAngle);
    }

    public float CaculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
        return swipeAngle;
    }

    public bool CheckStoneBeforeSwap(int row, int col)
    {
        if (row < 0 || row >= stoneManager.row || col < 0 || col >= stoneManager.column) return false;
        if (stoneManager.boardStone[row, col] == null) return false;
        if (stoneManager.boardStone[row, col].stoneType == StoneType.Ice) return false;
        return true;
    }

    public bool CheckStoneAfterSwap(int row, int col, int newRow, int newCol)
    {
        if (stoneManager.CheckMatch3(row, col) || stoneManager.CheckMatch3(newRow, newCol)) return true;
        if (stoneManager.CheckMatch3IfStoneCenter(row, col) || stoneManager.CheckMatch3IfStoneCenter(newRow, newCol)) return true;
        return false;
    }

    public void SwapStone(float angle)
    {
        int row = Mathf.RoundToInt(firstTouchPosition.y), col = Mathf.RoundToInt(firstTouchPosition.x);

        if (!CheckStoneBeforeSwap(row, col)) return;

        int dx = 0, dy = 0;
        if (-45 <= angle && angle <= 45) dx = 1;
        else if (45 < angle && angle < 135) dy = 1;
        else if (-135 < angle && angle < -45) dy = -1;
        else dx = -1;

        int newRow = row + dy, newCol = col + dx;
        if (!CheckStoneBeforeSwap(newRow, newCol)) return;

        StoneBehaviour stoneA = stoneManager.boardStone[row, col];
        StoneBehaviour stoneB = stoneManager.boardStone[newRow, newCol];

        // Doi cho
        StartCoroutine(SmoothSwap(stoneA, stoneB, row, col, newRow, newCol));
    }

    private IEnumerator SmoothSwap(StoneBehaviour stoneA, StoneBehaviour stoneB, int row, int col, int newRow, int newCol)
    {
        isSwapping = true;

        // Doi cho trong mang
        stoneManager.boardStone[row, col] = stoneB;
        stoneManager.boardStone[newRow, newCol] = stoneA;

        Vector3 posA = stoneA.transform.position;
        Vector3 posB = stoneB.transform.position;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            stoneA.transform.position = Vector3.Lerp(posA, posB, t);
            stoneB.transform.position = Vector3.Lerp(posB, posA, t);
            yield return null;
        }

        stoneA.transform.position = posB;
        stoneB.transform.position = posA;

        if (!CheckStoneAfterSwap(row, col, newRow, newCol))
        {
            yield return new WaitForSeconds(0.05f);
            // Doi cho trong mang
            stoneManager.boardStone[row, col] = stoneA;
            stoneManager.boardStone[newRow, newCol] = stoneB;

            duration = 0.2f;
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                stoneA.transform.position = Vector3.Lerp(posB, posA, t);
                stoneB.transform.position = Vector3.Lerp(posA, posB, t);
                yield return null;
            }

            stoneA.transform.position = posA;
            stoneB.transform.position = posB;
        }

        yield return new WaitForSeconds(0.1f);

        isSwapping = false;
        StoneManager.startFind = true;
    }
}

