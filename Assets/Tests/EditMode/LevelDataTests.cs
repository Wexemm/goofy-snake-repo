using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Edit Mode tesztek a LevelData.GetRows() metódushoz.
/// Teszteli az architektúra pályaadatokat kezelő alapegységét.
/// </summary>
public class LevelDataTests
{
    // ── Segédmetódus ────────────────────────────────────────────────────────

    private LevelData CreateLevelData(string map)
    {
        var data = ScriptableObject.CreateInstance<LevelData>();
        data.map = map;
        return data;
    }

    // ── Tesztek ─────────────────────────────────────────────────────────────

    [Test]
    public void GetRows_ReturnsCorrectRowCount()
    {
        // 3 soros pálya → 3 sort kell visszaadnia
        var data = CreateLevelData("###\n...\n###");

        string[] rows = data.GetRows();

        Assert.AreEqual(3, rows.Length, "GetRows() nem a várt számú sort adja vissza.");
        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetRows_ReturnsCorrectContent()
    {
        // A sorok tartalma pontosan megfelel a térkép karaktereinek
        var data = CreateLevelData("#.#\n.1.\n#E#");

        string[] rows = data.GetRows();

        Assert.AreEqual("#.#", rows[0], "Az első sor tartalma helytelen.");
        Assert.AreEqual(".1.", rows[1], "A második sor tartalma helytelen.");
        Assert.AreEqual("#E#", rows[2], "A harmadik sor tartalma helytelen.");
        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetRows_IgnoresEmptyLines()
    {
        // Üres sorok (pl. záró sortörés) nem kerülnek bele az eredménybe
        var data = CreateLevelData("###\n...\n\n");

        string[] rows = data.GetRows();

        Assert.AreEqual(2, rows.Length, "Az üres sorokat ki kell szűrni.");
        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetRows_HandlesWindowsLineEndings()
    {
        // Windows-stílusú sortörések (\r\n) nem okoznak hibás karaktereket a sorok végén
        var data = CreateLevelData("###\r\n...\r\n###");

        string[] rows = data.GetRows();

        Assert.AreEqual(3, rows.Length, "\\r\\n sortörések esetén is 3 sort kell visszaadni.");
        Assert.AreEqual("###", rows[0], "Az első sor ne tartalmazzon \\r karaktert.");
        Assert.AreEqual("###", rows[2], "Az utolsó sor ne tartalmazzon \\r karaktert.");
        Object.DestroyImmediate(data);
    }

    [Test]
    public void GetRows_SingleRowMap()
    {
        // Egysoros térkép is helyes eredményt ad
        var data = CreateLevelData("#1E#");

        string[] rows = data.GetRows();

        Assert.AreEqual(1, rows.Length, "Egysoros térkép esetén 1 sort kell visszaadni.");
        Assert.AreEqual("#1E#", rows[0]);
        Object.DestroyImmediate(data);
    }
}
