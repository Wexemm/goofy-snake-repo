using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private string musicClipPath = "Music";
    [SerializeField] private float volume = 0.5f;

    private AudioSource _audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudio()
    {
        // Get or create AudioSource
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Load and play background music
        var musicClip = Resources.Load<AudioClip>(musicClipPath);
        if (musicClip != null)
        {
            _audioSource.clip = musicClip;
            _audioSource.volume = volume;
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
            _audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"AudioManager: Could not load audio clip at Resources/{musicClipPath}");
        }
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (_audioSource != null)
            _audioSource.volume = volume;
    }

    public float GetVolume()
    {
        return volume;
    }
}
