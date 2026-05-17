using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    void Awake()
    {
        instance = this;
    }

    public void Win()
    {
        Debug.Log("Győzelem!");
        int currentLevel = PlayerPrefs.GetInt("currentLevel", 0);
        int nextLevel = currentLevel + 1;

        if (LevelManager.instance == null)
        {
            Debug.LogWarning("LevelManager nincs jelen – főmenüből indítsd a játékot! Fallback: scene újratöltés.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        LevelManager.instance.UnlockNextLevel(currentLevel);

        Debug.Log($"currentLevel={currentLevel}, nextLevel={nextLevel}, totalLevels={LevelManager.instance.totalLevels}");

        if (nextLevel < LevelManager.instance.totalLevels)
        {
            LevelManager.instance.LoadLevel(nextLevel);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("EndScreen");
        }
    }

    /// <summary>
    /// Verseny módban hívódik meg, amikor az egyik játékos célba ér.
    /// </summary>
    public void RaceWin(int playerNumber)
    {
        RaceWinUI.Show(playerNumber);
    }
}