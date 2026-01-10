using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject levelSelectionMenu;
    public GameObject gameplayUI;
    public GameObject settingPopup;
    public BoardRenderer boardRenderer;

    public void ShowGameplay()
    {
        levelSelectionMenu.SetActive(false);
        gameplayUI.SetActive(true);
    }

    public void ShowLevelMenu()
    {
        levelSelectionMenu.SetActive(true);
        gameplayUI.SetActive(false);
    }

    public void OpenSetting()
    {
        settingPopup.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        settingPopup.SetActive(false);
        Time.timeScale = 1; 
    }

    public void ExitLevel()
    {
        Time.timeScale = 1;
        boardRenderer.ClearBoard();
        settingPopup.SetActive(false);
        gameplayUI.SetActive(false);
        levelSelectionMenu.SetActive(true);

        // FindObjectOfType<BoardRenderer>().ClearBoard(); 
    }
}