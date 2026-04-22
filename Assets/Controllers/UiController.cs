using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UiController : MonoBehaviour
{

    public GameObject StartScreen;
    public GameObject GameOverScreen;
    public GameObject GameWinScreen;

    public enum ButtonFunc {START, REPLAY, QUIT}
    public enum ScreenTypes {START, END}
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // activate the correct screen based on Player prefs- either Win, Lose, Start
        string GetScreen = PlayerPrefs.GetString("Gamestate", "Start");
        SetSaveValues();

        switch(GetScreen)
        {
            case "Start":
                MainMenu();
                break;

            case "Win":
                CompleteScreen(GameWinScreen);
                break;

            case "Lose":
                CompleteScreen(GameOverScreen);
                break;
        }
    }

    private void MainMenu()
    {
        StartScreen.SetActive(true);
    }

    private void CompleteScreen(GameObject EndScreen)
    {
        EndScreen.SetActive(true);
    }

    private void SetSaveValues()
    {
        PlayerPrefs.SetInt("PlayerHealth", 200);
        PlayerPrefs.SetString("SUNFLOWER_REMAINING", "2");
        PlayerPrefs.SetString("ROSE_REMAINING", "2");
        PlayerPrefs.SetString("BLUEBELL_REMAINING", "1");
        PlayerPrefs.SetInt("TotalEnemies", 5);
        PlayerPrefs.SetString("GameState", "Start");
        PlayerPrefs.SetString("CurrentEnemy", "None");
    }

    public void OnButtonPressed(ButtonFunc function)
    {
        switch(function)
        {
            case ButtonFunc.START:
               // player prefs set first load in to true to trigger a popup
                PlayerPrefs.SetString("LoadCase", "START");
                SceneManager.LoadScene("Environment1");
                break;

            case ButtonFunc.REPLAY:
                // clear all the player refs and reset the scene in environment 1 with all the enemies
                PlayerPrefs.SetString("LoadCase", "REPLAY");
                SceneManager.LoadScene("Environment1");
                break;

            case ButtonFunc.QUIT:
                Debug.Log("Quit game");
                UnityEditor.EditorApplication.ExitPlaymode();
                Application.Quit();
                break;
        }
    }
}
