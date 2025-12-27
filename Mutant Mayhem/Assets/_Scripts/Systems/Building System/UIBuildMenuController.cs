using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIBuildMenuController : MonoBehaviour
{
    public GridLayoutGroup buttonLayoutGrid;
    public GridLayoutGroup textLayoutGrid;
    [SerializeField] List<GameObject> structureButtonPrefabs;
    [SerializeField] GameObject emptyTextPrefab;
    public List<UIStructure> uiStructureList;
    [SerializeField] ScrollRectController scrollRectController;
    [SerializeField] TextMeshProUGUI infoPanelHeader;
    [SerializeField] TextMeshProUGUI infoPanelDescription;
    [SerializeField] CanvasGroup myCanvasGroup;
    public float menuFadeDuration = 0.1f;

    Coroutine _fadeRoutine;

    [SerializeField] ControlsPanel controlsPanel;

    public bool isTouchScrolling = false;
    public Vector2 touchStartPos;
    int currentIndex;
    Player player;
    BuildingSystem buildingSystem;
    InputAction scrollAction;
    InputAction swapWithDestroyAction;
    bool isMenuOpen = false;

    [System.Serializable]
    private class BuildMenuEntry
    {
        public bool isHeader;
        public int sectionIndex;
        public int originalIndex;
        public GameObject buttonGO;
        public GameObject textGO;
        public UIStructure uiStructure; // null for headers
    }

    private readonly List<BuildMenuEntry> _entries = new List<BuildMenuEntry>();

    void Awake()
    {
        player = FindObjectOfType<Player>();

        // Clear objects in layout groups
        for (int i = buttonLayoutGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonLayoutGrid.transform.GetChild(i).gameObject);
        }

        for (int i = textLayoutGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(textLayoutGrid.transform.GetChild(i).gameObject);
        }

        buildingSystem = FindObjectOfType<BuildingSystem>();

        uiStructureList.Clear();
        _entries.Clear();

        // Build the menu entries now that we have the references
       InitializeBuildList();
    }
    
    void OnEnable()
    {
        if (player == null)
        {
            Debug.LogError("BuildMenuController did not find Player in scene");
            return;
        }
        //PlayerCredits = playerStartingCredits;

        InputActionMap uiActionMap = player.inputAsset.FindActionMap("UI");
        scrollAction = uiActionMap.FindAction("Scroll");
        InputActionMap playerActionMap = player.inputAsset.FindActionMap("Player");
        swapWithDestroyAction = playerActionMap.FindAction("Toolbar");

        scrollAction.performed += OnScroll;
        swapWithDestroyAction.performed += buildingSystem.SwapWithDestroyTool;
    }

    void OnDisable()
    {
        if (scrollAction != null)
            scrollAction.performed -= OnScroll;

        if (swapWithDestroyAction != null && buildingSystem != null)
            swapWithDestroyAction.performed -= buildingSystem.SwapWithDestroyTool;
    }

    void Start()
    {
        // Start hidden
        if (myCanvasGroup != null)
        {
            myCanvasGroup.alpha = 0f;
            myCanvasGroup.interactable = false;
            myCanvasGroup.blocksRaycasts = false;
        }
    }

    void InitializeBuildList()
    {
        uiStructureList.Clear();
        _entries.Clear();

        int originalIndex = 0;
        int currentSection = 0;

        // Build entries in the exact order of structureButtonPrefabs.
        foreach (GameObject prefab in structureButtonPrefabs)
        {
            GameObject newButton = Instantiate(prefab, buttonLayoutGrid.transform);
            UIStructure uiStructure = newButton.GetComponent<UIStructure>();

            // Header / separator item (no UIStructure attached)
            if (uiStructure == null)
            {
                currentSection++;

                GameObject newText = Instantiate(emptyTextPrefab, textLayoutGrid.transform);

                _entries.Add(new BuildMenuEntry
                {
                    isHeader = true,
                    sectionIndex = currentSection,
                    originalIndex = originalIndex,
                    buttonGO = newButton,
                    textGO = newText,
                    uiStructure = null
                });

                originalIndex++;
                continue;
            }

            // Normal structure entry
            uiStructure.originalSiblingIndex = originalIndex;
            uiStructure.Initialize(buildingSystem, scrollRectController);
            uiStructureList.Add(uiStructure);

            uiStructure.textInstance = Instantiate(uiStructure.textPrefab, textLayoutGrid.transform);

            _entries.Add(new BuildMenuEntry
            {
                isHeader = false,
                sectionIndex = currentSection,
                originalIndex = originalIndex,
                buttonGO = newButton,
                textGO = uiStructure.textInstance,
                uiStructure = uiStructure
            });

            originalIndex++;
        }

        RefreshBuildList();
    }

    #region Refresh / Toggle

    public void RefreshBuildList()
    {
        // Update interactability based on unlock state
        foreach (BuildMenuEntry entry in _entries)
        {
            if (entry.isHeader || entry.uiStructure == null)
                continue;

            bool unlocked = BuildingSystem._UnlockedStructuresDict[entry.uiStructure.structureSO.structureType];
            entry.uiStructure.MakeInteractable(unlocked);
        }

        // Build a display list that preserves header positions and sorts structures within each section
        List<BuildMenuEntry> ordered = new List<BuildMenuEntry>(_entries.Count);

        // Determine all section indices in their natural order
        int maxSection = 0;
        for (int i = 0; i < _entries.Count; i++)
            if (_entries[i].sectionIndex > maxSection)
                maxSection = _entries[i].sectionIndex;

        // Section 0 = items before the first header (if any)
        for (int section = 0; section <= maxSection; section++)
        {
            // Add headers that belong to this section in original order
            foreach (BuildMenuEntry e in _entries)
            {
                if (e.isHeader && e.sectionIndex == section)
                    ordered.Add(e);
            }

            // Collect structure entries in this section
            List<BuildMenuEntry> structuresInSection = new List<BuildMenuEntry>();
            foreach (BuildMenuEntry e in _entries)
            {
                if (!e.isHeader && e.sectionIndex == section)
                    structuresInSection.Add(e);
            }

            // Sort: unlocked first, then original order
            structuresInSection = structuresInSection
                .OrderByDescending(e => BuildingSystem._UnlockedStructuresDict[e.uiStructure.structureSO.structureType])
                .ThenBy(e => e.originalIndex)
                .ToList();

            ordered.AddRange(structuresInSection);
        }

        // Apply the visual order to both layout groups
        for (int i = 0; i < ordered.Count; i++)
        {
            ordered[i].buttonGO.transform.SetSiblingIndex(i);
            ordered[i].textGO.transform.SetSiblingIndex(i);
        }

        // Rebuild uiStructureList to match visual order (excluding headers)
        uiStructureList = ordered
            .Where(e => !e.isHeader && e.uiStructure != null)
            .Select(e => e.uiStructure)
            .ToList();

        SetMenuSelection(buildingSystem.structureInHand);
    }

    public void ToggleBuildMenu()
    {
        // Fade the entire menu as one CanvasGroup
        StartMenuFade(!isMenuOpen);
        
        isMenuOpen = !isMenuOpen;
        TouchManager.Instance.ShowLeftSideAttackButtons(!isMenuOpen);
    }

    #endregion

    #region Select

    public bool SetMenuSelection(StructureSO structure)
    {
        foreach (UIStructure uiStructure in uiStructureList)
        {
            if (uiStructure == null)
                continue;
            
            StructureSO uiStructureSO = uiStructure.structureSO;

            if (uiStructureSO.tileName == structure.tileName)
            {
                if (!uiStructure.TryToSelect(false))
                    return false;

                currentIndex = uiStructureList.IndexOf(uiStructure);
                string headerText = $"{uiStructureSO.tileName}    <color=#{ColorUtility.ToHtmlStringRGB(Color.green)}>{uiStructureSO.maxHealth}HP</color>";
                if (uiStructureSO.maxHealth <= 0)
                {
                    headerText = uiStructureSO.tileName;
                }
                infoPanelHeader.text = headerText;
                infoPanelDescription.text = uiStructureSO.description;
                //Debug.Log("BuildMenu selection changed to " + uiStructure.name);
                return true;
            }
        }

        //Debug.LogError("UIBuildMenuController: SetMenuSelection failed");
        return false;
    }

    public void SelectHighlightedStructure()
    {
        if (currentIndex < 0 || currentIndex >= uiStructureList.Count)
            return;

        uiStructureList[currentIndex].TryToSelect(true);
    }

    #endregion

    #region Scroll

    void OnScroll(InputAction.CallbackContext context)
    {
        //Debug.Log("OnScroll performed");
        if (!isMenuOpen)
            return;
        
        Vector2 scroll = context.ReadValue<Vector2>();
        
        if (scroll.y > 0)
            ScrollUp();
        else if (scroll.y < 0)
            ScrollDown();
    }

    public void ScrollUp()
    {
        if (currentIndex <= 0)
            return;

        int startIndex = currentIndex;
        currentIndex--;
        
        for (int i = currentIndex; i >= 0; i--) 
        {
            if (SetMenuSelection(uiStructureList[i].structureSO))
                return;
            currentIndex--;
        }

        currentIndex = startIndex;  
    }

    public void ScrollDown()
    {
        if (currentIndex >= uiStructureList.Count - 1)
            return;

        int startIndex = currentIndex;
        currentIndex++;
        
        for (int i = currentIndex; i < uiStructureList.Count; i++) 
        {
            if (SetMenuSelection(uiStructureList[i].structureSO))
                return;
            currentIndex++;
        }

        currentIndex = startIndex;        
    }

    #endregion

    void StartMenuFade(bool open)
    {
        if (myCanvasGroup == null)
            return;

        float target = open ? 1f : 0f;

        // Enable interaction immediately when opening; disable after fade when closing
        if (open)
        {
            myCanvasGroup.interactable = true;
            myCanvasGroup.blocksRaycasts = true;
        }

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeCanvasGroupTo(myCanvasGroup, target, menuFadeDuration, open));
    }

    IEnumerator FadeCanvasGroupTo(CanvasGroup cg, float targetAlpha, float duration, bool opening)
    {
        if (cg == null)
            yield break;

        float startAlpha = cg.alpha;

        if (duration <= 0f)
        {
            cg.alpha = targetAlpha;
        }
        else
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / duration);
                cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, u);
                yield return null;
            }
            cg.alpha = targetAlpha;
        }

        // After closing, disable interaction.
        if (!opening)
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        _fadeRoutine = null;
    }
}
