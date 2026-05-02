using UnityEngine;

public class ExitZone : MonoBehaviour
{
    public static bool player1Finished = false;
    public static bool player2Finished = false;

    void OnTriggerEnter2D(Collider2D other)
    {
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
        if (other.CompareTag("Player1")) player1Finished = false;
        if (other.CompareTag("Player2")) player2Finished = false;
    }
}