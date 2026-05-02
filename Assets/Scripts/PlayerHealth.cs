using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Health component for player prefabs.
/// Integrates with the enemy damage system and reuses the same
/// explosion + level-restart death flow as <see cref="ColoredArea"/>.
///
/// Add this component to each player prefab.
/// Subscribe to <see cref="OnHealthChanged"/> to drive a health bar UI.
/// Subscribe to <see cref="OnDeath"/> for any additional death reactions.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    private float _currentHealth;
    private bool _healthInitialized;

    public float CurrentHealth
    {
        get
        {
            if (!_healthInitialized) { _currentHealth = maxHealth; _healthInitialized = true; }
            return _currentHealth;
        }
        private set { _healthInitialized = true; _currentHealth = value; }
    }
    public float NormalizedHealth  => CurrentHealth / maxHealth;

    /// <summary>Fires whenever health changes. Argument is normalised health (0–1).</summary>
    public event Action<float> OnHealthChanged;

    /// <summary>Fires once when the player reaches 0 HP, before the death sequence.</summary>
    public event Action OnDeath;

    bool dead;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (dead || amount <= 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(NormalizedHealth);

        if (CurrentHealth <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (dead || amount <= 0f) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(NormalizedHealth);
    }

    // ── Death ─────────────────────────────────────────────────────────────────

    void Die()
    {
        dead = true;
        OnDeath?.Invoke();
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Freeze the player in place.
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Play an explosion matching the player's colour.
        SpriteRenderer sr    = GetComponent<SpriteRenderer>();
        Color explosionColor = sr != null ? sr.color : Color.white;
        SpawnExplosion(transform.position, explosionColor);

        if (sr != null) sr.enabled = false;

        yield return new WaitForSeconds(2f);

        // Restart the current level — mirrors ColoredArea behaviour.
        int currentLevel = PlayerPrefs.GetInt("currentLevel", 0);
        if (LevelManager.instance != null)
            LevelManager.instance.LoadLevel(currentLevel);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Identical to the explosion in ColoredArea so both death sources look consistent.
    void SpawnExplosion(Vector3 position, Color color)
    {
        GameObject obj = new("Explosion");
        obj.transform.position = position;

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.startColor      = color;
        main.startSpeed      = new ParticleSystem.MinMaxCurve(3f, 8f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        main.duration        = 0.2f;
        main.loop            = false;
        main.gravityModifier = 0.3f;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 50) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius    = 0.05f;

        var psr = ps.GetComponent<ParticleSystemRenderer>();
        psr.material     = new Material(Shader.Find("Sprites/Default"));
        psr.sortingOrder = 10;

        ps.Play();
        Destroy(obj, 3f);
    }
}
