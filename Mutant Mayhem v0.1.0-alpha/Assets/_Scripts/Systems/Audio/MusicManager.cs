using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public float musicStartDb = -4;
    public float sfxStartDb = -1;
    [SerializeField] UI_MusicPlayerPanel musicPlayerPanel;

    public List<PlaylistSO> mainMenuPlaylists = new List<PlaylistSO>();
    public List<PlaylistSO> mothershipPlaylists = new List<PlaylistSO>();
    public List<PlaylistSO> gamePlaylists = new List<PlaylistSO>();
    public List<PlaylistSO> deathPlaylists = new List<PlaylistSO>();

    public List<PlaylistSO> currentPlaylists = new List<PlaylistSO>();
    public PlaylistSO currentPlaylist;
    [SerializeField] AudioSource[] audioSources;
    [SerializeField] float crossFadeDuration;
    public AudioMixer mainMixer;
    
    [Header("Dynamic, don't set here")]
    public List<SongSO> songHistory = new List<SongSO>();
    public List<PlaylistSO> playlistHistory = new List<PlaylistSO>();
    public int historyIndex;

    public int currentPlaylistIndex;
    public int currentSongIndex;
    public string currentSongTitle = "";
    public int currentSourceIndex = 0;

    public bool isPaused;
    public bool isShuffleSongsOn = true;
    public bool isShuffleAllOn = true;

    Coroutine waitForSongToEnd;
    [HideInInspector] public Player player;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene current, LoadSceneMode loadSceneMode)
    {
        musicPlayerPanel = FindObjectOfType<UI_MusicPlayerPanel>();

        // Set to scene's Playlist (Scenes over build index of 1 are gameplay scenes)
        switch (current.buildIndex)
        {
            case 0:
                SwitchCurrentPlaylists(mainMenuPlaylists);
            break;

            case 1:
                SwitchCurrentPlaylists(mothershipPlaylists);
            break;

            default:
                player = FindObjectOfType<Player>();
                musicPlayerPanel.player = player;
                
                SwitchCurrentPlaylists(gamePlaylists);
            break;
        }

        PlayFirstSong(crossFadeDuration);
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneUnloaded(Scene current)
    {
        StopAllPlaying();
        songHistory.Clear();
        playlistHistory.Clear();
        historyIndex = 0;
    }

    #region Controls

    public void NextSongPressed()
    {
        //StopAllPlaying();
        PlayNextSong(0.1f);
    }

    public void PrevSongPressed()
    {
        //StopAllPlaying();
        PlayPrevSong();
    }

    public void TurnShuffleSongsOn()
    {
        isShuffleSongsOn = true;
        isShuffleAllOn = false;
        currentPlaylist.ShufflePlaylist();
    }

    public void TurnShuffleAllOn()
    {
        isShuffleSongsOn = true;
        isShuffleAllOn = true;
        currentPlaylist.ShufflePlaylist();
    }

    public void TurnShuffleOff()
    {
        isShuffleSongsOn = false;
        isShuffleAllOn = false;

        // Store current playlist and song
        int currPlaylistIndex = currentPlaylistIndex;
        SongSO currentSong = currentPlaylist.songList[currentSongIndex];

        // Reset entire list of playlists
        Scene currentScene = SceneManager.GetActiveScene();
        switch (currentScene.buildIndex)
        {
            case 0:
                SwitchCurrentPlaylists(mainMenuPlaylists);
            break;

            case 1:
                SwitchCurrentPlaylists(mothershipPlaylists);
            break;

            default:
                
                SwitchCurrentPlaylists(gamePlaylists);
            break;
        }

        // Set playlist and songIndex
        SwitchPlaylist(currPlaylistIndex);
        currentSongIndex = currentPlaylist.songList.IndexOf(currentSong);
    }

    public void PlayOrPausePressed()
    {
        StopAllCoroutines();

        if (!isPaused)
        {
            SetAllPause(true);
            isPaused = true;
        }
        else
        {
            isPaused = false;
            ResumePlaying(currentPlaylist.songList[currentSongIndex]);
        }
    }

    void SetAllPause(bool pause)
    {
        foreach (AudioSource source in audioSources)
        {
            if (pause)
                source.Pause();
            else
                source.UnPause();
        }
    }

    void ResumePlaying(SongSO currentSong)
    {
        if (currentSong == null || audioSources[currentSourceIndex].clip == null)
        {
            Debug.LogError("Music Manager: Could not resume playing, song or audiosource is null");
        }

        // Calculate remaining time in track
        float elapsedTime = audioSources[currentSourceIndex].time;
        float totalDuration = currentSong.audioClip.length * currentSong.numberOfLoops;
        float remainingTime = totalDuration - elapsedTime;

        // Restart playback
        audioSources[currentSourceIndex].Play();
        StartCoroutine(WaitForSongToEnd(remainingTime));
    }

    public void StopAllPlaying()
    {
        foreach (AudioSource source in audioSources)
        {
            source.Stop();
        }

        StopAllCoroutines();
    }

    public void PlaylistSelected(int playlistIndex)
    {
        //StopAllPlaying();

        if (playlistIndex >= 0 && playlistIndex < currentPlaylists.Count)
        {
            historyIndex++;
            SwitchPlaylist(playlistIndex);
            PlayFirstSong(0.1f);
        }
        else
        {
            Debug.LogError("Invalid playlist index.");
        }
    }

    public void ShufflePressed()
    {
        if (!isShuffleSongsOn)
            TurnShuffleSongsOn();
        else if (!isShuffleAllOn)
            TurnShuffleAllOn();
        else 
            TurnShuffleOff();
    }

    #endregion

    #region Fades

    void CrossFade(SongSO newSong, float fadeDuration)
    {
        StopAllCoroutines();

        GetNextAudioSource();
        StartCoroutine(FadeOut(fadeDuration));
        StartCoroutine(FadeIn(newSong, fadeDuration));

        waitForSongToEnd = StartCoroutine(WaitForSongToEnd(newSong.audioClip.length * newSong.numberOfLoops - crossFadeDuration));
    }

    IEnumerator FadeIn(SongSO song, float fadeDuration)
    {
        isPaused = false;

        if (musicPlayerPanel != null)
        {
            musicPlayerPanel.UpdateTrackInfo(currentPlaylist, song);
            musicPlayerPanel.UpdatePlayButton();
        }

        AudioSource audioSource = audioSources[currentSourceIndex];
        audioSource.clip = song.audioClip;
        audioSource.volume = 0;
        audioSource.Play();

        // Fade in
        float targetVolume = song.volume;
        float currentTime = 0;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0, targetVolume, currentTime / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    IEnumerator FadeOut(float fadeDuration)
    {
        // Get previous index and Audio Source
        int prevIndex = currentSourceIndex - 1;
        if (prevIndex < 0)
            prevIndex = audioSources.Length - 1;

        // Fade out
        AudioSource audioSource = audioSources[prevIndex];
        float startVolume = audioSource.volume;
        float currentTime = 0;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, currentTime / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        //audioSources[prevIndex].volume = startVolume;
    }

    #endregion

    #region Switch Song/Playlist

    void PlayFirstSong(float fadeTime)
    {
        // Trim everything ahead of current History Index, replace current history index
        if (historyIndex < songHistory.Count - 1)
        {
            songHistory.RemoveRange(historyIndex, songHistory.Count - (historyIndex + 1));
            playlistHistory.RemoveRange(historyIndex, playlistHistory.Count - (historyIndex + 1));
        }

        if (currentPlaylist == null || currentPlaylist.songList.Count == 0)
        {
            Debug.LogWarning("No songs available in the playlist.");
            return;
        }

        // Play first song
        SongSO firstSong = currentPlaylist.songList[currentSongIndex];
        CrossFade(firstSong, fadeTime);
        
        if (musicPlayerPanel != null)
            musicPlayerPanel.UpdateTrackInfo(currentPlaylist, firstSong);

        AddToHistory(firstSong);
    }
    
    void PlayNextSong(float fadeTime)
    {
        // Play next in history
        historyIndex++;
        if (historyIndex < songHistory.Count)
        {
            SwitchPlaylistInHistory(playlistHistory[historyIndex]);
            currentSongIndex = currentPlaylist.songList.IndexOf(songHistory[historyIndex]);

            CrossFade(currentPlaylist.songList[currentSongIndex], fadeTime);

            return;
        }

        // Next song index
        currentSongIndex++;

        if (isShuffleAllOn)
            SwitchPlaylist(Random.Range(0, currentPlaylists.Count)); 
        else if (currentSongIndex >= currentPlaylist.songList.Count)
        {
            // Loop back to beginning of playlist and reshuffle
            currentSongIndex = 0;
            if (isShuffleSongsOn)
                currentPlaylist.ShufflePlaylist();
        }
        
        // Play next song
        SongSO song = currentPlaylist.songList[currentSongIndex];
        AddToHistory(song);

        CrossFade(song, fadeTime);
    }

    void PlayPrevSong()
    {
        // Restart the current song if it's been playing
        if (audioSources[currentSourceIndex].time > 3.0f)
        {
            audioSources[currentSourceIndex].time = 0;
            StartCoroutine(FadeIn(currentPlaylist.songList[currentSongIndex], 0.1f)); 
            return;
        }

        if (historyIndex > 0)
        {
            historyIndex--;
            SwitchPlaylistInHistory(playlistHistory[historyIndex]);
            currentSongIndex = currentPlaylist.songList.IndexOf(songHistory[historyIndex]);

            CrossFade(currentPlaylist.songList[currentSongIndex], 0.1f);
        }
        else
        {
            Debug.Log("No previous songs in history.");
            audioSources[currentSourceIndex].time = 0;
            audioSources[currentSourceIndex].Play();
        }
    }

    IEnumerator WaitForSongToEnd(float seconds)
    {
        // Wait for the duration of the song (AudioSource.clip.length)
        yield return new WaitForSecondsRealtime(seconds);

        PlayNextSong(crossFadeDuration);
    }

    void SwitchPlaylist(int index)
    {
        currentPlaylist = currentPlaylists[index];
        currentPlaylistIndex = index;
        
        currentSongIndex = 0;

        if (isShuffleSongsOn)
            currentPlaylist.ShufflePlaylist();
    }

    void SwitchPlaylistInHistory(PlaylistSO playlist)
    {
        currentPlaylist = playlist;
        currentPlaylistIndex = currentPlaylists.IndexOf(playlist);
    }

    void SwitchCurrentPlaylists(List<PlaylistSO> newList)
    {
        currentPlaylists.Clear();
        foreach (PlaylistSO list in newList)
            currentPlaylists.Add(Instantiate(list, transform));

        if (musicPlayerPanel != null)
            musicPlayerPanel.ResetPlaylistDropdown();

        if (isShuffleAllOn)
            SwitchPlaylist(Random.Range(0, currentPlaylists.Count));
        else
            SwitchPlaylist(0);
    }

    void AddToHistory(SongSO song)
    {
        songHistory.Add(song);

        playlistHistory.Add(currentPlaylist);
    }

    void GetNextAudioSource()
    {
        currentSourceIndex++;
        if (currentSourceIndex >= audioSources.Length)
            currentSourceIndex = 0;
    }
    #endregion
}
