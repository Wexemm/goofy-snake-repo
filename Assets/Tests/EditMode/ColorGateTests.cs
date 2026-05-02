using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Edit Mode tesztek a ColorGate komponenshez.
/// Teszteli a kapu aktiváció-logikát: AddActivation/RemoveActivation,
/// és a Collider2D állapotát (zárva = collider BE, nyitva = collider KI).
/// </summary>
public class ColorGateTests
{
    private GameObject go;
    private ColorGate gate;
    private Collider2D col;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("TestGate");
        // Collider2D szükséges, hogy a ColorGate.Awake() megtalálja
        col = go.AddComponent<BoxCollider2D>();
        gate = go.AddComponent<ColorGate>();
        // Awake + Start lefut: aktiváció = 0, kapu zárva, collider engedélyezve
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(go);
    }

    // ── Kezdeti állapot ──────────────────────────────────────────────────────

    [Test]
    public void Gate_StartsLocked_ColliderEnabled()
    {
        // Alapállapotban a kapu zárva → az ütköző aktív (fizikai blokk)
        Assert.IsTrue(col.enabled,
            "Új kapu esetén a Collider2D-nek engedélyezve kell lennie (zárva).");
    }

    // ── AddActivation ────────────────────────────────────────────────────────

    [Test]
    public void AddActivation_OpensGate_ColliderDisabled()
    {
        gate.AddActivation();

        Assert.IsFalse(col.enabled,
            "Egy aktiváció után a kapu nyitva kell legyen (Collider2D kikapcsolva).");
    }

    [Test]
    public void MultipleAddActivations_GateRemainsOpen()
    {
        gate.AddActivation();
        gate.AddActivation();

        Assert.IsFalse(col.enabled,
            "Több aktiváció esetén a kapu nyitva kell maradjon.");
    }

    // ── RemoveActivation ─────────────────────────────────────────────────────

    [Test]
    public void RemoveActivation_AfterOne_ClosesGate()
    {
        gate.AddActivation();
        gate.RemoveActivation();

        Assert.IsTrue(col.enabled,
            "Az aktiváció eltávolítása után a kapu zárva kell legyen (Collider2D engedélyezve).");
    }

    [Test]
    public void RemoveActivation_WithTwoActivations_GateStaysOpen()
    {
        gate.AddActivation();
        gate.AddActivation();
        gate.RemoveActivation(); // még 1 aktiváció maradt

        Assert.IsFalse(col.enabled,
            "Ha maradt aktiváció, a kapu nyitva kell maradjon.");
    }

    [Test]
    public void RemoveActivation_BelowZero_DoesNotNegate()
    {
        // RemoveActivation nem mehet 0 alá – a kapu zárva marad, nem nyílik ki
        gate.RemoveActivation(); // aktiváció már 0 volt

        Assert.IsTrue(col.enabled,
            "Üres állapotban RemoveActivation nem okozhat hibás kapu-nyitást.");
    }

    // ── Teljes ciklus ────────────────────────────────────────────────────────

    [Test]
    public void FullCycle_OpenThenClose_WorksCorrectly()
    {
        gate.AddActivation();
        Assert.IsFalse(col.enabled, "Nyitás után collider ki kell kapcsolódjon.");

        gate.RemoveActivation();
        Assert.IsTrue(col.enabled, "Zárás után collider be kell kapcsolódjon.");
    }
}
