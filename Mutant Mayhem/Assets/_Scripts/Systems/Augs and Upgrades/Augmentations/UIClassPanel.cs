using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

    void Start()
    {
        // Load selected class from profile
        if (ProfileManager.Instance.currentProfile.selectedClassName != "")
        {
            PlayerClass savedClass = (PlayerClass)System.Enum.Parse(typeof(PlayerClass), ProfileManager.Instance.currentProfile.selectedClassName);
            // Ensure saved class is valid (in case of changes to PlayerClass enum)
            if (System.Enum.IsDefined(typeof(PlayerClass), savedClass))
                ClassManager.Instance.SelectClass(savedClass);
        }
        
        UpdateButtonsAndText();
    }

    public void OnFighterClicked()
    {
        ClassManager.Instance.SelectClass(PlayerClass.Fighter);
        UpdateButtonsAndText();
    }

    public void OnNeutralClicked()
    {
        ClassManager.Instance.SelectClass(PlayerClass.Neutral);
        UpdateButtonsAndText();
    }

    public void OnBuilderClicked()
    {
        ClassManager.Instance.SelectClass(PlayerClass.Builder);
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
