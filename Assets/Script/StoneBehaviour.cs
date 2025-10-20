using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum StoneType
{
    Red, Blue, Green, Ice
}
public class StoneBehaviour : MonoBehaviour
{
    public StoneType stoneType;
    private Rigidbody2D rigidbody2D;
    public StoneManager stoneManager;
    private bool isSettled = false;
    private bool isSliding = false;
    private float spawnTime;

    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private float swipeAngle = 0;
    public static bool isSwap = false;

    public virtual void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        stoneManager = FindObjectOfType<StoneManager>();
        spawnTime = Time.time;
    }

    public virtual void Update()
    {
        if (Time.time - spawnTime < 0.06f) return;
        if (isSettled && Mathf.Abs(rigidbody2D.velocity.y) < 0.05f) return;
        if (isSettled && Mathf.Abs(rigidbody2D.velocity.y) > 0.5f) UnSettle();
        if (isSliding) return;
        if (Mathf.Abs(rigidbody2D.velocity.y) < 0.05f) CheckSlideOrSettle();
    }

    public virtual void CheckSlideOrSettle()
    {
        int row = Mathf.RoundToInt(transform.position.y);
        int col = Mathf.RoundToInt(transform.position.x);

        ////Kiem tra truot trai
        //if (col > 0 && row > 0 && stoneManager.IsCellEmpty(row - 1, col - 1))
        //{
        //    StartCoroutine(SlideToTarget(col - 1, row - 1));
        //    return;
        //}

        ////Kiem tra truot phai
        //if (col < stoneManager.column - 1 && row > 0 && stoneManager.IsCellEmpty(row - 1, col + 1))
        //{
        //    StartCoroutine(SlideToTarget(col + 1, row - 1));
        //    return;
        //}
        Settle(row, col);
    }

    public void Settle(int row, int col)
    {
        isSettled = true;
        stoneManager.RegisterStone(this, row, col);
    }

    public void UnSettle()
    {
        isSettled = false;
        int row = Mathf.RoundToInt(transform.position.y);
        int col = Mathf.RoundToInt(transform.position.x);
        stoneManager.UnRegisterStone(row, col);
    }

    private IEnumerator SlideToTarget(int x, int y)
    {
        isSliding = true;
        Vector2 targetPos = new Vector2(x, y);
        while (Vector2.Distance(transform.position, targetPos) > 0.05f)
        {
            Vector2 newPos = Vector2.MoveTowards(transform.position, targetPos, Time.deltaTime * 10f);
            rigidbody2D.MovePosition(newPos);
            yield return new WaitForFixedUpdate();
        }
        rigidbody2D.MovePosition(targetPos);
        isSliding = false;
        spawnTime = Time.time;
        yield return null;
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

    public void SwapStone(float angle)
    {
        isSwap = true;
        int row = Mathf.RoundToInt(firstTouchPosition.y), col = Mathf.RoundToInt(firstTouchPosition.x);
        if (stoneManager.boardStone[row, col]==null) return;
        if (stoneManager.boardStone[row, col].stoneType == StoneType.Ice) return;

        int dx = 0, dy = 0;
        if (-45 <= angle && angle <= 45) dx = 1;
        else if (45 < angle && angle < 135) dy = 1;
        else if (-135 < angle && angle < -45) dy = -1;
        else dx = -1;

        int newRow = row + dy, newCol = col + dx;
        if (newRow < 0 || newRow >= stoneManager.row || newCol < 0 || newCol >= stoneManager.column) return;
        if (stoneManager.boardStone[newRow, newCol] == null) return;
        if (stoneManager.boardStone[newRow, newCol].stoneType == StoneType.Ice) return;
        StoneBehaviour stoneA = stoneManager.boardStone[row, col];
        StoneBehaviour stoneB = stoneManager.boardStone[newRow, newCol];

        // Doi cho trong mang
        stoneManager.boardStone[row, col] = stoneB;
        stoneManager.boardStone[newRow, newCol] = stoneA;

        // Tat trong luc cac vien phia tren
        ChangeKinematic(row, col, newRow, newCol, true);

        // Doi cho hien thi
        StartCoroutine(SmoothSwap(stoneA, stoneB, row, col, newRow, newCol));
    }

    private IEnumerator SmoothSwap(StoneBehaviour stoneA, StoneBehaviour stoneB, int row, int col, int newRow, int newCol)
    {
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

        yield return new WaitForSeconds(0.15f);

        // Bat lai trong luc
        ChangeKinematic(row, col, newRow, newCol, false);
        isSwap = false;
    }

    public void ChangeKinematic(int row, int col, int newRow, int newCol, bool status)
    {
        if (row != stoneManager.row - 1)
        {
            if (row == newRow)
            {
                if (stoneManager.boardStone[row + 1, col] != null &&
                    stoneManager.boardStone[row + 1, col].rigidbody2D != null)
                    stoneManager.boardStone[row + 1, col].rigidbody2D.isKinematic = status;

                if (stoneManager.boardStone[newRow + 1, newCol] != null &&
                    stoneManager.boardStone[newRow + 1, newCol].rigidbody2D != null)
                    stoneManager.boardStone[newRow + 1, newCol].rigidbody2D.isKinematic = status;
            }
            else
            {
                if (row > newRow && stoneManager.boardStone[row + 1, col] != null &&
                    stoneManager.boardStone[row + 1, col].rigidbody2D != null) 
                    stoneManager.boardStone[row + 1, col].rigidbody2D.isKinematic = status;
                else
                {
                    if (row + 2 <= stoneManager.row - 1 && stoneManager.boardStone[row + 2, col]!=null &&
                        stoneManager.boardStone[row + 2, col].rigidbody2D != null)
                        stoneManager.boardStone[row + 2, col].rigidbody2D.isKinematic = status;
                }
            }
        }
    }
}

