using UnityEngine;
using UnityEngine.UIElements;

public class EndScreen : MonoBehaviour
{
    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) { Debug.LogError("EndScreen: UIDocument hiányzik!"); return; }

        var root = uiDocument.rootVisualElement;
        if (root == null) { Debug.LogError("EndScreen: rootVisualElement null!"); return; }

        var menuButton = root.Q<Button>("menuButton");
        if (menuButton == null) { Debug.LogError("EndScreen: menuButton nem található!"); return; }

        Debug.Log("EndScreen: menuButton megtalálva, esemény regisztrálva.");
        menuButton.clicked += GoToMenu;
    }

    void GoToMenu()
    {
        if (LevelManager.instance != null)
            LevelManager.instance.LoadMainMenu();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
