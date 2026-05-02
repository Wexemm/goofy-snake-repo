using System.Collections;
using UnityEngine;

/// <summary>
/// Lever (toggle): player 2 másodpercig áll rajta → vált állapotot.
/// Ha senki nem áll rajta, a timer resetelődik.
/// Aktiváláskor kattanás-animáció és kis particle-burst.
/// </summary>
public class ColorLever : MonoBehaviour
{
    public GateColor color;
    public float requiredHoldTime = 1f;
    public Sprite inactiveSprite;
    public Sprite activeSprite;

    private SpriteRenderer sr;
    private SpriteRenderer overlaySr;
    private bool isOn = false;
    private int playersOn = 0;
    private float holdTime = 0f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // Overlay child: az active sprite halványan látszik, ahogy a töltés halad
        GameObject overlayObj = new GameObject("ActiveOverlay");
        overlayObj.transform.SetParent(transform, false);
        overlayObj.transform.localPosition = Vector3.zero;
        overlayObj.transform.localScale = Vector3.one;

        overlaySr = overlayObj.AddComponent<SpriteRenderer>();
        overlaySr.sortingLayerID = sr.sortingLayerID;
        overlaySr.sortingOrder = sr.sortingOrder + 1;
        overlaySr.color = new Color(1f, 1f, 1f, 0f);
    }

    void Start()
    {
        overlaySr.sprite = activeSprite;
        UpdateVisual();
    }

    void Update()
    {
        if (playersOn <= 0)
        {
            // Overlay fades out instantly when nobody is standing on it
            if (!isOn) SetOverlayAlpha(0f);
            return;
        }

        holdTime += Time.deltaTime;

        // Show active sprite bleeding through as progress increases (only when turning ON)
        if (!isOn)
            SetOverlayAlpha(holdTime / requiredHoldTime);

        if (holdTime >= requiredHoldTime)
        {
            holdTime = 0f;
            Toggle();
        }
    }

    public void NotifyEnemyEnter() { playersOn++; }

    public void NotifyEnemyExit()
    {
        playersOn = Mathf.Max(0, playersOn - 1);
        if (playersOn == 0) holdTime = 0f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() != null || other.GetComponent<EnemyBase>() != null) playersOn++;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() == null && other.GetComponent<EnemyBase>() == null) return;
        playersOn = Mathf.Max(0, playersOn - 1);
        if (playersOn == 0) holdTime = 0f;
    }

    void Toggle()
    {
        isOn = !isOn;
        foreach (var gate in FindObjectsByType<ColorGate>(FindObjectsInactive.Exclude))
        {
            if (gate.color != color) continue;
            if (isOn) gate.AddActivation();
            else gate.RemoveActivation();
        }
        UpdateVisual();
        StartCoroutine(ClickAnimation());
        SpawnClickParticles();
    }

    void UpdateVisual()
    {
        if (sr == null || inactiveSprite == null || activeSprite == null) return;
        sr.sprite = isOn ? activeSprite : inactiveSprite;
        SetOverlayAlpha(0f); // overlay már nem kell, a fő sprite mutatja az állapotot
    }

    void SetOverlayAlpha(float alpha)
    {
        if (overlaySr == null) return;
        Color c = overlaySr.color;
        c.a = alpha;
        overlaySr.color = c;
    }

    IEnumerator ClickAnimation()
    {
        Vector3 originalScale = transform.localScale;

        // Squish in
        float squishDuration = 0.06f;
        float squishScale = 0.78f;
        for (float t = 0f; t < squishDuration; t += Time.deltaTime)
        {
            float factor = Mathf.Lerp(1f, squishScale, t / squishDuration);
            transform.localScale = originalScale * factor;
            yield return null;
        }

        // Spring back out
        float releaseDuration = 0.14f;
        for (float t = 0f; t < releaseDuration; t += Time.deltaTime)
        {
            float factor = Mathf.Lerp(squishScale, 1f, t / releaseDuration);
            transform.localScale = originalScale * factor;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    void SpawnClickParticles()
    {
        Color particleColor = sr != null ? sr.color : Color.white;

        GameObject obj = new GameObject("LeverClickParticles");
        obj.transform.position = transform.position;

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.startColor      = particleColor;
        main.startSpeed      = new ParticleSystem.MinMaxCurve(1.5f, 4f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.05f, 0.13f);
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
        main.duration        = 0.1f;
        main.loop            = false;
        main.gravityModifier = 0.25f;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 18) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius    = 0.05f;

        var psr = ps.GetComponent<ParticleSystemRenderer>();
        psr.material     = new Material(Shader.Find("Sprites/Default"));
        psr.sortingOrder = 10;

        ps.Play();
        Destroy(obj, 2f);
    }
}
