using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/// <summary>
/// Drives the Main Menu UI: wires up buttons and runs the
/// periodic glitch animation on the title label.
/// Attach this component to the GameObject that has the UIDocument.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class MainMenuUI : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────
    [Tooltip("Average seconds between glitch bursts.")]
    [SerializeField] private float glitchInterval = 3.5f;

    [Tooltip("First scene to load when the player clicks Start.")]
    [SerializeField] private string gameSceneName = "Game";

    // ── Private ────────────────────────────────────────────────────
    private Label _titleLabel;

    // Glitch timing
    private float _nextGlitchTime;
    private bool  _glitching;
    private int   _glitchStep;
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

    // ── Unity lifecycle ────────────────────────────────────────────
    private void OnEnable()
    {
        var doc  = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        _titleLabel = root.Q<Label>("title");

        root.Q<Button>("startButton")    ?.RegisterCallback<ClickEvent>(_ => OnStart());
        root.Q<Button>("labyrinthButton")?.RegisterCallback<ClickEvent>(_ => OnLabyrinth());
        root.Q<Button>("quitButton")     ?.RegisterCallback<ClickEvent>(_ => OnQuit());

        ScheduleNextGlitch();
    }

    private void Update()
    {
        if (_glitching)
        {
            RunGlitchSequence();
        }
        else if (Time.time >= _nextGlitchTime)
        {
            _glitching  = true;
            _glitchStep = 0;
            _stepTimer  = 0f;
        }
    }

    // ── Glitch logic ───────────────────────────────────────────────
    private void RunGlitchSequence()
    {
        _stepTimer -= Time.deltaTime;
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
        // Randomise interval ±40 % so it doesn't feel mechanical
        float jitter    = glitchInterval * 0.4f;
        _nextGlitchTime = Time.time + glitchInterval + Random.Range(-jitter, jitter);
    }

    // ── Button handlers ────────────────────────────────────────────
    private void OnStart()
    {
        // Normál mód: kézzel készített pályák
        PlayerPrefs.SetInt("GameMode", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnLabyrinth()
    {
        // Labirintus mód: automatikusan generált pályák
        PlayerPrefs.SetInt("GameMode", 1);
        PlayerPrefs.DeleteKey("ProceduralSeed"); // minden alkalommal új pálya
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameSceneName);
    }

    private static void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
