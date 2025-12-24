using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_MusicPlayerPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static UI_MusicPlayerPanel Instance;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI artistText;
    [SerializeField] TMP_Dropdown playlistsDropdown;

    [SerializeField] Button muteSFXButton;
    Color muteSFXButtonStartColor;
    [SerializeField] Sprite spr_muteSFXButton;
    [SerializeField] Sprite spr_mutedSFXButton;
    [SerializeField] Slider sfxVolumeSlider;
    [SerializeField] float voiceVolumeOffset = 10f; // Voice is 10dB louder than SFX

    [SerializeField] float voiceVolumeCap = 5f;
    [SerializeField] float uiVolumeOffset = 10f;
    [SerializeField] float uiVolumeCap = 4f;
    [SerializeField] Button muteMusicButton;
    Color muteMusicButtonStartColor;
    [SerializeField] Sprite spr_muteMusicButton;
    [SerializeField] Sprite spr_mutedMusicButton;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Button playButton;
    [SerializeField] Sprite spr_playButton;
    [SerializeField] Sprite spr_pauseButton;
    [SerializeField] TextMeshProUGUI shuffleButtonText;
    [SerializeField] Color buttonDisabledColor = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] float sliderDecibelsMin = -24f;
    [SerializeField] GameObject mainPanel;
    public GameObject persistentCanvas;

    public AudioMixer mainMixer;
    bool sfxMuted;
    bool musicMuted;

    [HideInInspector] public Player player;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(persistentCanvas);
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(persistentCanvas);
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        muteMusicButtonStartColor = muteMusicButton.image.color;
        muteSFXButtonStartColor = muteSFXButton.image.color;
        mainMixer.SetFloat("musicVolume", MusicManager.Instance.musicStartDb);
        mainMixer.SetFloat("sfxVolume", MusicManager.Instance.sfxStartDb);

        UpdateShuffleButton();
        //UpdatePlayButton();
        UpdateVolumeSliders();

    }

    #region Controls

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (player != null)
            player.stats.playerShooter.canShoot = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (player != null)
            player.stats.playerShooter.canShoot = true;
    }

    public void OnMuteSFXButton()
    {
        if (!sfxMuted)
        {
            SetSFXVolume(0);
            sfxMuted = true;
        }
        else
        {
            sfxMuted = false;
            SetSFXVolume(sfxVolumeSlider.value);
        }

        UpdateMuteSFXButton();

        DeselectButton();
    }

    public void OnSFXVolumeSlider()
    {
        SetSFXVolume(sfxVolumeSlider.value);
        if (sfxMuted == true)
        {
            OnMuteSFXButton();
        }

        DeselectButton();
    }

    public void OnMuteMusicButton()
    {
        if (!musicMuted)
        {
            SetMusicVolume(0);
            musicMuted = true;
        }
        else
        {
            musicMuted = false;
            SetMusicVolume(musicVolumeSlider.value);
        }

        UpdateMuteMusicButton();

        DeselectButton();
    }

    public void OnMusicVolumeSlider()
    {
        SetMusicVolume(musicVolumeSlider.value);
        if (musicMuted == true)
        {
            OnMuteMusicButton();
        }

        DeselectButton();
    }

    public void OnPlaylistSelected()
    {
        MusicManager.Instance.PlaylistSelected(playlistsDropdown.value);
        DeselectButton();
    }

    public void OnPlayButton()
    {
        MusicManager.Instance.PlayOrPausePressed();

        UpdatePlayButton();
        DeselectButton();
    }

    public void OnStopButton()
    {
        MusicManager.Instance.StopAllPlaying();

        UpdatePlayButton();
        DeselectButton();
    }

    public void OnNextButton()
    {
        MusicManager.Instance.NextSongPressed();

        DeselectButton();
    }

    public void OnBackButton()
    {
        MusicManager.Instance.PrevSongPressed();

        DeselectButton();
    }

    public void OnShuffleButton()
    {
        MusicManager.Instance.ShufflePressed();

        DeselectButton();
    }

    #endregion

    #region Set / Get 

    void SetSFXVolume(float value)
    {
        // Convert to decibels
        value = Mathf.Lerp(sliderDecibelsMin, 0, value);
        if (value <= sliderDecibelsMin)
            value = -80;

        if (!sfxMuted)
        {
            mainMixer.SetFloat("sfxVolume", value);
            float voiceVolume = Mathf.Min(value + voiceVolumeOffset, voiceVolumeCap);
            mainMixer.SetFloat("voiceVolume", voiceVolume);
            float uiVolume = Mathf.Min(value + uiVolumeOffset, uiVolumeCap);
            mainMixer.SetFloat("uiVolume", uiVolume);
        }
    }

    void SetMusicVolume(float value)
    {
        // Convert to decibels
        value = Mathf.Lerp(sliderDecibelsMin, 0, value);
        if (value <= sliderDecibelsMin)
            value = -80;

        if (!musicMuted)
            mainMixer.SetFloat("musicVolume", value);
    }

    float GetSFXVolumeNorm()
    {
        float dB;
        mainMixer.GetFloat("sfxVolume", out dB);
        float value = Mathf.InverseLerp(sliderDecibelsMin, 0, dB);
        return value;
    }

    float GetMusicVolumeNorm()
    {
        float dB;
        mainMixer.GetFloat("musicVolume", out dB);
        float value = Mathf.InverseLerp(sliderDecibelsMin, 0f, dB);
        return value;
    }

    #endregion

    #region Update UI

    public void ShowPanel(bool show)
    {
        if (show)
        {
            mainPanel.SetActive(true);
            OnPlayButton();
        }
        else
        {
            mainPanel.SetActive(false);
            OnStopButton();
        }
    }

    public void UpdateTrackInfo(PlaylistSO playlist, SongSO song)
    {
        titleText.text = song.title;
        artistText.text = "By: " + song.artist;

        int playlistIndex = MusicManager.Instance.currentScenePlaylists.IndexOf(playlist);

        if (playlistIndex != -1)
            playlistsDropdown.SetValueWithoutNotify(playlistIndex);
        else
            Debug.LogError("Could not find index of playlist for dropdown");

        DeselectButton();
    }

    public void ResetPlaylistDropdown()
    {
        playlistsDropdown.ClearOptions();

        List<string> newList = new List<string>();
        foreach (PlaylistSO list in MusicManager.Instance.currentScenePlaylists)
        {
            newList.Add(list.playlistName);
        }

        playlistsDropdown.AddOptions(newList);
    }

    public void UpdatePlayButton()
    {
        if (MusicManager.Instance.isPaused)
            playButton.image.sprite = spr_playButton;
        else 
            playButton.image.sprite = spr_pauseButton;
    }

    public void UpdateShuffleButton()
    {
        // Update Shuffle button text
        shuffleButtonText.color = Color.cyan;
        if (MusicManager.Instance.isShuffleAllOn)
            shuffleButtonText.text = "Shuffle All";
        else if (MusicManager.Instance.isShuffleSongsOn)
            shuffleButtonText.text = "Shuffle";
        else 
        {
            shuffleButtonText.text = "No Shuffle";
            shuffleButtonText.color = buttonDisabledColor;
        }
    }

    void UpdateMuteSFXButton()
    {
        if (sfxMuted)
        {
            muteSFXButton.image.sprite = spr_mutedSFXButton;
            muteSFXButton.image.color = buttonDisabledColor;
        }
        else
        {
            muteSFXButton.image.sprite = spr_muteSFXButton;
            muteSFXButton.image.color = muteSFXButtonStartColor;
        }
    }

    void UpdateMuteMusicButton()
    {
        if (musicMuted)
        {
            muteMusicButton.image.sprite = spr_mutedMusicButton;
            muteMusicButton.image.color = buttonDisabledColor;
        }
        else
        {
            muteMusicButton.image.sprite = spr_muteMusicButton;
            muteMusicButton.image.color = muteMusicButtonStartColor;
        }
    }

    void UpdateVolumeSliders()
    {
        float sfxVolume = GetSFXVolumeNorm();
        float musicVolume = GetMusicVolumeNorm();

        sfxVolumeSlider.value = sfxVolume;
        musicVolumeSlider.value = musicVolume;
    }

    void DeselectButton()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    #endregion
}
