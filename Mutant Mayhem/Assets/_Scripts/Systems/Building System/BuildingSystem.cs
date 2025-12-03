using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem Instance;

    public List<StructureSO> AllStructureSOs;
    [SerializeField] List<bool> unlockedStructuresStart;
    public StructureSO structureInHand;
    public int droneHangarsBuilt = 0;

    public static event Action<float> OnPlayerCreditsChanged;
    public event Action<bool> OnBuildMenuOpen;
    private static float playerCredits;
    public static float PlayerCredits
    {
        get => playerCredits;
        set 
        { 
            if (playerCredits != value)
            {
                playerCredits = value;
                OnPlayerCreditsChanged?.Invoke(playerCredits);
            }
        }
    }
    public static float buildRange = 6f;
    public RangeCircle buildRangeCircle;
    public RangeCircle repairRangeCircle;
    [SerializeField] float buildCamLerpTime = 0.35f;
    public LayerMask layersForBuildClearCheck;
    [SerializeField] LayerMask layersToClearOnBuild;
    public UIBuildMenuController buildMenuController;
    [SerializeField] QCubeController qCubeController;
    [SerializeField] MouseLooker mouseLooker;
    [SerializeField] CameraController cameraController;
    [SerializeField] TextMeshProUGUI buildButtonText;

    [Header("Tilemaps")]
    [SerializeField] TileManager tileManager;
    [SerializeField] Tilemap structureTilemap;
    [SerializeField] Tilemap animatedTilemap;
    [SerializeField] Tilemap highlightTilemap;
    public Tilemap previewTilemap;
    [SerializeField] TileBase highlightedTileAsset;
    [SerializeField] TileBase destroyTileAsset;
    [SerializeField] TileBase blockedTileAsset;
    [SerializeField] GameObject debugDotPrefab;

    [Header("Dynamic vars, don't set here")]
    public float structureCostMult = 1;

    public static Dictionary<StructureType, bool> _UnlockedStructuresDict = 
                                        new Dictionary<StructureType, bool>();
    bool _isInBuildMode;
    public bool isInBuildMode
    {
        get => _isInBuildMode;
        set 
        { 
            if (_isInBuildMode != value)
            {
                _isInBuildMode = value;
                OnBuildMenuOpen?.Invoke(_isInBuildMode);
            }
        }
    }
    public int currentRotation;
    Vector3Int highlightedPos;
    Vector3Int lastHighlightedPos;
    public bool allHighlighted;
    public List<StructureSO> buildOnlyOneList;
    
    
    bool inRange;
    Player player;
    InputActionMap playerActionMap;
    InputAction helpAction;
    InputAction toolbarAction;
    InputAction rotateStructureAction;
    InputAction buildAction;
    InputAction cheatCodeCreditsAction;
    List<Vector3Int> destroyPositions = new List<Vector3Int>();
    public List<Vector3Int> highlightPositions = new List<Vector3Int>();
    TurretManager turretManager;

    Coroutine clearStructureInHand;
    Coroutine lockBuildCircleToMuzzle;
    public GameObject lastSelectedUiObject;
    public StructureSO lastStructureInHand;

    

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } 
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        BuildStructsAvailDict();

        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("BuildingSystem could not find Player in scene");
            return;
        }
        //PlayerCredits = playerStartingCredits;

        playerActionMap = player.inputAsset.FindActionMap("Player");
        buildAction = playerActionMap.FindAction("BuildStructure");
        helpAction = playerActionMap.FindAction("Help");
        toolbarAction = playerActionMap.FindAction("Toolbar");
        rotateStructureAction = playerActionMap.FindAction("RotateStructure");
        cheatCodeCreditsAction = playerActionMap.FindAction("CheatCodeCredits");

        rotateStructureAction.started += OnRotate;
        cheatCodeCreditsAction.started += OnCheatCodeCredits; 

        lastStructureInHand = AllStructureSOs[2];
    }

    void OnDisable()
    {
        rotateStructureAction.started -= OnRotate;
        cheatCodeCreditsAction.started -= OnCheatCodeCredits;

        _UnlockedStructuresDict.Clear();   
    }

    void Start()
    {
        player.stats.structureStats.buildingSystem =  this;
        buildRangeCircle.radius = buildRange;

        turretManager = TurretManager.Instance;
        if (turretManager == null)
        {
            Debug.LogError("BuildingSystem could not find TurretManager in scene");
        }
    }

    void FixedUpdate()
    {
        if (!isInBuildMode)
            return;
            
        // Highlight a tile
        if (structureInHand != null)
        {
            HighlightTile();
                
            if (buildAction.IsPressed())
                OnBuild();
        }
    }

    void Update()
    {
        // Keep track of currently selected object
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            lastSelectedUiObject = EventSystem.current.currentSelectedGameObject;
        }        
    }

    #region Inputs

    public void OnBuild()
    {
        if (!isInBuildMode)
        {
            return;
        }

        // Build`
        if (allHighlighted)
        {
            if (structureInHand.actionType == ActionType.Build)
            {
                Build(highlightedPos);
                StartCoroutine(DelayUIReselect());
            }
            else if (structureInHand.actionType == ActionType.Destroy)
            {
                RemoveTile(destroyPositions[0]);
                StartCoroutine(DelayUIReselect());
            }
        }
        // Messages for failed to build or destroy
        else if (inRange)
        {
            if (structureInHand.actionType == ActionType.Build)
                //MessagePanel.Instance.DelayMessage("Area not clear for building", Color.yellow, 0.1f);
            if (structureInHand.actionType == ActionType.Destroy)
                MessageBanner.Instance.DelayMessage("Unable to destroy", Color.yellow, 0.1f);
            StartCoroutine(DelayUIReselect());
        }
        else
        {
            if (structureInHand.actionType == ActionType.Build)
                MessageBanner.Instance.DelayMessage("Too far away to build", Color.yellow, 0.1f);
            if (structureInHand.actionType == ActionType.Destroy)
                MessageBanner.Instance.DelayMessage("Too far away to destroy", Color.yellow, 0.1f);
            StartCoroutine(DelayUIReselect());
        }
    }

    public IEnumerator DelayUIReselect()
    {
        yield return new WaitForFixedUpdate();
        if (lastSelectedUiObject != null)
            EventSystem.current.SetSelectedGameObject(lastSelectedUiObject);
    }

    public void RotateButtonPressed_isLeft(bool left)
    {
        if (!isInBuildMode || structureInHand.structureType == AllStructureSOs[2].structureType)
            return;

        if (left)
        {
            currentRotation += 90;
        }   
        else
        {
            currentRotation -= 90;
        }

        Rotate(structureInHand.ruleTileStructure.structureSO);
    }

    void OnRotate(InputAction.CallbackContext context)
    {
        // Return if not in build mode or is holding destroy tool
        if (!isInBuildMode || structureInHand.structureType == AllStructureSOs[2].structureType)
            return;

        if (context.control.name == "q" || context.control.name == "leftShoulder")
        {
            currentRotation += 90;
        }   
        else if (context.control.name == "e" || context.control.name == "rightShoulder")
        {
            currentRotation -= 90;
        }

        //Debug.Log(context.control.name);
        Rotate(structureInHand.ruleTileStructure.structureSO);
    }

    void Rotate(StructureSO structure)
    {
        // Normalize the rotation to be within the range (0, 360)
        currentRotation = (currentRotation % 360 + 360) % 360;
        RemoveBuildHighlight();
        // Ensure use of original SO cell positions for rotation
        if (AllStructureSOs.Contains(structure))
            structure = AllStructureSOs[AllStructureSOs.IndexOf(structure)];
        else
            return;
        structureInHand = StructureRotator.RotateStructure(structure, currentRotation);
        //lastStructureInHand = structureInHand;

        //lastHighlightedPos += Vector3Int.up * 100;
    }

    public void SwapWithDestroyTool(InputAction.CallbackContext context)
    {
        if (!isInBuildMode)
            return;

        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            // If destroy tool is not selected
            if (structureInHand.structureType != AllStructureSOs[2].structureType)
            {
                // Store structure in hand
                lastStructureInHand = structureInHand.ruleTileStructure.structureSO;
                // Select Destroy Tool
                buildMenuController.SetMenuSelection(AllStructureSOs[2]);
                ChangeStructureInHand(AllStructureSOs[2]);
                Debug.Log("Switched to destroy tool");
            }
            else
            {
                if (lastStructureInHand == null)
                {
                    return;
                }
                // Switch back to previously selected structure
                buildMenuController.SetMenuSelection(lastStructureInHand);
                ChangeStructureInHand(lastStructureInHand);
                Debug.Log("Switched back from destroy tool");
            }
        }
    }

    #endregion

    #region Menu / Unlock

    public void ToggleBuildMenu()
    {
        //Debug.Log("ToggleBuildMenu Called");

        if (!isInBuildMode)
        {
            UpgradePanelManager.Instance.CloseAllPanels();
            player.ExitInteractMode();

            if (InputManager.LastUsedDevice == Gamepad.current)
            {
                helpAction.Disable();
                toolbarAction.Disable();
            }

            //CursorManager.Instance.inMenu = true;
            buildButtonText.text = "Close";
            CursorManager.Instance.SetBuildCursor();
            InputManager.SetJoystickMouseControl(true);

            LockCameraToPlayer(true);

            isInBuildMode = true;
            SetBuildRangeCircle();
            player.playerShooter.isBuilding = true;
            buildMenuController.ToggleBuildMenu();
            
            //Debug.Log("Opened Build Panel");
            // When entering build mode, use the last structure the player had selected,
            // as long as it's unlocked. Otherwise, fall back to the Destroy Tool.
            if (lastStructureInHand != null &&
                _UnlockedStructuresDict.TryGetValue(lastStructureInHand.structureType, out bool unlocked) &&
                unlocked)
            {
                structureInHand = lastStructureInHand;
            }
            else
            {
                structureInHand = AllStructureSOs[2]; // Destroy Tool as a safe fallback
            }
            
            StartCoroutine(DelayMenuSelection()); // So that FadeCanvasGroupsWave can turn the elements on
        }
        else
        {
            helpAction.Enable();
            toolbarAction.Enable();

            //CursorManager.Instance.inMenu = false;
            buildButtonText.text = "Build";
            CursorManager.Instance.SetAimCursor();
            buildRangeCircle.EnableCircle(false);
            if (player.stats.playerShooter.isRepairing)
                SetRepairRangeCircle();
            else
            {
                InputManager.SetJoystickMouseControl(!SettingsManager.Instance.useInstantJoystickAim);
                //Debug.Log("Joystick turned off from BuildingSystem");
            }

            if (!player.stats.playerShooter.isRepairing)
                LockCameraToPlayer(false);
            
            isInBuildMode = false;
            player.playerShooter.isBuilding = false;
            buildMenuController.ToggleBuildMenu();
            
            float time = buildMenuController.fadeCanvasGroups.fadeOutAllTime;

            if (clearStructureInHand != null)
                StopCoroutine(clearStructureInHand);
            clearStructureInHand = StartCoroutine(ClearSelection(time));

            RemoveBuildHighlight();
            lastStructureInHand = structureInHand;
            structureInHand = AllStructureSOs[(int)StructureType.SelectTool];
            //Debug.Log("Closed Build Panel");
        }
    }

    public void LockCameraToPlayer(bool isLocked)
    {
        if (isLocked)
        {
            if (InputManager.LastUsedDevice == Keyboard.current)
                cameraController.ZoomAndFocus(player.transform, -2, 0.25f, buildCamLerpTime, true, false);
            else
                cameraController.ZoomAndFocus(player.transform, -4, 0.25f, buildCamLerpTime, true, false);
            mouseLooker.lockedToPlayer = true;
            cameraController.SetTouchscreenOffset(false);
        }
        else
        {
            cameraController.ZoomAndFocus(player.transform, 0, 1, buildCamLerpTime, false, false);
            mouseLooker.lockedToPlayer = false;
            cameraController.SetTouchscreenOffset(true);
        }  
    }

    public void SetBuildRangeCircle()
    {
        if (lockBuildCircleToMuzzle != null)
            StopCoroutine(lockBuildCircleToMuzzle);

        buildRangeCircle.transform.parent = player.stats.playerShooter.transform;
        buildRangeCircle.transform.position = player.stats.playerShooter.transform.position;
        buildRangeCircle.radius = buildRange;
        buildRangeCircle.EnableCircle(true);
        repairRangeCircle.EnableCircle(false);
    }

    public void SetRepairRangeCircle()
    {
        if (lockBuildCircleToMuzzle != null)
                StopCoroutine(lockBuildCircleToMuzzle);

        if (player.stats.playerShooter.isRepairing)
        {
            LockCameraToPlayer(true);
            repairRangeCircle.EnableCircle(true);
            UpdateRepairRangeCircle();

            lockBuildCircleToMuzzle = StartCoroutine(LockRepairCircleToMuzzle());

            //Vector3 worldPos = player.stats.playerShooter.muzzleTrans.position;
            //Vector3 localPos = player.transform.InverseTransformPoint(worldPos);
            //player.transform.TransformPoint(player.stats.playerShooter.muzzleTrans.position);
            //Debug.Log($"worldPos = {worldPos}, localPos = {localPos}");

            // Set buildRangeCircle's local position.
            //buildRangeCircle.transform.parent = player.stats.playerShooter.muzzleTrans;
            
        }
        else
        {
            buildRangeCircle.transform.position = player.transform.position;
            InputManager.SetJoystickMouseControl(!SettingsManager.Instance.useInstantJoystickAim);
        }
    }

    public void UpdateRepairRangeCircle()
    {
        repairRangeCircle.radius = player.stats.playerShooter.currentGunSO.bulletLifeTime * 
                                      player.stats.playerShooter.currentGunSO.bulletSpeed;
        repairRangeCircle.radiusStart = repairRangeCircle.radius;
    }

    IEnumerator LockRepairCircleToMuzzle()
    {
        while (true)
        {
            Vector3 worldPos = player.stats.playerShooter.muzzleTrans.position;
            //Vector3 localPos = player.transform.InverseTransformPoint(worldPos);
            //player.transform.TransformPoint(player.stats.playerShooter.muzzleTrans.position);
            //Debug.Log($"MuzzleTrans worldPos = {worldPos}, localPos = {localPos}");

            repairRangeCircle.transform.position = worldPos;

            yield return null;
        }
    }

    IEnumerator DelayMenuSelection()
    {
        yield return new WaitForSeconds(0.1f);

        buildMenuController.SetMenuSelection(lastStructureInHand);
    }

    IEnumerator ClearSelection(float time)
    {
        yield return new WaitForSeconds(time);

        RemoveBuildHighlight();
        //structureInHand = AllStructureSOs[(int)Structure.SelectTool];
    }

    public void ChangeStructureInHand(StructureSO structure)
    {
        if (structure != null)
            Rotate(structure);
        //Debug.Log("Changed structure in hand to: " + structure.tileName);
    }

    void BuildStructsAvailDict()
    {
        // Starts list at destroy tool
        for (int i = 2; i < AllStructureSOs.Count; i++)
        {           
            if (unlockedStructuresStart[i])
                _UnlockedStructuresDict.Add(AllStructureSOs[i].structureType, true);
            else
                _UnlockedStructuresDict.Add(AllStructureSOs[i].structureType, false);
        }
    }

    public void UnlockStructures(StructureSO rootStructure, bool playEffect)
    {
        UpgradePanelManager.Instance.PowerOnUpgradePanel(rootStructure, playEffect);

        foreach (var structure in rootStructure.structuresToUnlock)
        {
            _UnlockedStructuresDict[structure.structureType] = true;
        }

        buildMenuController.RefreshBuildList();

        if (rootStructure.canBuildOnlyOne && playEffect)
        {
            buildMenuController.SetMenuSelection(AllStructureSOs[2]); // Set to destroy tool
            ChangeStructureInHand(AllStructureSOs[2]);
        }
        
    }

    public void LockStructures(StructureSO rootStructure, bool playEffect)
    {
        UpgradePanelManager.Instance.PowerOffUpgradePanel(rootStructure, playEffect);

        if (structureInHand.structureType == rootStructure.structureType)
        {
            buildMenuController.SetMenuSelection(AllStructureSOs[2]); // Set to destroy tool
                ChangeStructureInHand(AllStructureSOs[2]);
        }

        foreach (var structure in rootStructure.structuresToUnlock)
        {
            if (structure.structureType == structureInHand.structureType)
            {
                buildMenuController.SetMenuSelection(AllStructureSOs[2]); // Set to destroy tool
                ChangeStructureInHand(AllStructureSOs[2]);
            }

            _UnlockedStructuresDict[structure.structureType] = false;
        }

        buildMenuController.RefreshBuildList();
    }

    public void OnCheatCodeCredits(InputAction.CallbackContext context)
    {
        PlayerCredits += 10000;
    }

    #endregion

    #region Build / Remove

    void Build(Vector3Int gridPos)
    {
        // Check Credits
        if (PlayerCredits < structureInHand.tileCost * structureCostMult)
        {
            MessageBanner.Instance.DelayMessage("Not enough Credits to build " + 
                                  structureInHand.tileName + "!", Color.red, 0.1f);
            return;
        }

        if (buildOnlyOneList.Contains(structureInHand))
        {
            MessageBanner.Instance.DelayMessage("You've already built one.  One is the max!", Color.red, 0.1f);
            return;
        }

        // Add Tile
        if (tileManager.AddBlueprintAt(gridPos, structureInHand.blueprintTile, currentRotation))
        {
            PlayerCredits -= Mathf.FloorToInt(structureInHand.tileCost * structureCostMult);
            //RemoveBuildHighlight();

            // Move the Build Menu selection since buildOnlyOne structures are removed from the list
            if (structureInHand.canBuildOnlyOne)
            {
                buildOnlyOneList.Add(structureInHand);
                buildMenuController.ScrollUp();
                buildMenuController.SelectHighlightedStructure();
            }
        }
    }

    public bool CheckSupplies(StructureSO structure)
    {
        if (structure.supplyCost > 0)
        {
            if (SupplyManager.SupplyBalance - structure.supplyCost < 0)
            {
                MessageBanner.Instance.DelayMessage("Not enough Supplies<sprite=2> to build " +
                                      structure.tileName + "!  Build Supply Depots!", Color.red, 0.1f);
                return false;
            }
        }
        else if (structure.supplyCost < 0)
        {
            // Check Supply Limit
            if (SupplyManager.SupplyProduced + structure.supplyCost > SupplyManager.SupplyLimit)
            {
                MessageBanner.Instance.DelayMessage("Supply<sprite=2> Limit reached!  Build more Supply Depots!", Color.red, 0.1f);
                return false;
            }
        }

        return true;
    }

    void RemoveTile(Vector3Int gridPos)
    {
        RemoveBuildHighlight();
        if (tileManager.CheckGridIsClear(gridPos, structureInHand, layersToClearOnBuild, false))
        {
            tileManager.RefundTileAt(gridPos);
            tileManager.RemoveTileAt(gridPos);
        }
        else
        {
            MessageBanner.PulseMessage("Tile not clear for removal", Color.yellow);
            Debug.Log("Tile removal unsuccesful");
        }
    }

    #endregion

    #region Highlight / Preview

    void HighlightTile()
    {
        Vector3Int mouseGridPos = GetMouseToGridPos();
        mouseGridPos.z = 0;
        ActionType currentAction = structureInHand.actionType;
        
        // Replace the highlight position for build or destroy
        
        if (currentAction != ActionType.Build && tileManager.ContainsTileKey(mouseGridPos))
            highlightedPos = tileManager.GridToRootPos(mouseGridPos);
        else 
            highlightedPos = mouseGridPos;

        if (highlightedPos != lastHighlightedPos)
            RemoveBuildHighlight();
        
        // Find player grid position
        Vector2 mouseWorldPos = CursorManager.Instance.GetCustomCursorWorldPos();

        // Return if not in range
        if (!InRange(player.transform.position, mouseWorldPos, buildRange))
        {
            inRange = false;
            allHighlighted = false;
            highlightPositions = new List<Vector3Int>();
            return;
        }

        inRange = true;
        if (currentAction == ActionType.Destroy)
        {
            HighlightForDestroy(highlightedPos);
            
            if (tileManager.ContainsTileKey(highlightedPos))
            {
                Vector3Int rootPos = tileManager.GridToRootPos(highlightedPos);

                Matrix4x4 matrix = animatedTilemap.GetTransformMatrix(highlightedPos);
                int rotation = StructureRotator.GetRotationFromMatrix(matrix);
                TileBase tile = animatedTilemap.GetTile(highlightedPos);
                    
                SetPreviewImageDestroy(rootPos, tile, new Color(1, 0, 0, 0.5f), -rotation);
            }
            lastHighlightedPos = highlightedPos;
            
            return;
        }

        if (currentAction == ActionType.Destroy && !CheckHighlightConditions(
            structureTilemap.GetTile<RuleTileStructure>(highlightedPos), structureInHand))
        { 
            allHighlighted = false;
            lastHighlightedPos = highlightedPos;
            return;
        }

        // Set preview build Image
        if (currentAction == ActionType.Build)
        {
            if (highlightedPos != lastHighlightedPos)
            {
                if (structureInHand.blueprintTile.buildUiTile != null)
                {
                    SetPreviewImageBuild(highlightedPos, 
                                        structureInHand.blueprintTile.buildUiTile, 
                                        new Color(1, 1, 1, 0.8f));
                }
                else
                    SetPreviewImageBuild(highlightedPos, 
                                        structureInHand.blueprintTile.damagedTiles[0], 
                                        new Color(1, 1, 1, 0.8f));
            }
        }

        // Highlight the tiles for building
        allHighlighted = true;
        if (currentAction == ActionType.Build)
        {
            highlightPositions.Clear();

            foreach (Vector3Int pos in structureInHand.cellPositions)
            {                
                if (tileManager.CheckGridIsClear(highlightedPos + pos, layersForBuildClearCheck, true))
                {
                    Vector3Int newPos = new Vector3Int(pos.x, pos.y, -1);
                    highlightTilemap.SetTile(highlightedPos + newPos, highlightedTileAsset);
                    highlightTilemap.SetTileFlags(highlightedPos + newPos, TileFlags.None);
                    highlightTilemap.SetColor(highlightedPos + newPos, new Color(1, 1, 1, 0.5f));
                }
                else
                {
                    // Show X where grid is not clear
                    Vector3Int newPos = new Vector3Int(pos.x, pos.y, -1);
                    highlightTilemap.SetTile(highlightedPos + newPos, blockedTileAsset);
                    allHighlighted = false;
                }

                highlightPositions.Add(highlightedPos + pos);
            }
        }             
        else if (currentAction == ActionType.Select ||
                 currentAction == ActionType.Interact)
        {
            // Do stuff?
        } 
        lastHighlightedPos = highlightedPos;
    }

    void RemoveBuildHighlight()
    {
        //Debug.Log("Removed Build Highlight");
        // Remove preview image
        Vector3Int previewImagePos = new Vector3Int(lastHighlightedPos.x, 
                                        lastHighlightedPos.y, lastHighlightedPos.z);
        previewTilemap.SetTile(previewImagePos, null);
        StructureRotator.ResetTileMatrix(previewTilemap, previewImagePos);

        RemoveDestroyHighlight();

        // Remove build highlight
        foreach (Vector3Int pos in structureInHand.cellPositions)
        {
            Vector3Int newPos = new Vector3Int(pos.x, pos.y, -1);
            highlightTilemap.SetTile(lastHighlightedPos + newPos, null);               
        }
        
        allHighlighted = false;
        lastHighlightedPos += Vector3Int.up * 100;
    }

    void RemoveDestroyHighlight()
    {
        if (destroyPositions.Count > 0)
        {
            foreach (Vector3Int pos in destroyPositions)
            {
                Vector3Int newPos = new Vector3Int(pos.x, pos.y, -1);
                highlightTilemap.SetTile(newPos, null);
            }
            destroyPositions.Clear();
        }
        allHighlighted = false;
    }

    void HighlightForDestroy(Vector3Int gridPos)
    {
        // Find cells for destroying
        if (animatedTilemap.GetTile(gridPos))
            destroyPositions = new List<Vector3Int>(tileManager.GetStructurePositions(animatedTilemap, gridPos));
        else
            destroyPositions = new List<Vector3Int>(tileManager.GetStructurePositions(TileManager.BlueprintTilemap, gridPos));
        if (destroyPositions.Count > 0)
        {
            foreach (Vector3Int pos in destroyPositions)
            {
                Vector3Int newPos = new Vector3Int(pos.x, pos.y, -1);
                highlightTilemap.SetTile(newPos, destroyTileAsset);
            }
            allHighlighted = true;
        }
        else
        {
            allHighlighted = false;
        }
    }

    void SetPreviewImageBuild(Vector3Int gridPos, TileBase tile, Color color)
    {
        Vector3Int imageTilePos = new Vector3Int(gridPos.x, gridPos.y, gridPos.z);
        //Debug.Log($"Setting build preview image at {imageTilePos} with rotation {currentRotation}");
        
        previewTilemap.SetTile(imageTilePos, tile);
        if (structureInHand.structureType == StructureType.Mine)
            StructureRotator.RotateTileAt(previewTilemap, imageTilePos, 0);
        else
        {
            StructureRotator.RotateTileAt(previewTilemap, imageTilePos, currentRotation);
            //Matrix4x4 matrix = previewTilemap.GetTransformMatrix(gridPos); // Need rootPos?
            //tileManager.StartCoroutine(tileManager.RotateTileObject(previewTilemap, gridPos, matrix));
        }
        
        previewTilemap.SetTileFlags(imageTilePos, TileFlags.None);
        previewTilemap.SetColor(imageTilePos, color);

        StartCoroutine(DelayRotateHighlightObject());
    }

    IEnumerator DelayRotateHighlightObject()
    {
        yield return new WaitForFixedUpdate();
        // Rotate structure object
        var obj = previewTilemap.GetInstantiatedObject(highlightedPos);
        if (obj != null)
        {
            obj.transform.rotation = Quaternion.Euler(0, 0, currentRotation);
            //Debug.Log("BuildingSystem: Found tileObject on Rotate, attempting to rotate object");
        }
    }

    void SetPreviewImageDestroy(Vector3Int gridPos, TileBase tile, Color color, int rotation)
    {
        Vector3Int imageTilePos = new Vector3Int(gridPos.x, gridPos.y, gridPos.z);
        //Debug.Log($"Setting destroy preview image at {imageTilePos} with rotation {rotation}");
        
        previewTilemap.SetTile(imageTilePos, tile);
        StructureRotator.RotateTileAt(previewTilemap, imageTilePos, rotation);
        previewTilemap.SetTileFlags(imageTilePos, TileFlags.None);
        previewTilemap.SetColor(imageTilePos, color);  
    }

    #endregion

    #region Checks

    Vector3Int GetMouseToGridPos()
    {
        Vector3 cursorPos = CursorManager.Instance.GetCustomCursorWorldPos();

        Vector3Int mouseCellPos = structureTilemap.WorldToCell(cursorPos);
        mouseCellPos.z = 0;

        return mouseCellPos;
    }

    bool InRange(Vector2 positionA, Vector2 positionB, float radius)
    {
        // Calculate the squared distance between positionA and positionB
        float distanceSquared = (positionA.x - positionB.x) * (positionA.x - positionB.x) +
                                (positionA.y - positionB.y) * (positionA.y - positionB.y);
    
    // Compare the squared distance with the squared radius
    return distanceSquared <= radius * radius;
    }

    bool CheckHighlightConditions(RuleTileStructure mousedTile, StructureSO structureInHand)
    {
        if (structureInHand.actionType == ActionType.Select ||
            structureInHand.actionType == ActionType.Destroy)
            if (mousedTile)
                return true;
            else return false;

        else if (structureInHand.actionType == ActionType.Build)
            return true;
                    
        return false;
    }

    #endregion
}

