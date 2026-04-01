using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{

    public GameObject StartScreen;
    public GameObject GameOverScreen;
    public GameObject GameWinScreen;

    public enum ButtonFunc {START, REPLAY, QUIT}
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // activate the correct screen based on Player prefs- either Win, Lose, Start
        string GetScreen = PlayerPrefs.GetString("Gamestate", "Start");

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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnButtonPressed(ButtonFunc function)
    {
        switch(function)
        {
            case ButtonFunc.START:
                Debug.Log("Start game");
                break;

            case ButtonFunc.REPLAY:
                Debug.Log("Replay game");
                break;

            case ButtonFunc.QUIT:
            Debug.Log("Quit game");
                break;
        }
    }
}
