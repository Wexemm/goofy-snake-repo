# Tesztek dokumentációja – Goofy Snake

## Áttekintés

A projekt **Unity Edit Mode** teszteket használ, amelyeket a **NUnit** keretrendszer hajt végre (ez Unity beépített tesztkönyvtára). Az összes teszt a `Assets/Tests/EditMode/` mappában található, és a Unity **Test Runner** ablakából futtatható.

**Edit Mode** azt jelenti, hogy a tesztek futtatásához **nem kell elindítani a játékot** (nem Play Mode). A tesztek egyszerű C# osztályokat, komponenseket és ScriptableObject-eket hoznak létre, ellenőrzik a viselkedésüket, majd törlik őket.

---
  
## Tesztelt rendszerek

### 1. `PlayerHealthTests.cs` – Játékos életereje

**Mit tesztel?**  
A `PlayerHealth` komponenst, ami a játékos HP-ját kezeli (sebzés, gyógyítás, normalizált érték, esemény kiváltás).

**Hogyan épül fel a teszt osztály?**

- **`[SetUp]`** – Minden teszt előtt létrehoz egy új `GameObject`-et, rárakja a `PlayerHealth` komponenst. Az `Awake()` automatikusan lefut, tehát az életerő máris `maxHealth` értéken van (100 HP).
- **`[TearDown]`** – Minden teszt után megsemmisíti a GameObject-et, hogy ne maradjanak "szemét" objektumok.
- **`[Test]`** – Maga a teszt metódus.

**Tesztek és magyarázatuk:**

| Teszt neve | Mit ellenőriz |
|---|---|
| `InitialHealth_IsMaxHealth` | Kezdetben az életerő pontosan a maximummal egyenlő (100). |
| `InitialNormalizedHealth_IsOne` | A normalizált életerő (0.0–1.0 skála) kezdetben 1.0. Ez az érték hajtja az életerő-sávot a UI-on. |
| `TakeDamage_ReducesCurrentHealth` | 30 sebzés után az életerő 70-re csökken. |
| `TakeDamage_UpdatesNormalizedHealth` | 50 sebzés után a normalizált érték 0.5 (50%). |
| `TakeDamage_FiresOnHealthChangedEvent` | A `TakeDamage()` hívás kiviszi az `OnHealthChanged` eseményt (event), és az helyes normalizált értéket közvetít. |
| `TakeDamage_ZeroAmount_DoesNothing` | 0 sebzés nem változtatja az életerőt. |
| `TakeDamage_NegativeAmount_DoesNothing` | Negatív sebzés nem csökkenti az életerőt (védelem hibás bemenet ellen). |
| `Heal_IncreasesCurrentHealth` | Sebzés után gyógyítás helyreállítja az életerőt a várt értékre. |
| `Heal_DoesNotExceedMaxHealth` | Gyógyítás nem emelheti az életerőt a maximum (100) fölé. |
| `Heal_ZeroAmount_DoesNothing` | 0 gyógyítás nem változtat semmit. |

> **Megjegyzés a halálról:** A halált okozó sebzés (`CurrentHealth <= 0`) szándékosan nincs Edit Mode-ban tesztelve, mert a `DeathSequence` coroutine scene-betöltést indít el, ami Edit Mode-ban nem működik.

---

### 2. `ColorGateTests.cs` – Színes kapu logika

**Mit tesztel?**  
A `ColorGate` komponenst, ami egy olyan kapu a pályán, amelyet gombokkal vagy karokkal lehet ki-/bekapcsolni (pl. egy kék gomb megnyom egy kék kaput). A kapu fizikailag `Collider2D`-vel van blokkolva, amit az aktivációk száma vezérel.

**Hogyan épül fel?**

- **`[SetUp]`** – Létrehoz egy `GameObject`-et `BoxCollider2D`-vel és `ColorGate` komponenssel.
- **`[TearDown]`** – Törli a létrehozott objektumot.

