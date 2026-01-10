using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public enum StoneType
{
    Red, Green, Blue, Purple, Yellow, Ice,
    BlueMatch4
}

public class StoneBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public StoneType stoneType;
    public StoneManager stoneManager;
    public TargetUIHandler uIHandler;
    public int c; 
    public int r;
    public static bool isSwapping = false;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;

    public void Awake()
    {
        stoneManager = FindObjectOfType<StoneManager>();
    }

    public IEnumerator FallAndSlide(List<(int row, int col)> movePath)
    {
        stoneManager.countStoneFallOrSlide += 1;
        float speed = 700f;
        RectTransform rectTransform = GetComponent<RectTransform>();

        foreach (var pos in movePath)
        {
            Vector2 targetPos = stoneManager.UpdatePositionStone(pos.col, pos.row);
            while (Vector2.Distance(rectTransform.anchoredPosition, targetPos) > 1f)
            {
                rectTransform.anchoredPosition = Vector2.MoveTowards(
                    rectTransform.anchoredPosition,
                    targetPos,
                    Time.deltaTime * speed
                );
                yield return null;
            }
            rectTransform.anchoredPosition = targetPos;
            this.r = pos.row;
            this.c = pos.col;
        }

        yield return new WaitForSeconds(0.1f);
        stoneManager.countStoneFallOrSlide -= 1;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        firstTouchPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        finalTouchPosition = eventData.position;
        if (Vector2.Distance(finalTouchPosition, firstTouchPosition) < 30f) return;

        float swipeAngle = CalculateAngle(finalTouchPosition, firstTouchPosition);
        SwapStone(swipeAngle);
    }

    float CalculateAngle(Vector2 final, Vector2 start)
    {
        Vector2 direction = final - start;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    public bool CheckStoneBeforeSwap(int row, int col)
    {
        if (row < 0 || row >= stoneManager.row || col < 0 || col >= stoneManager.column) return false;
        if (stoneManager.boardStone[row, col] == null) return false;
        if (stoneManager.boardStone[row, col].stoneType == StoneType.Ice) return false;
        return true;
    }

    //public bool CheckStoneAfterSwap(int row, int col, int newRow, int newCol)
    //{
    //    if (stoneManager.CheckMatch3(row, col) || stoneManager.CheckMatch3(newRow, newCol)) return true;
    //    if (stoneManager.CheckMatch3IfStoneCenter(row, col) || stoneManager.CheckMatch3IfStoneCenter(newRow, newCol)) return true;
    //    return false;
    //}

    public void SwapStone(float angle)
    {
        if (!CheckStoneBeforeSwap(r, c)) return;

        int dx = 0, dy = 0;
        if (-45 <= angle && angle <= 45) dx = 1;
        else if (45 < angle && angle < 135) dy = 1;
        else if (-135 < angle && angle < -45) dy = -1;
        else dx = -1;

        int newRow = r + dy, newCol = c + dx;
        if (!CheckStoneBeforeSwap(newRow, newCol)) return;

        StoneBehaviour stoneA = stoneManager.boardStone[r, c];
        StoneBehaviour stoneB = stoneManager.boardStone[newRow, newCol];

        // Doi cho
        StartCoroutine(SmoothSwap(stoneA, stoneB, r, c, newRow, newCol));
    }

    private IEnumerator SmoothSwap(StoneBehaviour stoneA, StoneBehaviour stoneB, int rA, int cA, int rB, int cB)
    {
        isSwapping = true;
        RectTransform rectA = stoneA.GetComponent<RectTransform>();
        RectTransform rectB = stoneB.GetComponent<RectTransform>();

        Vector2 startPosA = rectA.anchoredPosition;
        Vector2 startPosB = rectB.anchoredPosition;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curve = t * t * (3f - 2f * t);
            rectA.anchoredPosition = Vector2.Lerp(startPosA, startPosB, curve);
            rectB.anchoredPosition = Vector2.Lerp(startPosB, startPosA, curve);
            yield return null;
        }

        rectA.anchoredPosition = startPosB;
        rectB.anchoredPosition = startPosA;

        stoneA.r = rB; stoneA.c = cB;
        stoneB.r = rA; stoneB.c = cA;

        stoneManager.boardStone[rB, cB] = stoneA;
        stoneManager.boardStone[rA, cA] = stoneB;

        stoneManager.UpdateMove();

        yield return new WaitForSeconds(0.1f);
        isSwapping = false;
        StoneManager.startFind = true;
    }
}

