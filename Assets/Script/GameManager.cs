using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public FirestoreReader firestoreReader;
    public BoardRenderer boardRenderer;
    public StoneManager stoneManager;
    public UIManager uiManager;

    private void Awake()
    {
        uiManager.ShowLevelMenu();
    }

    public async void LoadLevel(string levelId)
    {
        uiManager.ShowGameplay();

        LevelData levelData = await firestoreReader.LoadLevelData(levelId);
        stoneManager.Init(levelData);
        boardRenderer.RenderBoard(levelData);
    }

}
