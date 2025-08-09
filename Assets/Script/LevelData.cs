using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData
{
    public int row;
    public int column;
    public List<(int x, int y)> positionBlockList = new List<(int x, int y)>();
    public List<string> ruleList = new List<string>();
}
