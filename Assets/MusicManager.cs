using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Plays music playlist continuously across all scenes
/// Add this to an empty GameObject in your FIRST scene
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Playlist")]
    public List<AudioClip> songs = new List<AudioClip>();
    public bool shufflePlaylist = false;

    [Header("Settings")]
    [Range(0f, 10f)]
    public float volume = 1f;
    public float fadeTime = 1f;

    [Header("Auto Start")]
    public bool playOnStart = true;

    // Components
    private AudioSource audioSource;

    // State
    private int currentIndex = 0;
    private bool isPlaying = false;
    private List<int> playOrder = new List<int>();

    void Awake()
    {
        // Singleton - destroy duplicates
        if (Instance != null && Instance != this)
        {
            Debug.Log("[Music] Duplicate MusicManager destroyed");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false; // We handle looping manually
        audioSource.volume = volume;

        Debug.Log("[Music] MusicManager ready with " + songs.Count + " songs");
    }

    void Start()
    {
        if (playOnStart && songs.Count > 0)
        {
            Play();
        }
    }

    void Update()
    {
        // Check if song ended
        if (isPlaying && !audioSource.isPlaying)
        {
            PlayNext();
        }

        // Update volume if changed in inspector
        audioSource.volume = volume;
    }

    /// <summary>
    /// Start playing the playlist
    /// </summary>
    public void Play()
    {
        if (songs.Count == 0)
        {
            Debug.LogWarning("[Music] No songs in playlist!");
            return;
        }

        // Build play order
        BuildPlayOrder();

        currentIndex = 0;
        isPlaying = true;
        PlayCurrentSong();
    }

    /// <summary>
    /// Stop playing
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
        audioSource.Stop();
        Debug.Log("[Music] Stopped");
    }

    /// <summary>
    /// Pause music
    /// </summary>
    public void Pause()
    {
        audioSource.Pause();
        Debug.Log("[Music] Paused");
    }

    /// <summary>
    /// Resume music
    /// </summary>
    public void Resume()
    {
        audioSource.UnPause();
        Debug.Log("[Music] Resumed");
    }

    /// <summary>
    /// Skip to next song
    /// </summary>
    public void PlayNext()
    {
        if (songs.Count == 0) return;

        currentIndex++;

        // Loop back to start
        if (currentIndex >= playOrder.Count)
        {
            currentIndex = 0;

            // Reshuffle if shuffle mode
            if (shufflePlaylist)
            {
                BuildPlayOrder();
            }
        }

        PlayCurrentSong();
    }

    /// <summary>
    /// Go to previous song
    /// </summary>
    public void PlayPrevious()
    {
        if (songs.Count == 0) return;

        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = playOrder.Count - 1;
        }

        PlayCurrentSong();
    }

    /// <summary>
    /// Play specific song by index
    /// </summary>
    public void PlaySong(int index)
    {
        if (index < 0 || index >= songs.Count) return;

        // Find in play order
        for (int i = 0; i < playOrder.Count; i++)
        {
            if (playOrder[i] == index)
            {
                currentIndex = i;
                break;
            }
        }

        isPlaying = true;
        PlayCurrentSong();
    }

    /// <summary>
    /// Set volume (0-10)
    /// </summary>
    public void SetVolume(float vol)
    {
        volume = Mathf.Clamp(vol, 0f, 10f);
        audioSource.volume = volume;
    }

    /// <summary>
    /// Get current song name
    /// </summary>
    public string GetCurrentSongName()
    {
        if (songs.Count == 0 || playOrder.Count == 0) return "";
        int songIndex = playOrder[currentIndex];
        return songs[songIndex] != null ? songs[songIndex].name : "";
    }

    /// <summary>
    /// Get current song index (in original playlist)
    /// </summary>
    public int GetCurrentSongIndex()
    {
        if (playOrder.Count == 0) return -1;
        return playOrder[currentIndex];
    }

    /// <summary>
    /// Check if music is playing
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying && audioSource.isPlaying;
    }

    // Internal methods

    void BuildPlayOrder()
    {
        playOrder.Clear();

        for (int i = 0; i < songs.Count; i++)
        {
            playOrder.Add(i);
        }

        if (shufflePlaylist)
        {
            // Fisher-Yates shuffle
            for (int i = playOrder.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = playOrder[i];
                playOrder[i] = playOrder[j];
                playOrder[j] = temp;
            }
            Debug.Log("[Music] Playlist shuffled");
        }
    }

    void PlayCurrentSong()
    {
        if (playOrder.Count == 0) return;

        int songIndex = playOrder[currentIndex];
        AudioClip clip = songs[songIndex];

        if (clip == null)
        {
            Debug.LogWarning("[Music] Song at index " + songIndex + " is null, skipping");
            PlayNext();
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();

        Debug.Log("[Music] Now playing: " + clip.name + " (" + (currentIndex + 1) + "/" + songs.Count + ")");
    }

    // Optional: Keyboard controls for testing
    void LateUpdate()
    {
        // M to mute/unmute
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (audioSource.volume > 0)
            {
                audioSource.volume = 0;
                Debug.Log("[Music] Muted");
            }
            else
            {
                audioSource.volume = volume;
                Debug.Log("[Music] Unmuted");
            }
        }

        // N for next song (debug)
        if (Input.GetKeyDown(KeyCode.N))
        {
            PlayNext();
        }
    }
}