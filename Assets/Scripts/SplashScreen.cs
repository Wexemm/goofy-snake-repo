using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] private string backgroundImagePath = "Background";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool _canContinue = false;
    private float _minDisplayTime = 1.5f;
    private Canvas _canvas;

    private void Start()
    {
        CreateSplashBackground();
        Invoke(nameof(ShowHintText), 1.0f); // Show text after 1 second
        Invoke(nameof(AllowContinue), _minDisplayTime);
    }

    private void CreateSplashBackground()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("SplashCanvas");
        _canvas = canvasObj.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 1000; // Place on top
        canvasObj.AddComponent<GraphicRaycaster>();
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Create background image
        GameObject imageObj = new GameObject("BackgroundImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        Image image = imageObj.AddComponent<Image>();
        RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // Load and set texture
        var texture = Resources.Load<Texture2D>(backgroundImagePath);
        if (texture != null)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            image.sprite = sprite;
            Debug.Log("Splash background image created successfully");
        }
        else
        {
            Debug.LogWarning($"Could not load image at Resources/{backgroundImagePath}");
        }
    }

    private void ShowHintText()
    {
        if (_canvas == null) return;

        // Create hint text
        GameObject textObj = new GameObject("HintText");
        textObj.transform.SetParent(_canvas.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = "";
        text.color = new Color(0.3f, 0.55f, 0.47f, 0.8f);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.15f);
        textRect.anchorMax = new Vector2(0.5f, 0.15f);
        textRect.sizeDelta = new Vector2(400, 50);
        textRect.anchoredPosition = Vector2.zero;
        
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 14;

        Debug.Log("Hint text displayed");
    }

    private void CreateSplashUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("SplashCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // Place on top
        canvasObj.AddComponent<GraphicRaycaster>();
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Create background image
        GameObject imageObj = new GameObject("BackgroundImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        Image image = imageObj.AddComponent<Image>();
        RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // Load and set texture
        var texture = Resources.Load<Texture2D>(backgroundImagePath);
        if (texture != null)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            image.sprite = sprite;
            Debug.Log("Splash background image created successfully");
        }
        else
        {
            Debug.LogWarning($"Could not load image at Resources/{backgroundImagePath}");
        }

        // Create hint text
        GameObject textObj = new GameObject("HintText");
        textObj.transform.SetParent(canvasObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = "";
        text.color = new Color(0.3f, 0.55f, 0.47f, 0.8f);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.1f);
        textRect.anchorMax = new Vector2(0.5f, 0.1f);
        textRect.sizeDelta = new Vector2(400, 50);
        textRect.anchoredPosition = Vector2.zero;
        
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 14;
    }

    private void Update()
    {
        if (!_canContinue) return;

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            GoToMainMenu();
        }
    }

    private void AllowContinue()
    {
        _canContinue = true;
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
