using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float acceleration = 20f;
    public float maxSpeed = 12f;
    public float drag = 3f;

    public Key upKey;
    public Key downKey;
    public Key leftKey;
    public Key rightKey;

    [Header("Trail")]
    public Color trailColor = new Color(0.20f, 0.72f, 0.13f, 1f);
    public float trailTime = 2.5f;
    public float trailWidth = 0.3f;

    [Header("Base Body")]
    public float minBodyLength = 1.8f;
    public int bodySegments = 20;

    [Header("Wall Hit Feedback")]
    public float wallHitMinSpeed = 3f;

    private Rigidbody2D rb;
    private TrailRenderer trail;
    private LineRenderer baseBody;
    private Vector2[] posHistory;
    private int histHead = 0;
    private float histSpacing = 0.07f;
    private float distAccum = 0f;
    private Vector2 prevPos;
    private bool isShaking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = drag;
        rb.gravityScale = 0f;

        // Determine tail color from PlayerIdentity
        var identity = GetComponent<PlayerIdentity>();
        Color tailColor = (identity != null && identity.playerColor == PlayerColor.Blue)
            ? new Color(0.10f, 0.40f, 0.95f, 1f)   // kék farok
            : new Color(0.90f, 0.10f, 0.10f, 1f);   // piros farok

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        string sortLayer = sr != null ? sr.sortingLayerName : "Default";
        int sortOrder   = sr != null ? sr.sortingOrder : 0;

        // Pozíció history buffer (kígyótest útvonalhoz)
        posHistory = new Vector2[bodySegments];
        prevPos = transform.position;
        for (int i = 0; i < bodySegments; i++)
            posHistory[i] = (Vector2)transform.position + Vector2.down * i * histSpacing;

        // Base body LineRenderer – a pozíció-history alapján rajzolódik
        baseBody = gameObject.AddComponent<LineRenderer>();
        baseBody.positionCount = bodySegments;
        baseBody.startWidth = trailWidth;
        baseBody.endWidth   = trailWidth * 0.28f;
        baseBody.material   = new Material(Shader.Find("Sprites/Default"));
        baseBody.colorGradient = BuildGradient(trailColor, tailColor);
        baseBody.sortingLayerName = sortLayer;
        baseBody.sortingOrder     = sortOrder - 2;
        baseBody.useWorldSpace    = true;
        baseBody.numCapVertices   = 4;
        baseBody.generateLightingData = false;
        baseBody.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        UpdateBaseBody();

        // Trail renderer – mozgáskor húzódó csík (a body fölé)
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = trailTime;
        trail.startWidth = trailWidth;
        trail.endWidth   = trailWidth * 0.2f;
        trail.material   = new(Shader.Find("Sprites/Default"));
        trail.colorGradient = BuildGradient(trailColor, tailColor);
        trail.sortingLayerName = sortLayer;
        trail.sortingOrder     = sortOrder - 1;

        // Snake head sprite
        if (sr != null)
        {
            Texture2D headTex = CreateSnakeHeadTexture(64, 64);
            sr.sprite = Sprite.Create(headTex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
            sr.color = Color.white;
        }
    }

    Gradient BuildGradient(Color body, Color tip)
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new(body, 0f),
                new(body, 0.72f),
                new(tip,  1f)
            },
            new GradientAlphaKey[] {
                new(1f, 0f),
                new(1f, 0.80f),
                new(0.85f, 1f)
            }
        );
        return g;
    }

    void UpdateBaseBody()
    {
        for (int i = 0; i < bodySegments; i++)
        {
            int idx = (histHead + i) % bodySegments;
            baseBody.SetPosition(i, posHistory[idx]);
        }
    }

    Texture2D CreateSnakeHeadTexture(int w, int h)
    {
        Color transparent  = new Color(0f, 0f, 0f, 0f);
        Color bodyGreen    = new Color(0.20f, 0.72f, 0.13f, 1f);
        Color darkGreen    = new Color(0.08f, 0.35f, 0.05f, 1f);
        Color scaleGreen   = new Color(0.13f, 0.52f, 0.09f, 1f);
        Color eyeWhite     = new Color(0.95f, 0.95f, 0.85f, 1f);
        Color pupilBlack   = new Color(0.05f, 0.05f, 0.05f, 1f);
        Color tongueRed    = new Color(0.85f, 0.10f, 0.10f, 1f);

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        // Fill transparent
        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;
        tex.SetPixels(pixels);

        // Head oval parameters – center slightly below middle, tongue above
        float cx = w * 0.5f;
        float cy = h * 0.40f;
        float rx = w * 0.42f;
        float ry = h * 0.42f;

        // 1. Dark green outline (slightly larger oval)
        float bx = rx + 2f, by = ry + 2f;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = (x - cx) / bx, dy = (y - cy) / by;
                if (dx * dx + dy * dy <= 1f) tex.SetPixel(x, y, darkGreen);
            }

        // 2. Body green fill + track which pixels are body
        bool[,] inBody = new bool[w, h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = (x - cx) / rx, dy = (y - cy) / ry;
                if (dx * dx + dy * dy <= 1f)
                {
                    tex.SetPixel(x, y, bodyGreen);
                    inBody[x, y] = true;
                }
            }

        // 3. Hexagonal scale pattern (darker dots)
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (!inBody[x, y]) continue;
                float s = 7f;
                float row = y / s;
                float col = x / s + ((int)row % 2) * 0.5f;
                float fx = col - Mathf.Floor(col) - 0.5f;
                float fy = row - Mathf.Floor(row) - 0.5f;
                if (fx * fx + fy * fy > 0.15f) tex.SetPixel(x, y, scaleGreen);
            }

        // 4. Forked tongue at top (nose direction = local Y+)
        int tongueBaseY = Mathf.Min(h - 1, (int)(cy + ry) + 1);
        int tongueX     = (int)cx;
        int remaining   = h - tongueBaseY;
        int stemLen     = Mathf.Max(2, remaining / 2);

        for (int i = 0; i < stemLen && tongueBaseY + i < h; i++)
            tex.SetPixel(tongueX, tongueBaseY + i, tongueRed);

        int forkStart = tongueBaseY + stemLen;
        for (int i = 0; forkStart + i < h; i++)
        {
            SetSafe(tex, tongueX - i - 1, forkStart + i, tongueRed);
            SetSafe(tex, tongueX + i + 1, forkStart + i, tongueRed);
        }

        // 5. Eyes (white circles, upper portion of head)
        int eyeY   = (int)(cy + ry * 0.38f);
        int eyeR   = Mathf.Max(3, w / 11);
        int leftX  = (int)(cx - rx * 0.45f);
        int rightX = (int)(cx + rx * 0.45f);

        DrawCircle(tex, leftX,  eyeY, eyeR, eyeWhite);
        DrawCircle(tex, rightX, eyeY, eyeR, eyeWhite);

        // 6. Vertical slit pupils
        int pR = Mathf.Max(1, eyeR / 2);
        for (int dy = -(eyeR - 1); dy <= eyeR - 1; dy++)
        {
            float t = (float)(eyeR - Mathf.Abs(dy)) / eyeR;
            int slitW = Mathf.Max(1, Mathf.RoundToInt(pR * 0.6f * t));
            for (int dx = -slitW; dx <= slitW; dx++)
            {
                SetSafe(tex, leftX  + dx, eyeY + dy, pupilBlack);
                SetSafe(tex, rightX + dx, eyeY + dy, pupilBlack);
            }
        }

        tex.Apply();
        return tex;
    }

    void DrawCircle(Texture2D tex, int cx, int cy, int r, Color c)
    {
        for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
            {
                float dx = x - cx, dy = y - cy;
                if (dx * dx + dy * dy <= r * r) SetSafe(tex, x, y, c);
            }
    }

    void SetSafe(Texture2D tex, int x, int y, Color c)
    {
        if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
            tex.SetPixel(x, y, c);
    }

    void Update()
    {
        Vector2 cur = transform.position;
        distAccum += Vector2.Distance(cur, prevPos);
        prevPos = cur;

        // Minden histSpacing távolságonként eltároljuk az aktuális pozíciót
        while (distAccum >= histSpacing)
        {
            distAccum -= histSpacing;
            histHead = (histHead + bodySegments - 1) % bodySegments;
            posHistory[histHead] = cur;
        }

        // Az első pont mindig a fej aktuális pozíciója
        posHistory[histHead] = cur;

        UpdateBaseBody();
    }

    void FixedUpdate()
    {
        Vector2 direction = Vector2.zero;

        if (Keyboard.current[upKey].isPressed)    direction.y += 1;
        if (Keyboard.current[downKey].isPressed)  direction.y -= 1;
        if (Keyboard.current[leftKey].isPressed)  direction.x -= 1;
        if (Keyboard.current[rightKey].isPressed) direction.x += 1;

        if (direction != Vector2.zero)
            rb.AddForce(direction.normalized * acceleration, ForceMode2D.Force);

        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        if (rb.linearVelocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.relativeVelocity.magnitude < wallHitMinSpeed) return;

        if (!isShaking)
            StartCoroutine(WallHitShake());

        SpawnWallHitParticles(col.contacts[0].point);
    }

    IEnumerator WallHitShake()
    {
        isShaking = true;
        Vector3 originalScale = transform.localScale;

        // Squish
        float squishDuration = 0.05f;
        float squishAmount = 0.72f;
        for (float t = 0f; t < squishDuration; t += Time.deltaTime)
        {
            float factor = Mathf.Lerp(1f, squishAmount, t / squishDuration);
            transform.localScale = originalScale * factor;
            yield return null;
        }

        // Spring back
        float releaseDuration = 0.10f;
        for (float t = 0f; t < releaseDuration; t += Time.deltaTime)
        {
            float factor = Mathf.Lerp(squishAmount, 1f, t / releaseDuration);
            transform.localScale = originalScale * factor;
            yield return null;
        }

        transform.localScale = originalScale;
        isShaking = false;
    }

    void SpawnWallHitParticles(Vector2 position)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color particleColor = sr != null ? sr.color : Color.white;

        GameObject obj = new GameObject("WallHitParticles");
        obj.transform.position = position;

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.startColor      = particleColor;
        main.startSpeed      = new ParticleSystem.MinMaxCurve(1f, 3f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.03f, 0.09f);
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.duration        = 0.1f;
        main.loop            = false;
        main.gravityModifier = 0.15f;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius    = 0.03f;

        var psr = ps.GetComponent<ParticleSystemRenderer>();
        psr.material     = new Material(Shader.Find("Sprites/Default"));
        psr.sortingOrder = 10;

        ps.Play();
        Destroy(obj, 2f);
    }

}