using UnityEngine;

/// <summary>
/// Nyomógomb: csak addig tartja aktívan a kapukat, amíg valamelyik player vagy enemy rajta áll.
/// </summary>
public class ColorButton : MonoBehaviour
{
    public GateColor color;
    public Sprite inactiveSprite; // X - senki nincs rajta
    public Sprite activeSprite;   // bekapcsolt állapot

    private SpriteRenderer sr;
    private int playersOn = 0;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        UpdateVisual();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() == null && other.GetComponent<EnemyBase>() == null) return;
        playersOn++;
        if (playersOn == 1) SetGates(true);
        UpdateVisual();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() == null && other.GetComponent<EnemyBase>() == null) return;
        playersOn = Mathf.Max(0, playersOn - 1);
        if (playersOn == 0) SetGates(false);
        UpdateVisual();
    }

    public void NotifyEnemyEnter()
    {
        playersOn++;
        if (playersOn == 1) SetGates(true);
        UpdateVisual();
    }

    public void NotifyEnemyExit()
    {
        playersOn = Mathf.Max(0, playersOn - 1);
        if (playersOn == 0) SetGates(false);
        UpdateVisual();
    }

    void SetGates(bool activate)
    {
        foreach (var gate in FindObjectsByType<ColorGate>(FindObjectsInactive.Exclude))
        {
            if (gate.color != color) continue;
            if (activate) gate.AddActivation();
            else gate.RemoveActivation();
        }
    }

    void UpdateVisual()
    {
        if (sr == null || inactiveSprite == null || activeSprite == null) return;
        sr.sprite = playersOn > 0 ? activeSprite : inactiveSprite;
    }
}
