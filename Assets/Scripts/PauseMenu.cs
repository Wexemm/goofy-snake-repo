using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

[RequireComponent(typeof(UIDocument))]
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float glitchInterval = 2.0f;

    private VisualElement _overlay;
    private Label _titleLabel;
    private bool _paused;

    // Glitch timing (uses unscaled time for pause menu)
    private float _nextGlitchTime;
    private bool _glitching;
    private int _glitchStep;
    private float _stepTimer;

    // Each entry: (className, durationSeconds)
    // null className means "no glitch class" (restore normal look)
    private static readonly (string cls, float dur)[] GlitchSequence =
    {
        ("glitch-a", 0.06f),
        (null,       0.04f),
        ("glitch-b", 0.07f),
        (null,       0.03f),
        ("glitch-c", 0.05f),
        (null,       0.04f),
        ("glitch-a", 0.04f),
        (null,       0.10f),
        ("glitch-b", 0.05f),
        (null,       0.00f),   // final step: end of burst
    };

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _overlay = root.Q<VisualElement>("pause-overlay");
        _titleLabel = root.Q<Label>("title");

        root.Q<Button>("resumeButton")?.RegisterCallback<ClickEvent>(_ => Resume());
        root.Q<Button>("menuButton")?.RegisterCallback<ClickEvent>(_ => GoToMainMenu());

        SetVisible(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_paused) Resume();
            else Pause();
        }

        if (_paused && _glitching)
        {
            RunGlitchSequence();
        }
        else if (_paused && !_glitching && Time.realtimeSinceStartup >= _nextGlitchTime)
        {
            _glitching = true;
            _glitchStep = 0;
            _stepTimer = 0f;
        }
    }

    private void Pause()
    {
        _paused = true;
        Time.timeScale = 0f;
        SetVisible(true);
        ScheduleNextGlitch();
    }

    private void Resume()
    {
        _paused = false;
        Time.timeScale = 1f;
        SetVisible(false);
        SetGlitchClass(null);
        _glitching = false;
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SetVisible(bool visible)
    {
        if (_overlay != null)
            _overlay.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // ── Glitch logic ───────────────────────────────────────────────
    private void RunGlitchSequence()
    {
        _stepTimer -= Time.unscaledDeltaTime;
        if (_stepTimer > 0f) return;

        if (_glitchStep >= GlitchSequence.Length)
        {
            // Burst finished — clear all glitch classes, wait for next burst
            SetGlitchClass(null);
            _glitching = false;
            ScheduleNextGlitch();
            return;
        }

        var (cls, dur) = GlitchSequence[_glitchStep];
        SetGlitchClass(cls);
        _stepTimer = dur;
        _glitchStep++;
    }

    private void SetGlitchClass(string cls)
    {
        if (_titleLabel == null) return;
        _titleLabel.RemoveFromClassList("glitch-a");
        _titleLabel.RemoveFromClassList("glitch-b");
        _titleLabel.RemoveFromClassList("glitch-c");
        if (cls != null)
            _titleLabel.AddToClassList(cls);
    }

    private void ScheduleNextGlitch()
    {
        // Randomise interval ±40% so it doesn't feel mechanical
        float jitter = glitchInterval * 0.4f;
        _nextGlitchTime = Time.realtimeSinceStartup + glitchInterval + Random.Range(-jitter, jitter);
    }
}
