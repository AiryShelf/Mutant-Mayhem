using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_MusicPlayerPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI artistText;
    [SerializeField] TMP_Dropdown playlistsDropdown;

    [SerializeField] Button muteSFXButton;
    Color muteSFXButtonStartColor;
    [SerializeField] Sprite spr_muteSFXButton;
    [SerializeField] Sprite spr_mutedSFXButton;
    [SerializeField] Slider sfxVolumeSlider;
    [SerializeField] Button muteMusicButton;
    Color muteMusicButtonStartColor;
    [SerializeField] Sprite spr_muteMusicButton;
    [SerializeField] Sprite spr_mutedMusicButton;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Button playButton;
    [SerializeField] Sprite spr_playButton;
    [SerializeField] Sprite spr_pauseButton;
    [SerializeField] Button stopButton;
    [SerializeField] Button nextButton;
    [SerializeField] Button backButton;
    [SerializeField] TextMeshProUGUI shuffleButtonText;
    [SerializeField] Color buttonDisabledColor = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] float sliderDecibelsMin = -24f;

    public AudioMixer mainMixer;
    bool sfxMuted;
    bool musicMuted;

    void Start()
    {
        muteMusicButtonStartColor = muteMusicButton.image.color;
        muteSFXButtonStartColor = muteSFXButton.image.color;
        mainMixer.SetFloat("musicVolume", MusicManager.Instance.musicStartDb);
        mainMixer.SetFloat("sfxVolume", MusicManager.Instance.sfxStartDb);

        UpdateShuffleButton();
        UpdatePlayButton();
        UpdateVolumeSliders();
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

        DeselectButton();
    }

    public void OnPlaylistSelected()
    {
        UpdatePlayButton();
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

        UpdatePlayButton();
        DeselectButton();
    }

    public void OnBackButton()
    {
        MusicManager.Instance.PrevSongPressed();

        UpdatePlayButton();
        DeselectButton();
    }

    public void OnShuffleButton()
    {
        MusicManager.Instance.ShufflePressed();

        UpdateShuffleButton();
        DeselectButton();
    }

    void SetSFXVolume(float value)
    {
        // Convert to decibels
        value = Mathf.Lerp(sliderDecibelsMin, 0, value);
        if (value <= sliderDecibelsMin)
            value = -80;
        
        if (!sfxMuted)
            mainMixer.SetFloat("sfxVolume", value);
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

    public void UpdateTrackInfo(PlaylistSO playlist, SongSO song)
    {
        titleText.text = song.title;
        artistText.text = song.artist;

        int playlistIndex = MusicManager.Instance.currentPlaylists.IndexOf(playlist);
        if (playlistIndex != -1)
            playlistsDropdown.value = playlistIndex;
        else
            Debug.LogError("Could not find index of playlist for dropdown");

        DeselectButton();
    }

    public void UpdatePlayButton()
    {
        if (MusicManager.Instance.isPaused)
            playButton.image.sprite = spr_playButton;
        else 
            playButton.image.sprite = spr_pauseButton;
    }

    void UpdateShuffleButton()
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
}
