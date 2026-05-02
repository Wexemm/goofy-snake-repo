using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Goofy Snake/Level Config")]
public class LevelConfig : ScriptableObject
{
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject redAreaPrefab;
    public GameObject blueAreaPrefab;
    public GameObject redButtonPrefab;
    public GameObject blueButtonPrefab;
    public GameObject exitPrefab;
    public GameObject player1Prefab;
    public GameObject player2Prefab;

    [Header("Sárga (Yellow) - 'y' gomb, 'Y' lever, '3' kapu")]
    public GameObject yellowButtonPrefab;
    public GameObject yellowLeverPrefab;
    public GameObject yellowGatePrefab;

    [Header("Lila (magenta) - 'p' gomb, 'P' lever, '4' kapu")]
    public GameObject magentaButtonPrefab;
    public GameObject magentaLeverPrefab;
    public GameObject magentaGatePrefab;

    [Header("Cián (Cyan) - 'c' gomb, 'C' lever, '5' kapu")]
    public GameObject cyanButtonPrefab;
    public GameObject cyanLeverPrefab;
    public GameObject cyanGatePrefab;

    [Header("Ellenségek (Enemies) - 'e' chaser, 'q' red enemy, 'u' blue enemy")]
    public GameObject chaserEnemyPrefab;
    public GameObject redEnemyPrefab;
    public GameObject blueEnemyPrefab;
}
    