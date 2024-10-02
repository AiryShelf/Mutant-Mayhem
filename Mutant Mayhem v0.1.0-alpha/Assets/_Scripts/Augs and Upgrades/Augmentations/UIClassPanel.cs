using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIClassPanel : MonoBehaviour
{
    [SerializeField] Image fighterOverlay;
    [SerializeField] Image neutralOverlay;
    [SerializeField] Image builderOverlay;
    [SerializeField] TextMeshProUGUI classNameText;
    [SerializeField] TextMeshProUGUI classDescriptionText;
    [TextArea(5,5)]
    [SerializeField] string fighterDescription;
    [TextArea(5,5)]
    [SerializeField] string neutralDescription;
    [TextArea(5,5)]
    [SerializeField] string builderDescription;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 1) // Mothership
            UpdateButtonsAndText();
    }

    public void OnFighterClicked()
    {
        ClassManager.Instance.selectedClass = PlayerClass.Fighter;
        UpdateButtonsAndText();
    }

    public void OnNeutralClicked()
    {
        ClassManager.Instance.selectedClass = PlayerClass.Neutral;
        UpdateButtonsAndText();
    }

    public void OnBuilderClicked()
    {
        ClassManager.Instance.selectedClass = PlayerClass.Builder;
        UpdateButtonsAndText();
    }

    public void UpdateButtonsAndText()
    {
        //Debug.Log("Updated class buttons");
        PlayerClass playerClass = ClassManager.Instance.selectedClass;
        // Remove button's overlay if selected
        fighterOverlay.gameObject.SetActive(playerClass != PlayerClass.Fighter);
        neutralOverlay.gameObject.SetActive(playerClass != PlayerClass.Neutral);
        builderOverlay.gameObject.SetActive(playerClass != PlayerClass.Builder);

        // Text
        classNameText.text = playerClass + " class: ";
        
        switch (playerClass)
        {
            case PlayerClass.Fighter:
                classDescriptionText.text = fighterDescription;
            break;
            case PlayerClass.Neutral:
                classDescriptionText.text = neutralDescription;
            break;
            case PlayerClass.Builder:
                classDescriptionText.text = builderDescription;
            break;
        }

    }
}
