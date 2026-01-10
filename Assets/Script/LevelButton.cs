using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    private string levelId;
    public GameManager gameManager;

    void Start()
    {
        levelId = gameObject.name.ToLower();
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        gameManager.LoadLevel(levelId);
    }
}
