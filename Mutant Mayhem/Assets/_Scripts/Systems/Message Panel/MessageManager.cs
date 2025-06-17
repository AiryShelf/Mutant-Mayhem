using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance { get; private set; }

    public Animator portraitAnimator;
    public Animator backgroundAnimator;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI messageText;
    public GameObject messagePanel;

    PlanetDialogueSO currentPlanetDialogue;
    ConversationSO currentConversation;
    List<ConversationSO> queuedConversations = new List<ConversationSO>();

    AudioSource voiceSource;

    bool skipMessage = false;
    bool skipConversation = false;

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

        WaveControllerRandom.OnWaveStarted -= OnWaveStarted;
        WaveControllerRandom.OnWaveEnded -= OnWaveEnded;
        WaveControllerRandom.OnWaveStarted += OnWaveStarted;
        WaveControllerRandom.OnWaveEnded += OnWaveEnded;

        if (currentPlanetDialogue == null)
        {
            Debug.LogError("MessageManager: PlanetDialogueSO is null.");
            return;
        }

        if (currentPlanetDialogue.startConversation != null)
        {
            Debug.Log("MessageManager: Starting planet dialogue with start conversation.");
            StartCoroutine(ShowConversationRoutine(currentPlanetDialogue.startConversation));
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
                Debug.Log("MessageManager: Playing conversation for wave " + waveIndex);
                ShowConversation(waveDialogue.conversation);
                return;
            }
        }

        Debug.LogWarning("MessageManager: No conversation found for wave " + waveIndex + " with playOnWaveStart = " + playOnWaveStart);
    }

    public void ShowConversation(ConversationSO conversation)
    {
        if (currentConversation != null)
        {
            Debug.Log("MessageManager: Current conversation is not null, adding request to queue.");
            queuedConversations.Add(conversation);
            return;
        }

        StartCoroutine(ShowConversationRoutine(conversation));
    }

    IEnumerator ShowConversationRoutine(ConversationSO conversation)
    {
        skipConversation = false;

        if (conversation == null || conversation.messages.Count == 0)
        {
            Debug.LogWarning("MessageManager: No messages found in the conversation, or it's null.");
            yield break;
        }

        if (currentConversation != null)
        {
            queuedConversations.Add(currentConversation);
            Debug.Log("MessageManager: Current conversation is not null, adding to queue.");
            yield break;
        }

        currentConversation = conversation;

        foreach (MessageSO message in conversation.messages)
        {
            if (skipConversation)
            {
                Debug.Log("MessageManager: Skipping conversation due to skipConversation flag.");
                break;
            }
            yield return StartCoroutine(ShowMessage(message));
        }

        currentConversation = null;

        if (queuedConversations.Count > 0)
        {
            Debug.Log("MessageManager: Processing queued conversations.");
            ConversationSO nextConversation = queuedConversations[0];
            queuedConversations.RemoveAt(0);
            StartCoroutine(ShowConversationRoutine(nextConversation));
        }
    }

    public void StopCurrentConversation()
    {
        if (currentConversation != null)
        {
            Debug.Log("MessageManager: Stopping current conversation.");
            skipConversation = true;
            currentConversation = null;
            StopMessage();
        }
        else
        {
            Debug.LogWarning("MessageManager: No current conversation to stop.");
        }
    }

    public void StopAllConversations()
    {
        Debug.Log("MessageManager: Stopping all conversations.");
        skipConversation = true;
        currentConversation = null;
        queuedConversations.Clear();
        StopMessage();
    }

    #endregion

    #region Messages -------------------------------------------------------

    IEnumerator ShowMessage(MessageSO message)
    {
        skipMessage = false;
        yield return new WaitForSeconds(message.messageStartDelay);

        messagePanel.SetActive(true);

        speakerNameText.text = message.speakerName;
        speakerNameText.color = message.speakerNameColor;
        messageText.text = message.messageText;
        voiceSource = AudioManager.Instance.PlaySoundAt(message.voiceClip, transform.position);

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

        float timer = 0f;
        float duration = message.voiceClip.clips[0].length + message.messageEndDelay;
        while (timer < duration)
        {
            if (skipMessage)
            {
                Debug.Log("MessageManager: Skipping message for " + message.speakerName);
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        StopMessage();
    }

    public void StopMessage()
    {
        skipMessage = true;
        messagePanel.SetActive(false);
        AudioManager.Instance.StopSound(voiceSource);
    }
    
    #endregion
}
