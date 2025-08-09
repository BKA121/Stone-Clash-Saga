using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public FirestoreReader firestoreReader;
    public BoardRenderer boardRenderer;

    private async void Start()
    {
        LevelData levelData = await firestoreReader.LoadLevelData("level_1");
        boardRenderer.RenderBoard(levelData);
    }
}
