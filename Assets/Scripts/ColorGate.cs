using UnityEngine;

public class ColorGate : MonoBehaviour
{
    public GateColor color;
    public Sprite inactiveSprite; // locked (lakat)
    public Sprite activeSprite;   // open

    private SpriteRenderer sr;
    private Collider2D col;
    private int activationCount = 0;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void Start()
    {
        UpdateVisual();
    }

    public void AddActivation()
    {
        activationCount++;
        UpdateVisual();
    }

    public void RemoveActivation()
    {
        activationCount = Mathf.Max(0, activationCount - 1);
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (col == null) col = GetComponent<Collider2D>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        bool open = activationCount > 0;
        if (col != null) col.enabled = !open;
        if (sr != null && inactiveSprite != null && activeSprite != null)
            sr.sprite = open ? activeSprite : inactiveSprite;

        // Rebuild the pathfinding grid so enemies respect the new gate state.
        AStarGrid.Instance?.BuildGrid();
    }
}
