using UnityEngine;

/// <summary>
/// A sprite-ot automatikusan a LevelGenerator.cellSize-ra skálázza.
/// A BoxCollider2D mindig pontosan egy cellát fed le (nincs rés köztük).
/// </summary>
[ExecuteAlways]
public class FitToCell : MonoBehaviour
{
    [Tooltip("Sorting order a SpriteRendererhez (floor = 0, tehát legalább 1 kell)")]
    public int sortingOrder = 1;

    void OnEnable() => Fit();
    void Start()    => Fit();

    void Fit()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        float cell = GetCellSize();
        Vector2 spriteSize = sr.sprite.bounds.size;
        if (spriteSize.x <= 0 || spriteSize.y <= 0) return;

        float scale = Mathf.Min(cell / spriteSize.x, cell / spriteSize.y);
        transform.localScale = new Vector3(scale, scale, 1f);
        sr.sortingOrder = sortingOrder;

        var col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            // world size = localSize * scale = cell  =>  localSize = cell / scale
            float localSize = cell / scale;
            col.size = new Vector2(localSize, localSize);
        }
    }

    float GetCellSize()
    {
        var gen = FindAnyObjectByType<LevelGenerator>();
        return gen != null ? gen.cellSize : 0.5f;
    }
}
