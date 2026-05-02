# Goofy Snake 

Kétjátékos, kooperatív puzzle-platform játék Unity 6-ban (URP, 2D).  
Két kígyót irányítasz egyszerre – mindkét játékosnak el kell érnie a célzónát a pálya megoldásához.

---

## Tartalomjegyzék

1. [Projekt struktúra](#projekt-struktúra)
2. [Futtatás és buildelés](#futtatás-és-buildelés)
3. [Pályák (Levels) – fájlszintű elérés](#pályák-levels--fájlszintű-elérés)
4. [Irányítás](#irányítás)
5. [Pályaelemek és logika](#pályaelemek-és-logika)
6. [Tesztek](#tesztek)
7. [Függőségek](#függőségek)

---

## Projekt struktúra

```
Assets/
├── Levels/          # ScriptableObject pályaadatok (LevelData1–LevelData6)
├── Prefab/          # Játékobjektum prefabok (falak, padló, játékosok, ellenségek stb.)
├── Scenes/          # Unity jelenetek (SplashScreen, MainMenu, Game, EndScreen)
├── Scripts/         # C# játéklogika
│   ├── GameManager.cs        – győzelem / szintváltás kezelése
│   ├── LevelGenerator.cs     – pálya betöltése LevelData-ból
│   ├── LevelData.cs          – pályatérkép ScriptableObject
│   ├── LevelConfig.cs        – prefab-hivatkozások konfigurációja
│   ├── PlayerMovement.cs     – kígyó mozgás, trail, kígyótest
│   ├── PlayerHealth.cs       – életerő, sebzés, halál
│   ├── ColorGate.cs          – kapu nyitás/zárás aktiváció alapján
│   ├── ColorButton.cs        – nyomógomb, amíg rajta állnak nyitva tartja a kaput
│   ├── ColorLever.cs         – kar, felváltva nyit/zár
│   ├── ColoredArea.cs        – színes terület, helytelen játékos = halál
│   ├── ExitZone.cs           – győzelmi zóna, mindkét játékos szükséges
│   ├── Enemies/              – ellenség AI (Chaser, Red, Blue)
│   ├── Menu/                 – főmenü, szintválasztó, LevelManager, EndScreen
│   └── Pathfinding/          – A* útkereső az ellenségekhez
├── Tests/
│   └── EditMode/             # Edit Mode unit tesztek (legalább 3 db)
└── LevelConfig.asset        # Aktív pálya-konfiguráció (prefab hozzárendelések)
```

---

## Futtatás és buildelés

### Előkészített Windows build

A `Build/` mappában egy előre lefordított Windows x64 futtatható verzió található:

```
Build/Goofy Snake.exe
```

Dupla kattintással indítható, Unity telepítése **nem szükséges**.

### Buildelés Unity-ből

1. Nyisd meg a projektet **Unity 6** (6000.0.x) vagy újabb verziójával.
2. `File → Build Profiles` → Windows, Mac, Linux → **Build**.
3. A projekt minden szükséges scene-t tartalmaz (`SplashScreen`, `MainMenu`, `Game`, `EndScreen`).

---

## Pályák (Levels) – fájlszintű elérés

### Tárolási forma

A pályák **Unity ScriptableObject** formátumban vannak eltárolva, szöveg-alapú ASCII-térképpel:

```
Assets/Levels/
├── LevelData1.asset   # 1. pálya
├── LevelData2.asset   # 2. pálya
├── LevelData3.asset   # 3. pálya
├── LevelData4.asset   # 4. pálya
├── LevelData5.asset   # 5. pálya
└── LevelData6.asset   # 6. pálya
```

Minden `.asset` fájl egy `LevelData` ScriptableObject, amelynek `map` mezője egy szöveg-alapú ASCII-térkép:

| Karakter | Jelentés |
|----------|----------|
| `#` | Fal |
| `.` | Padló |
| `1` | 1. játékos kezdőpozíciója |
| `2` | 2. játékos kezdőpozíciója |
| `E` | Kilépési zóna (cél) |
| `R` | Piros terület (kék játékos meghal rajta) |
| `B` | Kék terület (piros játékos meghal rajta) |
| `r` | Piros nyomógomb |
| `b` | Kék nyomógomb |
| `y` | Sárga nyomógomb |
| `Y` | Sárga kar (lever) |
| `3` | Sárga kapu |
| `p` | Lila nyomógomb |
| `P` | Lila kar |
| `4` | Lila kapu |
| `c` | Cián nyomógomb |
| `C` | Cián kar |
| `5` | Cián kapu |
| `e` | Chaser ellenség |
| `q` | Piros ellenség |
| `u` | Kék ellenség |

### Pálya elérése a kódból

```csharp
// LevelData.GetRows() visszaadja a sorok tömbjét
LevelData data = /* Inspector vagy Resources.Load<LevelData>(...) */;
string[] rows = data.GetRows();
// rows[y][x] == '#' → fal, stb.
```

A `LevelGenerator.cs` runtime-ban állítja elő a pályát a `LevelConfig.asset` prefab-hivatkozásai és az aktuális pálya (`PlayerPrefs["currentLevel"]`) alapján.

### Példa pályatérkép (LevelData1)

Az `Assets/Levels/LevelData1.asset` Unity Inspectorban megnyitva vagy text editorban megtekinthető. A `.asset` fájl YAML-alapú szöveg, a `map` mező tartalmazza az ASCII-térképet.

---

## Irányítás

| Játékos | Fel | Le | Bal | Jobb |
|---------|-----|----|-----|------|
| **1. játékos** | `W` | `S` | `A` | `D` |
| **2. játékos** | `↑` | `↓` | `←` | `→` |

---

## Pályaelemek és logika

- **Kígyó mozgás**: fizika alapú (Rigidbody2D), gyorsulással és lassítással. A kígyótest pozíció-history alapján rajzolódik (LineRenderer).
- **Kapuk**: `ColorGate` – a megfelelő szín gombja/karja nyitja meg. Több aktiváció lehetséges (pl. két gomb egy kapuhoz).
- **Nyomógomb vs. kar**: A gomb (`ColorButton`) csak addig nyit, amíg valaki rajta áll. A kar (`ColorLever`) állapotot vált.
- **Ellenségek**: A* pathfinding alapján követik a játékosokat.
- **Győzelem**: Mindkét játékosnak egyszerre kell az `ExitZone`-ban lennie.

---

## Tesztek

A tesztek az `Assets/Tests/EditMode/` mappában találhatók, Unity Edit Mode tesztekként futtathatók:

**Window → General → Test Runner → EditMode**

| Teszt osztály | Mit tesztel |
|--------------|-------------|
| `LevelDataTests` | `LevelData.GetRows()` – sorok helyes feldolgozása, üres sorok kiszűrése, Windows sortörés kezelése |
| `PlayerHealthTests` | `PlayerHealth.TakeDamage()`, `Heal()`, `NormalizedHealth` – életerő mechanika |
| `ColorGateTests` | `ColorGate.AddActivation()`, `RemoveActivation()` – kapu nyitás/zárás logika és ütköző állapot |

---

## Függőségek

- **Unity 6** (6000.0.x+), Universal Render Pipeline (URP)
- `com.unity.inputsystem` 1.19.0 – új Input System
- `com.unity.test-framework` 1.6.0 – unit tesztek
- `com.unity.2d.tilemap` – 2D tilemap (padló, fal renderelés)
- `com.unity.postprocessing` 3.5.4 – vizuális effektek
