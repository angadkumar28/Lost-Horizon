using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject gameOverPanel;
    private StationaryPlayer playerScript;

    void Start()
    {
        playerScript = Object.FindFirstObjectByType<StationaryPlayer>();

        mainMenuPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        Time.timeScale = 0; 
    }

    public void PlayGame()
    {
        mainMenuPanel.SetActive(false);
        Time.timeScale = 1;
        if (playerScript != null)
        {
            playerScript.StartGame();
        }
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}