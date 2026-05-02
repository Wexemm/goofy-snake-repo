using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Edit Mode tesztek a PlayerHealth komponenshez.
/// Teszteli az életerő mechanikát (sebzés, gyógyítás, normalizált érték).
/// Megjegyzés: halált kiváltó sebzés nem kerül tesztelésre Edit Mode-ban,
/// mert a DeathSequence coroutine scene-töltést kezdeményez.
/// </summary>
public class PlayerHealthTests
{
    private GameObject go;
    private PlayerHealth health;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("TestPlayer");
        health = go.AddComponent<PlayerHealth>();
        // Az Awake() automatikusan lefut: CurrentHealth = maxHealth (100)
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
    }

    // ── Kezdeti állapot ──────────────────────────────────────────────────────

    [Test]
    public void InitialHealth_IsMaxHealth()
    {
        // Komponens hozzáadásakor az életerő az maximum értéken kell legyen
        Assert.AreEqual(health.maxHealth, health.CurrentHealth, 0.001f,
            "Kezdeti életerő nem egyezik a maximummal.");
    }

    [Test]
    public void InitialNormalizedHealth_IsOne()
    {
        // Normalizált életerő kezdetben 1.0 kell legyen
        Assert.AreEqual(1f, health.NormalizedHealth, 0.001f,
            "Kezdeti normalizált életerő nem 1.");
    }

    // ── TakeDamage ───────────────────────────────────────────────────────────

    [Test]
    public void TakeDamage_ReducesCurrentHealth()
    {
        health.TakeDamage(30f);

        Assert.AreEqual(70f, health.CurrentHealth, 0.001f,
            "30 sebzés után az életerőnek 70-nek kell lennie.");
    }

    [Test]
    public void TakeDamage_UpdatesNormalizedHealth()
    {
        health.TakeDamage(50f);

        Assert.AreEqual(0.5f, health.NormalizedHealth, 0.001f,
            "50%-os sebzés után a normalizált életerőnek 0.5-nek kell lennie.");
    }

    [Test]
    public void TakeDamage_FiresOnHealthChangedEvent()
    {
        float receivedValue = -1f;
        health.OnHealthChanged += v => receivedValue = v;

        health.TakeDamage(20f);

        Assert.AreEqual(health.NormalizedHealth, receivedValue, 0.001f,
            "Az OnHealthChanged event nem sült el, vagy helytelen értéket adott.");
    }

    [Test]
    public void TakeDamage_ZeroAmount_DoesNothing()
    {
        health.TakeDamage(0f);

        Assert.AreEqual(100f, health.CurrentHealth, 0.001f,
            "0 sebzés nem változtathatja meg az életerőt.");
    }

    [Test]
    public void TakeDamage_NegativeAmount_DoesNothing()
    {
        health.TakeDamage(-10f);

        Assert.AreEqual(100f, health.CurrentHealth, 0.001f,
            "Negatív sebzés nem csökkentheti az életerőt.");
    }

    // ── Heal ─────────────────────────────────────────────────────────────────

    [Test]
    public void Heal_IncreasesCurrentHealth()
    {
        health.TakeDamage(40f); // 60 HP marad
        health.Heal(20f);       // 80 HP-ra gyógyul

        Assert.AreEqual(80f, health.CurrentHealth, 0.001f,
            "Gyógyítás után az életerőnek 80-nak kell lennie.");
    }

    [Test]
    public void Heal_DoesNotExceedMaxHealth()
    {
        health.Heal(9999f);

        Assert.AreEqual(health.maxHealth, health.CurrentHealth, 0.001f,
            "Gyógyítás nem emelheti az életerőt a maximum fölé.");
    }

    [Test]
    public void Heal_ZeroAmount_DoesNothing()
    {
        health.TakeDamage(20f);
        float before = health.CurrentHealth;
        health.Heal(0f);

        Assert.AreEqual(before, health.CurrentHealth, 0.001f,
            "0 gyógyítás nem változtathatja az életerőt.");
    }
}