**A kapu logikája (amit a tesztek lefedenek):**
- `activationCount` egy belső számláló, hogy hány forrás (gomb/kar) aktiválja éppen a kaput.
- Ha `activationCount > 0` → kapu **nyitva** → `Collider2D` kikapcsolva (a játékos átmehet).
- Ha `activationCount == 0` → kapu **zárva** → `Collider2D` bekapcsolva (fizikai akadály).

**Tesztek és magyarázatuk:**

| Teszt neve | Mit ellenőriz |
|---|---|
| `Gate_StartsLocked_ColliderEnabled` | Új kapu esetén a Collider2D be van kapcsolva (zárva). |
| `AddActivation_OpensGate_ColliderDisabled` | Egy aktiváció után a Collider2D kikapcsol (kapu nyílik). |
| `MultipleAddActivations_GateRemainsOpen` | Több aktiváció esetén is nyitva marad a kapu. |
| `RemoveActivation_AfterOne_ClosesGate` | Ha az egyetlen aktivációt eltávolítjuk, a kapu bezárul. |
| `RemoveActivation_WithTwoActivations_GateStaysOpen` | Ha marad még aktiváció, a kapu nyitva marad. |
| `RemoveActivation_BelowZero_DoesNotNegate` | Ha 0-ról hívjuk a `RemoveActivation`-t, a számláló nem megy negatívba, a kapu zárva marad. |
| `FullCycle_OpenThenClose_WorksCorrectly` | Teljes nyitás-zárás ciklus helyesen működik. |

---

### 3. `LevelDataTests.cs` – Pályaadatok feldolgozása

**Mit tesztel?**  
A `LevelData` ScriptableObject `GetRows()` metódusát, ami a szöveges pályatérképet sorokra bontja. A pályák szövegként vannak tárolva (pl. `###\n...\n###`), ahol minden karakter a pálya egy celláját jelenti (`#` = fal, `.` = üres, `1` = játékos, `E` = kijárat stb.).

**Hogyan épül fel?**

Nincs `[SetUp]`/`[TearDown]` — minden teszt saját `LevelData` példányt hoz létre a `CreateLevelData()` segédmetódussal, majd a teszt végén maga törli.

**A `GetRows()` metódus működése:**
```csharp
return System.Array.FindAll(
    map.Replace("\r", "").Split('\n'),
    row => row.Length > 0
);
```
1. Eltávolítja a `\r` karaktereket (Windows sortörések kezelése).
2. `\n` mentén darabokra vágja a stringet.
3. Kiszűri az üres sorokat.

**Tesztek és magyarázatuk:**

| Teszt neve | Mit ellenőriz |
|---|---|
| `GetRows_ReturnsCorrectRowCount` | 3 soros térkép esetén pontosan 3 sort ad vissza. |
| `GetRows_ReturnsCorrectContent` | Minden sor tartalma pontosan megfelel a vártnak. |
| `GetRows_IgnoresEmptyLines` | Üres sorok (pl. záró sortörés) nem kerülnek bele az eredménybe. |
| `GetRows_HandlesWindowsLineEndings` | `\r\n` sortörések esetén sem maradnak `\r` karakterek a sorok végén. |
| `GetRows_SingleRowMap` | Egysoros térkép is helyesen feldolgozható. |

---

## Hogyan futtathatók a tesztek?

1. Nyisd meg a Unity Editor-t.
2. Menj a **Window → General → Test Runner** menüpontra.
3. Válaszd az **EditMode** fület.
4. Kattints a **Run All** gombra.

Minden teszt zöld pipával jelenik meg, ha sikeres, piros X-szel, ha hibás.

---

## Miért Edit Mode tesztek?

Az Edit Mode tesztek gyorsabbak, mint a Play Mode tesztek, mert:
- Nem kell elindítani a játékmotor teljes ciklusát.
- Azonnali, determinisztikus eredményt adnak.
- Ideálisak **logika-tesztelésre** (számítások, állapotgépek, adatfeldolgozás), ahol nincs szükség valódi renderelésre vagy fizikaszimulációra.

A tesztek **izoláltak**: minden teszt saját, frissen létrehozott objektumon fut, így egymás eredményét nem befolyásolják.
