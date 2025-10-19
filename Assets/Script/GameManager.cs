using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public FirestoreReader firestoreReader;
    public BoardRenderer boardRenderer;
    public StoneManager stoneManager;

    private async void Start()
    {
        LevelData levelData = await firestoreReader.LoadLevelData("level_1");
        stoneManager.Init(levelData);
        boardRenderer.RenderBoard(levelData);
    }
}
