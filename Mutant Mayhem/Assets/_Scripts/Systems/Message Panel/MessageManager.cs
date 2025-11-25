using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance { get; private set; }
    public static bool IsConversationActive => Instance != null && Instance.currentConversation != null;

    public Animator portraitAnimator;
    public MessagePortraitController portraitController;
    public Animator backgroundAnimator;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI messageText;
    public GameObject messagePanel;

    PlanetDialogueSO currentPlanetDialogue;
    ConversationData currentConversation;
    List<ConversationData> queuedConversations = new List<ConversationData>();

    [SerializeField] AudioSource voiceSource;

    bool skipMessage = false;
    bool skipConversation = false;
    bool isPaused = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartPlanetDialogue()
    {
        currentPlanetDialogue = PlanetManager.Instance.currentPlanet.planetDialogue;

        WaveController.OnWaveStarted -= OnWaveStarted;
        WaveController.OnWaveEnded -= OnWaveEnded;
        WaveController.OnWaveStarted += OnWaveStarted;
        WaveController.OnWaveEnded += OnWaveEnded;

        if (currentPlanetDialogue == null)
        {
            Debug.LogError("MessageManager: PlanetDialogueSO is null.");
            return;
        }

        if (currentPlanetDialogue.startConversation != null)
        {
            Debug.Log("MessageManager: Playing planet " + PlanetManager.Instance.currentPlanet.bodyName + " dialogue with start conversation.");
            PlayConversation(currentPlanetDialogue.startConversation);
        }
        else
        {
            Debug.LogWarning("MessageManager: No start conversation found in PlanetDialogueSO.");
        }
    }

    void OnWaveStarted(int waveIndex)
    {
        PlayWaveConversation(waveIndex, true);
    }

    void OnWaveEnded(int waveIndex)
    {
        PlayWaveConversation(waveIndex, false);
    }

    #region Conversations --------------------------------------------------

    void PlayWaveConversation(int waveIndex, bool playOnWaveStart)
    {
        //Debug.Log("MessageManager: Attempting to play wave conversation for wave " + waveIndex + ", playOnWaveStart = " + playOnWaveStart);
        if (currentPlanetDialogue == null)
        {
            Debug.LogWarning("MessageManager: PlanetDialogueSO is null when trying to play wave conversation.");
            return;
        }

        foreach (WaveDialogue waveDialogue in currentPlanetDialogue.waveDialogues)
        {
            if (waveDialogue.conversation != null &&
                waveDialogue.waveIndex == waveIndex && waveDialogue.playOnWaveStart == playOnWaveStart)
            {
                //Debug.Log("MessageManager: Playing conversation for wave " + waveIndex);
                PlayConversation(waveDialogue.conversation);
                return;
            }
        }

        Debug.LogWarning("MessageManager: No conversation found for wave " + waveIndex + " with playOnWaveStart = " + playOnWaveStart);
    }

    public void PlayConversation(ConversationData conversation)
    {
        if (conversation == null || conversation.messages.Count == 0)
        {
            Debug.LogWarning("MessageManager: Tried to play a null or empty conversation.");
            return;
        }

        queuedConversations.Add(conversation);

        if (currentConversation == null)
        {
            StartCoroutine(PlayConversationRoutine());
        }
    }

    IEnumerator PlayConversationRoutine()
    {
        while (queuedConversations.Count > 0)
        {
            currentConversation = queuedConversations[0];
            queuedConversations.RemoveAt(0);

            skipConversation = false;

            foreach (MessageSO message in currentConversation.messages)
            {
                if (skipConversation)
                {
                    //Debug.Log("MessageManager: Skipping conversation due to skipConversation flag.");
                    break;
                }

                if (message.touchscreenOnly && (InputManager.LastUsedDevice != Touchscreen.current))
                {
                    //Debug.Log("MessageManager: Skipping touchscreen-only message for " + message.speakerName);
                    continue;
                }

                yield return StartCoroutine(PlayMessage(message));
            }

            currentConversation = null;
        }
    }

    public void StopAllConversations()
    {
        //Debug.Log("MessageManager: Stopping all conversations.");
        skipConversation = true;
        currentConversation = null;
        queuedConversations.Clear();
        StopMessage();
    }

    #endregion

    #region Messages -------------------------------------------------------

    IEnumerator PlayMessage(MessageSO message)
    {
        //Debug.Log("MessageManager: Playing message for " + message.speakerName);
        //Debug.Log($"MessageManager: message voice clip: {message.voiceClip?.name}");
        skipMessage = false;
        yield return new WaitForSeconds(message.messageStartDelay);

        messagePanel.SetActive(true);

        speakerNameText.text = message.speakerName;
        speakerNameText.color = message.speakerNameColor;
        messageText.text = message.messageText;

        // Use the preset AudioSource referenced from the inspector
        if (voiceSource == null)
        {
            Debug.LogError("MessageManager: VoiceSource reference is missing!");
            yield break;
        }

        voiceSource.Stop();                         // ensure the previous clip is stopped
        voiceSource.transform.position = transform.position;
        voiceSource.clip = message.voiceClip.clips[0];  // assign new clip from SoundSO

        // Apply optional random pitch variation carried over from SoundSO
        float rand = Random.Range(-message.voiceClip.pitchRandRange, message.voiceClip.pitchRandRange);
        voiceSource.pitch = 1f + rand;

        voiceSource.Play();

        portraitController.SetAudioSource(voiceSource);

        yield return null;

        if (isPaused) PauseMessage();

        if (message.portraitAnimatorController != null)
        {
            portraitAnimator.runtimeAnimatorController = message.portraitAnimatorController;
        }
        else
        {
            Debug.LogError("MessageManager: No portrait animator controller found for " + message.speakerName);
            portraitAnimator.runtimeAnimatorController = null;
        }
        if (message.backgroundAnimatorController != null)
        {
            backgroundAnimator.runtimeAnimatorController = message.backgroundAnimatorController;
        }
        else
        {
            Debug.LogError("MessageManager: No background animator controller found for " + message.speakerName);
            backgroundAnimator.runtimeAnimatorController = null;
        }

        while (voiceSource != null)
        {
            if (skipMessage)
            {
                //Debug.Log("MessageManager: Skipping message for " + message.speakerName);
                break;
            }

            if (isPaused)
            {
                yield return null;
                continue;
            }

            if (!voiceSource.isPlaying)
            {
                //Debug.Log("MessageManager: VoiceSource " + voiceSource.name + " finished playing");
                break;
            }

            yield return null;
        }

        if (!skipMessage)
            yield return new WaitForSeconds(message.messageEndDelay);

        // Stop playing
        messagePanel.SetActive(false);
        if (voiceSource != null)
        {
            voiceSource.Stop();
        }
        skipMessage = false;
        
        yield return null;
    }

    public void StopMessage()
    {
        skipMessage = true;
        if (voiceSource != null)
        {
            voiceSource.Stop();
        }
    }

    public void PauseMessage()
    {
        if (!IsConversationActive)
        {
            Debug.LogWarning("MessageManager: No conversation is active to pause.");
            return;
        }

        isPaused = true;
        messagePanel.SetActive(false);
        if (voiceSource != null && voiceSource.isPlaying)
        {
            voiceSource.Pause();
            //Debug.Log("MessageManager: Paused message voice clip.");
        }
        else
        {
            Debug.LogWarning("MessageManager: No voice source is playing to pause.");
        }
    }

    public void UnPauseMessage()
    {
        if (!IsConversationActive)
        {
            Debug.LogWarning("MessageManager: No conversation is active to unpause.");
            return;
        }

        messagePanel.SetActive(true);
        isPaused = false;
        
        if (voiceSource != null)
        {
            voiceSource.Play();
            //Debug.Log("MessageManager: Unpaused message voice clip.");
        }
        else
        {
            Debug.LogWarning("MessageManager: No voice source is paused to unpause.");
        }
    }
    
    #endregion
}
