using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePathOfStone 
{
    public StoneBehaviour stone;
    public int curRow, curCol;
    public List<(int row, int col)> movePath;

    public MovePathOfStone(StoneBehaviour stone, int curRow, int curCol)
    {
        this.stone = stone;
        this.curRow = curRow;
        this.curCol = curCol;
        movePath = new List<(int row, int col)>();
    }
}
