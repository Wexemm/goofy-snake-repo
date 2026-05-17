using UnityEngine;
using UnityEngine.UIElements;

public class LevelGenerator : MonoBehaviour
{
    public LevelConfig config;
    public LevelData[] levels;

    [Header("Cella méret")]
    public float cellSize = 0.5f;

    [Header("Kamera")]
    public float cameraPadding = 0.5f;

    [Header("Procedurális pálya generálás")]
    [Tooltip("Ha be van kapcsolva, véletlenszerű labirintus-pálya generálódik a kézzel készített pályák helyett.")]
    public bool useProceduralGeneration = false;
    public ProceduralLevelGenerator proceduralGenerator;

    [Tooltip("In-game UI (Súj pálya / Mentés / Betöltés gombok) megjelenjen-e procedurális módban.")]
    public bool showProceduralUI = true;
    [Tooltip("Assets/UI/PanelSettings.asset — húzd be ide.")]
    public PanelSettings panelSettings;

    void Start()
    {
        // A főmenü gombjától érkező mód felülírja az Inspector beállítást
        // GameMode 1 = Labyrinth (procedurális), 0 = normál, -1 = nem lett beállítva (Inspector dönt)
        int gameMode = PlayerPrefs.GetInt("GameMode", -1);
        if      (gameMode == 1) useProceduralGeneration = true;
        else if (gameMode == 0) useProceduralGeneration = false;

        if (LevelManager.instance != null)
            LevelManager.instance.totalLevels = useProceduralGeneration ? int.MaxValue : levels.Length;

        GenerateLevel();

        if (useProceduralGeneration && showProceduralUI)
        {
            var ui = gameObject.AddComponent<ProceduralMapUI>();
            ui.panelSettings = panelSettings;
        }
    }

    void GenerateLevel()
    {
        if (config == null) { Debug.LogError("LevelGenerator: Config nincs behúzva az Inspectorba!", this); return; }

        string[] rows;
        LevelData levelData = null;

        if (useProceduralGeneration)
        {
            // Automatikus pályagenerálás
            if (proceduralGenerator == null)
                proceduralGenerator = GetComponent<ProceduralLevelGenerator>()
                                      ?? gameObject.AddComponent<ProceduralLevelGenerator>();

            string map = proceduralGenerator.Generate();
            rows = System.Array.FindAll(
                map.Replace("\r", "").Split('\n'),
                r => r.Length > 0);
        }
        else
        {
            if (levels == null || levels.Length == 0) { Debug.LogError("LevelGenerator: Levels tömb üres!", this); return; }

            int idx = Mathf.Clamp(PlayerPrefs.GetInt("currentLevel", 0), 0, levels.Length - 1);
            levelData = levels[idx];
            if (levelData == null) { Debug.LogError($"LevelGenerator: levels[{idx}] nincs behúzva!", this); return; }
            rows = levelData.GetRows();
        }

        int cols = rows[0].Length;

        // Középre igazított offset (center-pivot sprite-okhoz)
        float offsetX = -(cols - 1) * cellSize / 2f;
        float offsetY =  (rows.Length - 1) * cellSize / 2f;

        for (int y = 0; y < rows.Length; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                char tile = rows[y][x];
                Vector3 pos = new(offsetX + x * cellSize, offsetY - y * cellSize, 0);

                switch (tile)
                {
                    case '#':
                        Instantiate(config.wallPrefab, pos, Quaternion.identity);
                        break;
                    case '.':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        break;
                    case 'R':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        Instantiate(config.redAreaPrefab, pos, Quaternion.identity);
                        break;
                    case 'B':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        Instantiate(config.blueAreaPrefab, pos, Quaternion.identity);
                        break;
                    case 'r':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        Instantiate(config.redButtonPrefab, pos, Quaternion.identity);
                        break;
                    case 'b':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        Instantiate(config.blueButtonPrefab, pos, Quaternion.identity);
                        break;
                    case 'E':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        Instantiate(config.exitPrefab, pos, Quaternion.identity);
                        break;
                    case '1':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        SetSortingOrder(Instantiate(config.player1Prefab, pos, Quaternion.identity), 2);
                        break;
                    case '2':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        SetSortingOrder(Instantiate(config.player2Prefab, pos, Quaternion.identity), 2);
                        break;

                    // --- Sárga ---
                    case 'y':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        if (config.yellowButtonPrefab) Instantiate(config.yellowButtonPrefab, pos, Quaternion.identity);
                        break;
                    case 'Y':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        if (config.yellowLeverPrefab) Instantiate(config.yellowLeverPrefab, pos, Quaternion.identity);
                        break;
                    case '3':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        if (config.yellowGatePrefab) Instantiate(config.yellowGatePrefab, pos, Quaternion.identity);
                        break;

                    // --- Magenta ---
                    case 'p':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        if (config.magentaButtonPrefab) Instantiate(config.magentaButtonPrefab, pos, Quaternion.identity);
                        break;
                    case 'P':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        if (config.magentaLeverPrefab) Instantiate(config.magentaLeverPrefab, pos, Quaternion.identity);
                        break;
                    case '4':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        if (config.magentaGatePrefab) Instantiate(config.magentaGatePrefab, pos, Quaternion.identity);
                        break;

                    // --- Cián ---
                    case 'c':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        if (config.cyanButtonPrefab) Instantiate(config.cyanButtonPrefab, pos, Quaternion.identity);
                        break;
                    case 'C':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        if (config.cyanLeverPrefab) Instantiate(config.cyanLeverPrefab, pos, Quaternion.identity);
                        break;
                    case '5':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        if (config.cyanGatePrefab) Instantiate(config.cyanGatePrefab, pos, Quaternion.identity);
                        break;

                    // --- Ellenségek (Enemies) ---
                    case 'e':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        if (config.chaserEnemyPrefab) SetSortingOrder(Instantiate(config.chaserEnemyPrefab, pos, Quaternion.identity), 2);
                        break;
                    case 'q':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        if (config.redEnemyPrefab) SetSortingOrder(Instantiate(config.redEnemyPrefab, pos, Quaternion.identity), 2);
                        break;
                    case 'u':
                        Instantiate(config.floorPrefab, pos, Quaternion.identity);
                        if (config.blueEnemyPrefab) SetSortingOrder(Instantiate(config.blueEnemyPrefab, pos, Quaternion.identity), 2);
                        break;
                }
            }
        }

