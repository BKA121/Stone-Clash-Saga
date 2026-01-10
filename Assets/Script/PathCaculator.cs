using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCaculator
{
    public StoneBehaviour[,] boardStone;
    public int boardRow, boardCol;

    public PathCaculator(StoneBehaviour[,] boardStone, int boardRow, int boardCol)
    {
        this.boardStone = boardStone;
        this.boardRow = boardRow;
        this.boardCol = boardCol;
    }

    public List<MovePathOfStone> GetMovePathOfStones()
    {
        Dictionary<StoneBehaviour, MovePathOfStone> stonePathMap =
            new Dictionary<StoneBehaviour, MovePathOfStone>();
        for (int j = 0; j < boardCol; j++)
        {
            for (int i = 0; i < boardRow; i++)
            {
                StoneBehaviour stone = boardStone[i, j];
                if (stone == null || stone.stoneType == StoneType.Ice) continue;

                if (!stonePathMap.TryGetValue(stone, out MovePathOfStone movePath))
                {
                    movePath = new MovePathOfStone(stone, i, j);
                    stonePathMap.Add(stone, movePath);
                }
                int curRow = i;
                int curCol = j;
                int fallDistance = 0;
                for (int r = curRow - 1; r >= 0; r--)
                {
                    if (boardStone[r, curCol] == null) fallDistance++;
                    else break;
                }

                if (fallDistance > 0)
                {
                    boardStone[curRow, curCol] = null;
                    curRow -= fallDistance;
                    boardStone[curRow, curCol] = stone;
                    movePath.movePath.Add((curRow, curCol));
                }
                bool canMove = true;
                while (canMove)
                {
                    canMove = false;

                    if (curRow - 1 >= 0 && curCol - 1 >= 0 &&
                        boardStone[curRow - 1, curCol] != null &&
                        boardStone[curRow - 1, curCol - 1] == null &&
                        boardStone[curRow, curCol - 1] != null &&
                        boardStone[curRow, curCol - 1].stoneType == StoneType.Ice)
                    {
                        boardStone[curRow, curCol] = null;
                        curRow--; curCol--;
                        boardStone[curRow, curCol] = stone;
                        movePath.movePath.Add((curRow, curCol));
                        canMove = true;
                        continue;
                    }

                    if (curRow - 1 >= 0 && curCol + 1 < boardCol &&
                        boardStone[curRow - 1, curCol] != null &&
                        boardStone[curRow - 1, curCol + 1] == null &&
                        boardStone[curRow, curCol + 1] != null &&
                        boardStone[curRow, curCol + 1].stoneType == StoneType.Ice)
                    {
                        boardStone[curRow, curCol] = null;
                        curRow--; curCol++;
                        boardStone[curRow, curCol] = stone;

                        movePath.movePath.Add((curRow, curCol));
                        canMove = true;
                        continue;
                    }

                    int extraFall = 0;
                    for (int r = curRow - 1; r >= 0; r--)
                    {
                        if (boardStone[r, curCol] == null) extraFall++;
                        else break;
                    }

                    if (extraFall > 0)
                    {
                        boardStone[curRow, curCol] = null;
                        curRow -= extraFall;
                        boardStone[curRow, curCol] = stone;

                        movePath.movePath.Add((curRow, curCol));
                        canMove = true;
                    }
                }
            }
        }

        List<MovePathOfStone> result = new List<MovePathOfStone>();
        foreach (var kv in stonePathMap)
        {
            if (kv.Value.movePath.Count > 0)
                result.Add(kv.Value);
        }

        return result;
    }
}

