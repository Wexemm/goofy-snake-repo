using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public int totalLevels = 1;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (!PlayerPrefs.HasKey("unlockedLevels"))
        {
            PlayerPrefs.SetInt("unlockedLevels", 1);
        }
    }

    public int GetUnlockedLevels()
    {
        return PlayerPrefs.GetInt("unlockedLevels");
    }

    public void UnlockNextLevel(int currentLevel)
    {
        int unlocked = GetUnlockedLevels();
        if (currentLevel + 1 < totalLevels && currentLevel + 1 >= unlocked)
        {
            PlayerPrefs.SetInt("unlockedLevels", currentLevel + 2);
            PlayerPrefs.Save();
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.SetInt("unlockedLevels", 1);
        PlayerPrefs.SetInt("currentLevel", 0);
        PlayerPrefs.Save();
    }

    public void LoadLevel(int levelIndex)
    {
        PlayerPrefs.SetInt("currentLevel", levelIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Game");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}