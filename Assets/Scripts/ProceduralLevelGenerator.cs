using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Véletlenszerű labirintus-pályát generál rekurzív visszalépéses (Recursive Backtracking)
/// algoritmussal. A pálya minden cellájából el lehet jutni minden más cellába,
/// ami garantálja, hogy a játékosok mindig elérhetik a kijáratot.
/// </summary>
public class ProceduralLevelGenerator : MonoBehaviour
{
    [Header("Pálya méret (páratlan értékek ajánlottak, pl. 21)")]
    public int width  = 21;
    public int height = 21;

    [Header("Ellenségek száma")]
    public int enemyCount = 3;

    [Header("Seed (0 = minden körben más véletlen pálya)")]
    public int fixedSeed = 0;

    /// <summary>Az utoljára használt seed — a UI ezt mutatja és menti el.</summary>
    public int LastUsedSeed { get; private set; }

    private char[,] grid;
    private System.Random rng;

    /// <summary>
    /// Generál egy ASCII térképet és visszaadja sztringként,
    /// amit a LevelGenerator képes feldolgozni.
    /// </summary>
    public string Generate()
    {
        // A maze algoritmus páratlan méreteket igényel
        int w = (width  % 2 == 0) ? width  + 1 : width;
        int h = (height % 2 == 0) ? height + 1 : height;

        // Biztonsági minimum
        if (w < 5) w = 5;
        if (h < 5) h = 5;

        int seed;
        if (fixedSeed != 0)
        {
            seed = fixedSeed;
        }
        else
        {
            // Ha a UI "Betöltés" gombot nyomott, PlayerPrefs-ben van egy kért seed
            int forced = PlayerPrefs.GetInt("ProceduralSeed", 0);
            seed = (forced != 0) ? forced : Random.Range(1, 999999);
        }

        LastUsedSeed = seed;
        // Eltároljuk, hogy a UI megmutathassa és el lehessen menteni
        PlayerPrefs.SetInt("ProceduralSeed", seed);
        PlayerPrefs.Save();

        rng = new System.Random(seed);

        Debug.Log($"[ProceduralLevelGenerator] Seed={seed}  Méret={w}x{h}  Ellenségek={enemyCount}");

        grid = new char[h, w];

        // 1. lépés: mindent fallal töltünk
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                grid[y, x] = '#';

        // 2. lépés: labirintus kivájása
        CarvePassages(1, 1);

        // 3. lépés: játékosok + kijárat elhelyezése
        PlaceSpecialTiles(w, h);

        // 4. lépés: ellenségek elhelyezése
        PlaceEnemies(w, h);

        return GridToString(w, h);
    }

    // ─── Rekurzív visszalépéses labirintus-algoritmus ───────────────────────

    void CarvePassages(int cx, int cy)
    {
        // 4 irány véletlenszerű sorrendben
        int[] dirs = { 0, 1, 2, 3 };
        Shuffle(dirs);

        foreach (int d in dirs)
        {
            int nx, ny, mx, my;
            switch (d)
            {
                case 0: nx = cx;     ny = cy - 2; mx = cx;     my = cy - 1; break; // fel
                case 1: nx = cx;     ny = cy + 2; mx = cx;     my = cy + 1; break; // le
                case 2: nx = cx + 2; ny = cy;     mx = cx + 1; my = cy;     break; // jobbra
                default: nx = cx - 2; ny = cy;    mx = cx - 1; my = cy;     break; // balra
            }

            // Csak akkor haladunk, ha a célcella a határokon belül van és még fal
            if (nx > 0 && nx < grid.GetLength(1) - 1 &&
                ny > 0 && ny < grid.GetLength(0) - 1 &&
                grid[ny, nx] == '#')
            {
                grid[cy, cx] = '.';  // aktuális cella
                grid[my, mx] = '.';  // közbülső fal (folyosó)
                grid[ny, nx] = '.';  // következő cella
                CarvePassages(nx, ny);
            }
        }

        // Biztonsági fallback: a kiindulópontot is padlóvá tesszük
        grid[cy, cx] = '.';
    }

    // ─── Speciális cellák elhelyezése ───────────────────────────────────────

    void PlaceSpecialTiles(int w, int h)
    {
        // Játékos 1: bal felső sarok
        SetCell(1,     1,     '1');

        // Játékos 2: jobb felső sarok
        SetCell(w - 2, 1,     '2');

        // Kijárat: jobb alsó sarok
        SetCell(w - 2, h - 2, 'E');
    }

    // Páratlan cellakoordinátára clampolva beállít egy karaktert
    void SetCell(int x, int y, char c)
    {
        // Biztosítjuk, hogy páratlan indexen landen (valós maze-cella)
        int px = (x % 2 == 0) ? x - 1 : x;
        int py = (y % 2 == 0) ? y - 1 : y;
        px = Mathf.Clamp(px, 1, grid.GetLength(1) - 2);
        py = Mathf.Clamp(py, 1, grid.GetLength(0) - 2);
        grid[py, px] = c;
    }

    // ─── Ellenségek elhelyezése ──────────────────────────────────────────────

    void PlaceEnemies(int w, int h)
    {
        // Gyűjtsük össze a szabad padlócellákat (nem saroktól)
        var freeCells = new List<(int x, int y)>();
        for (int y = 3; y < h - 3; y++)
            for (int x = 3; x < w - 3; x++)
                if (grid[y, x] == '.')
                    freeCells.Add((x, y));

        int count = Mathf.Min(enemyCount, freeCells.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = rng.Next(freeCells.Count);
            var (ex, ey) = freeCells[idx];
            grid[ey, ex] = 'e';
            freeCells.RemoveAt(idx);
        }
    }

    // ─── Segédfüggvények ─────────────────────────────────────────────────────

    void Shuffle(int[] arr)
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }

    string GridToString(int w, int h)
    {
        var sb = new StringBuilder();
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
                sb.Append(grid[y, x]);
            if (y < h - 1)
                sb.Append('\n');
        }
        return sb.ToString();
    }
}
