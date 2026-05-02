using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ColoredArea : MonoBehaviour
{
    [Tooltip("Csak ez a szín léphet be. Piros areánál Red, kék areánál Blue.")]
    public PlayerColor allowedColor;

    private static bool restarting = false;

    void OnEnable()
    {
        restarting = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (restarting) return;

        PlayerIdentity identity = other.GetComponent<PlayerIdentity>();
        if (identity == null) return;

        if (identity.playerColor != allowedColor)
        {
            restarting = true;
            StartCoroutine(ExplodeAndRestart(other.gameObject));
        }
    }

    IEnumerator ExplodeAndRestart(GameObject player)
    {
        // Mozgás és irányítás letiltása
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Robbanás effekt
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        Color explosionColor = sr != null ? sr.color : Color.white;
        SpawnExplosion(player.transform.position, explosionColor);

        // Karakter elrejtése
        if (sr != null) sr.enabled = false;

        yield return new WaitForSeconds(2f);

        // Szint újraindítása
        int currentLevel = PlayerPrefs.GetInt("currentLevel", 0);
        if (LevelManager.instance != null)
            LevelManager.instance.LoadLevel(currentLevel);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void SpawnExplosion(Vector3 position, Color color)
    {
        GameObject obj = new("Explosion");
        obj.transform.position = position;

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.startColor = color;
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        main.duration = 0.2f;
        main.loop = false;
        main.gravityModifier = 0.3f;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 50) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.05f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 10;

        ps.Play();
        Destroy(obj, 3f);
    }
}