        if (levelData != null)
            SpawnEnemiesFromList(levelData, offsetX, offsetY);

        FitCamera(rows.Length, cols);
        FillOutsideWithWalls(rows.Length, cols);
    }

    void SpawnEnemiesFromList(LevelData levelData, float offsetX, float offsetY)
    {
        if (levelData.enemySpawns == null) return;

        foreach (EnemySpawnData spawn in levelData.enemySpawns)
        {
            Vector3 pos = new(offsetX + spawn.gridPosition.x * cellSize,
                              offsetY - spawn.gridPosition.y * cellSize, 0);

            GameObject prefab = spawn.type switch
            {
                EnemySpawnData.EnemyType.Chaser => config.chaserEnemyPrefab,
                EnemySpawnData.EnemyType.Red    => config.redEnemyPrefab,
                EnemySpawnData.EnemyType.Blue   => config.blueEnemyPrefab,
                _                               => null
            };

            if (prefab != null)
                SetSortingOrder(Instantiate(prefab, pos, Quaternion.identity), 2);
        }
    }

    void FitCamera(int rowCount, int colCount)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float levelWidth  = colCount * cellSize;
        float levelHeight = rowCount * cellSize;

        cam.transform.position = new Vector3(0, 0, cam.transform.position.z);

        float vertSize  = levelHeight / 2f + cameraPadding;
        float horizSize = levelWidth  / 2f / cam.aspect + cameraPadding;
        cam.orthographicSize = Mathf.Max(vertSize, horizSize);
    }

    void FillOutsideWithWalls(int rowCount, int colCount)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float camHalfW = cam.orthographicSize * cam.aspect;
        float camHalfH = cam.orthographicSize;

        float originX = -(colCount - 1) * cellSize / 2f;
        float originY =  (rowCount - 1) * cellSize / 2f;

        int xMin = Mathf.FloorToInt((-camHalfW - originX) / cellSize) - 3;
        int xMax = Mathf.CeilToInt ((  camHalfW - originX) / cellSize) + 3;
        int yMin = Mathf.FloorToInt(( originY - camHalfH) / cellSize) - 3;
        int yMax = Mathf.CeilToInt ((  originY + camHalfH) / cellSize) + 3;

        for (int iy = yMin; iy <= yMax; iy++)
        {
            for (int ix = xMin; ix <= xMax; ix++)
            {
                if (ix >= 0 && ix < colCount && iy >= 0 && iy < rowCount)
                    continue;

                Vector3 pos = new(originX + ix * cellSize, originY - iy * cellSize, 0);
                Instantiate(config.wallPrefab, pos, Quaternion.identity);
            }
        }
    }

    void SetSortingOrder(GameObject obj, int order)
    {
        var sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = order;
    }
}
