using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public enum StoneType
{
    Red, Green, Blue, Purple, Yellow, Ice,
    RedMatch4, GreenMatch4, BlueMatch4, PurpleMatch4, YellowMatch4,
    RedMatchTorL, GreenMatchTorL, BlueMatchTorL, PurpleMatchTorL, YellowMatchTorL, StoneMatch5
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
    public bool isHorizontalExplosion; // true thì phá hàng 

    public void Awake()
    {
        stoneManager = FindObjectOfType<StoneManager>();
    }

    public IEnumerator FallAndSlide(List<(int row, int col)> movePath)
    {
        stoneManager.countStoneFallOrSlide += 1;
        float speed = 750f;
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

        if (stoneA.stoneType == StoneType.StoneMatch5 || stoneB.stoneType == StoneType.StoneMatch5)
        {
            StoneBehaviour target, bomb;

            if (stoneA.stoneType == StoneType.StoneMatch5)
            {
                target = stoneB;
                bomb = stoneA;
            }
            else
            {
                target = stoneA;
                bomb = stoneB;
            }

            // Xử lý nổ trong trường hợp cả hai là bomb 
            if (target.stoneType == StoneType.StoneMatch5) stoneManager.ExecuteUltraBomb();

            // Xử lý nổ trong trường hợp swap với viên bình thường 
            else stoneManager.ExecuteColorBomb(bomb, target.stoneType);
        }
        else if (!stoneManager.CheckStoneAfterSwap(stoneA.r, stoneA.c, stoneB.r, stoneB.c)) 
            StartCoroutine(SmoothRemoveSwap(stoneA, stoneB, stoneA.r, stoneA.c, stoneB.r, stoneB.c));
        else
        {
            StoneManager.startFind = true;
        }
    }

    private IEnumerator SmoothRemoveSwap(StoneBehaviour stoneA, StoneBehaviour stoneB, int rA, int cA, int rB, int cB)
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

        yield return new WaitForSeconds(0.1f);
        isSwapping = false;
    }
}

