using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/// <summary>
/// Verseny módban megjelenő nyertes overlay.
/// Teljes képernyős sötét réteget és a győztes nevét mutatja,
/// majd "Új pálya" / "Főmenü" gombokat kínál.
/// </summary>
public class RaceWinUI : MonoBehaviour
{
    /// <summary>
    /// Létrehozza és megjeleníti a nyertes UI-t.
    /// </summary>
    public static void Show(int winnerPlayer)
    {
        var go = new GameObject("RaceWinUI");
        var ui = go.AddComponent<RaceWinUI>();
        ui._winner = winnerPlayer;
    }

    private int _winner;

    void Start()
    {
        Time.timeScale = 0f;  // játék megáll

        // PanelSettings automatikus keresés
        PanelSettings ps = null;
        var found = Resources.FindObjectsOfTypeAll<PanelSettings>();
        if (found.Length > 0) ps = found[0];

        if (ps == null)
        {
            Debug.LogWarning("[RaceWinUI] Nem található PanelSettings!");
            return;
        }

        BuildUI(ps);
    }

    void BuildUI(PanelSettings ps)
    {
        var docGO = new GameObject("RaceWinUI_Document");
        var doc   = docGO.AddComponent<UIDocument>();
        doc.panelSettings = ps;

        var root = doc.rootVisualElement;

        // ── Teljes képernyős sötét overlay ──
        var overlay = new VisualElement();
        overlay.style.position        = Position.Absolute;
        overlay.style.left = overlay.style.top =
        overlay.style.right = overlay.style.bottom = 0;
        overlay.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.80f));
        overlay.style.alignItems      = Align.Center;
        overlay.style.justifyContent  = Justify.Center;
        root.Add(overlay);

        // ── Belső panel ──
        var panel = new VisualElement();
        panel.style.backgroundColor = new StyleColor(new Color(0.055f, 0.075f, 0.070f, 0.97f));
        SetBorder(panel, new Color(0.18f, 0.55f, 0.40f, 0.85f), 2);
        SetRadius(panel, 10);
        panel.style.paddingTop    = panel.style.paddingBottom = 38;
        panel.style.paddingLeft   = panel.style.paddingRight  = 56;
        panel.style.alignItems    = Align.Center;
        overlay.Add(panel);

        // ── Játékos neve (piros = P1, kék = P2) ──
        Color playerColor = _winner == 1
            ? new Color(0.95f, 0.25f, 0.25f, 1f)
            : new Color(0.20f, 0.45f, 0.95f, 1f);

        var nameLabel = new Label(_winner == 1 ? "PLAYER 1" : "PLAYER 2");
        nameLabel.style.fontSize     = 48;
        nameLabel.style.color        = new StyleColor(playerColor);
        nameLabel.style.letterSpacing = 7;
        nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        nameLabel.style.marginBottom = 6;
        panel.Add(nameLabel);

        // ── Alcím ──
        var sub = new Label("CÉLBA ÉRT ELŐSZÖR!");
        sub.style.fontSize     = 16;
        sub.style.color        = new StyleColor(new Color(0.65f, 0.88f, 0.72f, 1f));
        sub.style.letterSpacing = 5;
        sub.style.marginBottom = 34;
        panel.Add(sub);

        // ── Gombok ──
        MakeBtn(panel, "ÚJ PÁLYA", () =>
        {
            Time.timeScale = 1f;
            PlayerPrefs.DeleteKey("ProceduralSeed");
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });

        MakeBtn(panel, "FŐMENÜ", () =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        });
    }

    // ─── Segédfüggvények ─────────────────────────────────────────────────────

    void MakeBtn(VisualElement parent, string text, System.Action onClick)
    {
        var btn = new Button(onClick) { text = text };
        btn.style.width        = 220;
        btn.style.height       = 46;
        btn.style.marginBottom = 10;
        btn.style.fontSize     = 14;
        btn.style.letterSpacing = 4;
        btn.style.color = new StyleColor(new Color(0.70f, 0.95f, 0.82f));
        btn.style.backgroundColor = new StyleColor(new Color(0.08f, 0.28f, 0.20f));
        SetBorder(btn, new Color(0.18f, 0.55f, 0.40f, 0.80f), 1);
        SetRadius(btn, 5);
        parent.Add(btn);
    }

    static void SetBorder(VisualElement el, Color c, float w)
    {
        var sc = new StyleColor(c);
        el.style.borderTopColor    = el.style.borderBottomColor =
        el.style.borderLeftColor   = el.style.borderRightColor  = sc;
        el.style.borderTopWidth    = el.style.borderBottomWidth =
        el.style.borderLeftWidth   = el.style.borderRightWidth  = w;
    }

    static void SetRadius(VisualElement el, float r)
    {
        el.style.borderTopLeftRadius    = el.style.borderTopRightRadius    =
        el.style.borderBottomLeftRadius = el.style.borderBottomRightRadius = r;
    }
}
