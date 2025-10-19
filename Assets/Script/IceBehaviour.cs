using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBehaviour : StoneBehaviour
{
    public override void Start()
    {
        stoneManager = StoneManager.FindObjectOfType<StoneManager>();
    }

    public override void Update()
    {
        return;
    }

    public override void CheckSlideOrSettle()
    {
        int row = Mathf.RoundToInt(transform.position.y);
        int col = Mathf.RoundToInt(transform.position.x);

        Settle(row, col);
    }

}
