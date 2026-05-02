using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private VisualElement _mainContainer;
    private VisualElement _levelSelectPanel;
    private VisualElement _levelGrid;
    private Button _continueButton;

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        if (root == null) return;

        _mainContainer = root.Q<VisualElement>("container");
        _levelSelectPanel = root.Q<VisualElement>("level-select-panel");
        _levelGrid = root.Q<VisualElement>("level-grid");

        _continueButton = root.Q<Button>("continueButton");
        var startButton = root.Q<Button>("startButton");
        var levelsButton = root.Q<Button>("levelsButton");
        var quitButton = root.Q<Button>("quitButton");
        var backButton = root.Q<Button>("backButton");

        if (_continueButton != null) _continueButton.clicked += ContinueGame;
        if (startButton != null) startButton.clicked += NewGame;
        if (levelsButton != null) levelsButton.clicked += ShowLevelSelect;
        if (quitButton != null) quitButton.clicked += QuitGame;
        if (backButton != null) backButton.clicked += HideLevelSelect;

        // Hide continue if no save exists
        bool hasSave = PlayerPrefs.GetInt("currentLevel", 0) > 0;
        if (_continueButton != null)
            _continueButton.style.display = hasSave ? DisplayStyle.Flex : DisplayStyle.None;

        ShowMainMenu();
    }

    void ContinueGame()
    {
        int level = PlayerPrefs.GetInt("currentLevel", 0);
        LevelManager.instance.LoadLevel(level);
    }

    void NewGame()
    {
        LevelManager.instance.ResetProgress();
        LevelManager.instance.LoadLevel(0);
    }

    void ShowLevelSelect()
    {
        _mainContainer.style.display = DisplayStyle.None;
        _levelSelectPanel.style.display = DisplayStyle.Flex;
        BuildLevelGrid();
    }

    void HideLevelSelect()
    {
        ShowMainMenu();
    }

    void ShowMainMenu()
    {
        _mainContainer.style.display = DisplayStyle.Flex;
        _levelSelectPanel.style.display = DisplayStyle.None;
    }

    void BuildLevelGrid()
    {
        _levelGrid.Clear();

        int totalLevels = LevelManager.instance != null ? LevelManager.instance.totalLevels : 6;
        int unlocked = PlayerPrefs.GetInt("unlockedLevels", 1);

        for (int i = 0; i < totalLevels; i++)
        {
            var btn = new Button();
            btn.text = (i + 1).ToString("D2");
            btn.AddToClassList("level-btn");

            if (i < unlocked)
            {
                btn.AddToClassList("level-btn-unlocked");
                int levelIndex = i;
                btn.clicked += () => LevelManager.instance.LoadLevel(levelIndex);
            }
            else
            {
                btn.AddToClassList("level-btn-locked");
                btn.SetEnabled(false);
            }

            _levelGrid.Add(btn);
        }
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}