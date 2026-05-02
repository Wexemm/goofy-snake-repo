using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Goofy Snake/Level Data")]
public class LevelData : ScriptableObject
{
    [TextArea(5, 30)]
    [Tooltip("Ellenség karakterek: 'e' = Chaser, 'q' = Red enemy, 'u' = Blue enemy")]
    public string map;

    [Header("Ellenség spawnok (opcionális – alternatíva a térképkarakter helyett)")]
    public EnemySpawnData[] enemySpawns;

    public string[] GetRows()
    {
        return System.Array.FindAll(
            map.Replace("\r", "").Split('\n'),
            row => row.Length > 0
        );
    }
}

[System.Serializable]
public class EnemySpawnData
{
    public enum EnemyType { Chaser, Red, Blue }

    [Tooltip("Az ellenség típusa.")]
    public EnemyType type;

    [Tooltip("Rácskoordináta (oszlop, sor) — ugyanaz a rendszer mint a térkép.")]
    public Vector2Int gridPosition;
}
