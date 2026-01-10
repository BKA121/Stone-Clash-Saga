using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TargetUIHandler : MonoBehaviour
{
    public GameObject targetItemPrefab; 
    public Transform container;   
    public TMP_Text movesText;
    private Dictionary<string, TMP_Text> textMap = new Dictionary<string, TMP_Text>();

    public void InitializeUI(Dictionary<string, int> targetList, Sprite[] stoneSprites, int maxMove)
    {
        UpdateMovesUI(maxMove);

        foreach (Transform child in container) Destroy(child.gameObject);
        textMap.Clear();

        foreach (var target in targetList)
        {
            GameObject stoneTarget = Instantiate(targetItemPrefab, container);

            TargetItemInfo info = stoneTarget.GetComponent<TargetItemInfo>();

            if (info != null)
            {
                info.stoneTarget.sprite = System.Array.Find(stoneSprites, s => s.name == target.Key);

                info.countStoneTarget.text = target.Value.ToString();

                textMap.Add(target.Key, info.countStoneTarget);
            }
        }
    }

    public void UpdateMovesUI(int curMove)
    {
        if (movesText != null)
        {
            movesText.text = curMove.ToString();
        }
    }

    public void UpdateCountTargetStoneUI(string typeName, int remainingAmount)
    {
        if (textMap.ContainsKey(typeName))
        {
            textMap[typeName].text = remainingAmount.ToString();
        }
    }
}