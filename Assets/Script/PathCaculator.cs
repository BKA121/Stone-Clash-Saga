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
        this.boardCol = boardCol;
        this.boardRow = boardRow;
    }

    // Ham tra ve list tat ca duong di cua stone trong board
    public List<MovePathOfStone> GetMovePathOfStones()
    {
        var moveAllPath = new List<MovePathOfStone>();

        for(int j=0; j<boardCol; j++)
        {
            for(int i=0; i<boardRow; i++)
            {
                if (boardStone[i, j] == null || boardStone[i, j].stoneType == StoneType.Ice) continue;

                var stone = boardStone[i, j];
                var movePathOfStone = new MovePathOfStone(stone, i, j);

                // Tinh roi thang
                int fallDistance = 0;
                for (int r = i - 1; r >= 0; r--) 
                {
                    if (boardStone[r, j] == null) fallDistance++;
                    else break;
                }
                int curRow = i;
                int curCol = j;
                if (fallDistance > 0)
                {
                    boardStone[curRow, curCol] = null;
                    curRow -= fallDistance;
                    boardStone[curRow, curCol] = stone;
                    movePathOfStone.movePath.Add((curRow, curCol));
                }

                bool isSlideOrFall = true;
                while (isSlideOrFall)
                {
                    isSlideOrFall = false;

                    // Tinh toan truot trai
                    // Dieu kien: ton tai o cheo duoi, o do phai trong
                    // Vi tri luc tinh toan truot cheo la curRow, curCol
                    if (curRow - 1 >= 0 && curCol - 1 >= 0 && boardStone[curRow - 1, curCol] != null &&
                        boardStone[curRow - 1, curCol - 1] == null &&
                        boardStone[curRow, curCol - 1] != null &&
                        boardStone[curRow, curCol - 1].stoneType == StoneType.Ice)
                    {
                        boardStone[curRow, curCol] = null;
                        curRow -= 1; curCol -= 1;
                        boardStone[curRow, curCol] = stone;
                        movePathOfStone.movePath.Add((curRow, curCol));
                        isSlideOrFall = true;
                        continue;
                    }

                    // Tinh toan truot phai
                    // Dieu kien: ton tai o cheo phai va trong, o canh phai la ice
                    // Vi tri luc tinh toan truot phai la curRow, curCol
                    if (curRow - 1 >= 0 && curCol + 1 < boardCol && boardStone[curRow - 1, curCol] != null &&
                        boardStone[curRow - 1, curCol + 1] == null &&
                        boardStone[curRow, curCol + 1] != null &&
                        boardStone[curRow, curCol + 1].stoneType == StoneType.Ice)
                    {
                        boardStone[curRow, curCol] = null;
                        curRow -= 1; curCol += 1;
                        boardStone[curRow, curCol] = stone;
                        movePathOfStone.movePath.Add((curRow, curCol));
                        isSlideOrFall = true;
                        continue;
                    }

                    // Kiem tra roi thang neu co the
                    if (!isSlideOrFall)
                    {
                        fallDistance = 0;
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
                            movePathOfStone.movePath.Add((curRow, curCol));

                            isSlideOrFall = true;
                        }
                    }
                }

                if (movePathOfStone.movePath.Count > 0) moveAllPath.Add(movePathOfStone);

            }
        }
        return moveAllPath;
    }
}
