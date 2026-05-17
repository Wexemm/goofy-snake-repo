using UnityEngine;

public class ExitZone : MonoBehaviour
{
    public static bool player1Finished = false;
    public static bool player2Finished = false;

    // Verseny módban megakadályozza, hogy kétszer triggerelődjön a győzelem
    private static bool raceFinished = false;

    void Awake()
    {
        // Új pálya betöltésekor resetelünk
        raceFinished = false;
        player1Finished = false;
        player2Finished = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ── Verseny mód (Labyrinth): az első célba érő játékos nyer ──
        if (PlayerPrefs.GetInt("GameMode", 0) == 1)
        {
            if (raceFinished) return;

            if (other.CompareTag("Player1"))
            {
                raceFinished = true;
                Debug.Log("[Race] Player 1 nyert!");
                GameManager.instance.RaceWin(1);
            }
            else if (other.CompareTag("Player2"))
            {
                raceFinished = true;
                Debug.Log("[Race] Player 2 nyert!");
                GameManager.instance.RaceWin(2);
            }
            return;
        }

        // ── Normál mód: mindkét játékosnak célba kell érnie ──
        if (other.CompareTag("Player1"))
        {
            player1Finished = true;
            Debug.Log("Player1 célban!");
        }

        if (other.CompareTag("Player2"))
        {
            player2Finished = true;
            Debug.Log("Player2 célban!");
        }

        if (player1Finished && player2Finished)
        {
            Debug.Log("Mindkét játékos célban! GYŐZELEM!");
            player1Finished = false;
            player2Finished = false;
            GameManager.instance.Win();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (PlayerPrefs.GetInt("GameMode", 0) == 1) return;
        if (other.CompareTag("Player1")) player1Finished = false;
        if (other.CompareTag("Player2")) player2Finished = false;
    }
}