using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/// <summary>
/// In-game UI panel procedurális pályákhoz.
/// Automatikusan létrehozza a saját UIDocument-jét — nem kell prefab.
/// Megjelenik jobb alul: Seed kijelző + Új pálya / Mentés / Betöltés gombok.
/// </summary>
public class ProceduralMapUI : MonoBehaviour
{
    [Tooltip("A projekt PanelSettings assete (Assets/UI/PanelSettings.asset). " +
             "Ha üres, automatikusan megpróbálja megtalálni.")]
    public PanelSettings panelSettings;

    const string KEY_CURRENT = "ProceduralSeed";
    const string KEY_SAVED   = "SavedMapSeed";

    // Neon témához illő színek
    static readonly Color BgColor      = new Color(0.055f, 0.075f, 0.070f, 0.95f);
    static readonly Color BorderColor  = new Color(0.18f,  0.55f,  0.40f,  0.55f);
    static readonly Color TextColor    = new Color(0.70f,  0.95f,  0.82f,  1.00f);
    static readonly Color DimColor     = new Color(0.38f,  0.55f,  0.47f,  1.00f);
    static readonly Color BtnGreen     = new Color(0.08f,  0.30f,  0.20f,  1.00f);
    static readonly Color BtnBlue      = new Color(0.08f,  0.18f,  0.32f,  1.00f);
    static readonly Color BtnOrange    = new Color(0.30f,  0.18f,  0.06f,  1.00f);
    static readonly Color BtnDisabled  = new Color(0.12f,  0.15f,  0.14f,  1.00f);

    private Label  seedLabel;
    private Button loadBtn;
    private int    currentSeed;

    void Start()
    {
        currentSeed = PlayerPrefs.GetInt(KEY_CURRENT, 0);

        // PanelSettings automatikus keresés, ha nincs megadva
        if (panelSettings == null)
        {
            var found = Resources.FindObjectsOfTypeAll<PanelSettings>();
            if (found.Length > 0)
                panelSettings = found[0];
            else
            {
                Debug.LogWarning("[ProceduralMapUI] Nem található PanelSettings! " +
                                 "Húzd be az Assets/UI/PanelSettings.asset-et a LevelGenerator Inspectorába.");
                return;
            }
        }

        BuildUI();
    }

    // ─── UI felépítése kódból ────────────────────────────────────────────────

    void BuildUI()
    {
        var go  = new GameObject("ProceduralMapUI_Document");
        var doc = go.AddComponent<UIDocument>();
        doc.panelSettings = panelSettings;

        var root = doc.rootVisualElement;

        // ── Külső keret (jobb alsó sarok) ──
        var panel = new VisualElement();
        panel.style.position          = Position.Absolute;
        panel.style.right             = 18;
        panel.style.bottom            = 18;
        panel.style.width             = 210;
        panel.style.backgroundColor   = new StyleColor(BgColor);
        panel.style.borderTopWidth    = 1;
        panel.style.borderBottomWidth = 1;
        panel.style.borderLeftWidth   = 1;
        panel.style.borderRightWidth  = 1;
        panel.style.borderTopColor    = new StyleColor(BorderColor);
        panel.style.borderBottomColor = new StyleColor(BorderColor);
        panel.style.borderLeftColor   = new StyleColor(BorderColor);
        panel.style.borderRightColor  = new StyleColor(BorderColor);
        SetRadius(panel, 7);
        panel.style.paddingTop    = 12;
        panel.style.paddingBottom = 12;
        panel.style.paddingLeft   = 14;
        panel.style.paddingRight  = 14;
        root.Add(panel);

        // ── Cím ──
        var title = new Label("PROCEDURÁLIS PÁLYA");
        title.style.color       = new StyleColor(TextColor);
        title.style.fontSize    = 11;
        title.style.letterSpacing = 2;
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.marginBottom = 4;
        panel.Add(title);

        // ── Elválasztó vonal ──
        var divider = new VisualElement();
        divider.style.height          = 1;
        divider.style.backgroundColor = new StyleColor(BorderColor);
        divider.style.marginBottom    = 8;
        panel.Add(divider);

        // ── Seed kijelző ──
        seedLabel = new Label($"seed: {currentSeed}");
        seedLabel.style.color      = new StyleColor(DimColor);
        seedLabel.style.fontSize   = 10;
        seedLabel.style.letterSpacing = 1;
        seedLabel.style.marginBottom  = 10;
        panel.Add(seedLabel);

        // ── Gombok ──
        MakeButton(panel, "ÚJ PÁLYA GENERÁLÁSA", OnNewMap,  BtnGreen);
        MakeButton(panel, "PÁLYA MENTÉSE",        OnSave,   BtnBlue);
        loadBtn = MakeButton(panel, "MENTETT PÁLYA BETÖLTÉSE", OnLoad, BtnOrange);

        bool hasSave = PlayerPrefs.HasKey(KEY_SAVED);
        loadBtn.SetEnabled(hasSave);
        if (!hasSave)
            loadBtn.style.backgroundColor = new StyleColor(BtnDisabled);
    }

    Button MakeButton(VisualElement parent, string text, System.Action onClick, Color bg)
    {
        var btn = new Button(onClick) { text = text };
        btn.style.marginBottom   = 5;
        btn.style.height         = 30;
        btn.style.fontSize       = 10;
        btn.style.letterSpacing  = 1;
        btn.style.color          = new StyleColor(TextColor);
        btn.style.backgroundColor = new StyleColor(bg);
        btn.style.borderTopWidth = btn.style.borderBottomWidth =
        btn.style.borderLeftWidth = btn.style.borderRightWidth = 1;
        btn.style.borderTopColor = btn.style.borderBottomColor =
        btn.style.borderLeftColor = btn.style.borderRightColor = new StyleColor(BorderColor);
        SetRadius(btn, 4);
        parent.Add(btn);
        return btn;
    }

    static void SetRadius(VisualElement el, float r)
    {
        el.style.borderTopLeftRadius     = r;
        el.style.borderTopRightRadius    = r;
        el.style.borderBottomLeftRadius  = r;
        el.style.borderBottomRightRadius = r;
    }

    // ─── Gomb események ─────────────────────────────────────────────────────

    void OnNewMap()
    {
        // Törli a jelenlegi seedet → Generate() új véletlent kap
        PlayerPrefs.DeleteKey(KEY_CURRENT);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnSave()
    {
        PlayerPrefs.SetInt(KEY_SAVED, currentSeed);
        PlayerPrefs.Save();

        loadBtn.SetEnabled(true);
        loadBtn.style.backgroundColor = new StyleColor(BtnOrange);

        seedLabel.text = $"seed: {currentSeed}  ✓ elmentve";
    }

    void OnLoad()
    {
        int saved = PlayerPrefs.GetInt(KEY_SAVED, 0);
        PlayerPrefs.SetInt(KEY_CURRENT, saved);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
