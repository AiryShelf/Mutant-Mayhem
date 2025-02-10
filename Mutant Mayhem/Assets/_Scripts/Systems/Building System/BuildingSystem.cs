using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public List<StructureSO> AllStructureSOs;
    [SerializeField] List<bool> unlockedStructuresStart;
    public StructureSO structureInHand;
    public static event Action<float> OnPlayerCreditsChanged;
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
    public LineRendererCircle buildRangeCircle;
    public LayerMask layersForBuildClearCheck;
    [SerializeField] LayerMask layersToClearOnBuild;
    public UIBuildMenuController buildMenuController;
    [SerializeField] QCubeController qCubeController;
    [SerializeField] MouseLooker mouseLooker;
    [SerializeField] CameraController cameraController;

    [Header("Tilemaps")]
    [SerializeField] TileManager tileManager;
    [SerializeField] Tilemap structureTilemap;
    [SerializeField] Tilemap animatedTilemap;
    [SerializeField] Tilemap highlightTilemap;
    [SerializeField] Tilemap previewTilemap;
    [SerializeField] TileBase highlightedTileAsset;
    [SerializeField] TileBase destroyTileAsset;
    [SerializeField] TileBase blockedTileAsset;
    [SerializeField] GameObject debugDotPrefab;

    [Header("Dynamic vars, don't set here")]
    public float structureCostMult = 1;

    public static Dictionary<StructureType, bool> _UnlockedStructuresDict = 
                                        new Dictionary<StructureType, bool>();
    public bool isInBuildMode;
    public int currentRotation;
    private Vector3Int highlightedTilePos;
    public bool allHighlighted;
    bool inRange;
    Player player;
    InputActionMap playerActionMap;
    InputAction toolbarAction;
    InputAction rotateStructureAction;
    InputAction buildAction;
    InputAction cheatCodeCreditsAction;
    List<Vector3Int> destroyPositions = new List<Vector3Int>();
    TurretManager turretManager;

    Coroutine clearStructureInHand;
    public GameObject lastSelectedUiObject;
    public StructureSO lastStructureInHand;

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

    void OnBuild()
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
                Build(highlightedTilePos);
                //StartCoroutine(DelayUIReselect());
            }
            else if (structureInHand.actionType == ActionType.Destroy)
            {
                RemoveTile(destroyPositions[0]);
                //StartCoroutine(DelayUIReselect());
            }
        }
        // Messages for failed to build or destroy
        else if (inRange)
        {
            if (structureInHand.actionType == ActionType.Build)
                //MessagePanel.Instance.DelayMessage("Area not clear for building", Color.yellow, 0.1f);
            if (structureInHand.actionType == ActionType.Destroy)
                MessagePanel.Instance.DelayMessage("Unable to destroy", Color.yellow, 0.1f);
            //StartCoroutine(DelayUIReselect());
        }
        else
        {
            if (structureInHand.actionType == ActionType.Build)
                MessagePanel.Instance.DelayMessage("Too far away to build", Color.yellow, 0.1f);
            if (structureInHand.actionType == ActionType.Destroy)
                MessagePanel.Instance.DelayMessage("Too far away to destroy", Color.yellow, 0.1f);
            //StartCoroutine(DelayUIReselect());
        }
    }

    public IEnumerator DelayUIReselect()
    {
        yield return new WaitForFixedUpdate();
        if (lastSelectedUiObject != null)
            EventSystem.current.SetSelectedGameObject(lastSelectedUiObject);
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

        Debug.Log(context.control.name);

        // Normalize the rotation to be within the range (0, 360)
        currentRotation = (currentRotation % 360 + 360) % 360;
        Rotate(structureInHand.ruleTileStructure.structureSO);
    }

    void Rotate(StructureSO structure)
    {
        RemoveBuildHighlight();
        // Ensure use of original SO cell positions for rotation
        if (AllStructureSOs.Contains(structure))
            structure = AllStructureSOs[AllStructureSOs.IndexOf(structure)];
        else
            return;
        structureInHand = StructureRotator.RotateStructure(structure, currentRotation);
        //lastStructureInHand = structureInHand;
    }

    public void SwapWithDestroyTool(InputAction.CallbackContext context)
    {
        if (!isInBuildMode)
            return;

        if (Input.GetKeyDown("x"))
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

    public void ToggleBuildMenu(bool on)
    {
        //Debug.Log("ToggleBuildMenu Called");

        if (on)
        {
            qCubeController.CloseUpgradeWindow();
            
            CursorManager.Instance.SetBuildCursor();
            if (InputController.LastUsedDevice == Gamepad.current)
            {
                InputController.SetJoystickMouseControl(true);
                Debug.Log("Joystick turned on from BuildingSystem");
            }

            // Lock camera to player
            cameraController.ZoomAndFocus(player.transform, 0, 0.25f, 0.5f, true, false);
            mouseLooker.lockedToPlayer = true;

            buildRangeCircle.EnableCircle(true);
            isInBuildMode = true;
            player.playerShooter.isBuilding = true;
            buildMenuController.OpenBuildMenu(true);
            
            //Debug.Log("Opened Build Panel");
            structureInHand = lastStructureInHand;
            
            StartCoroutine(DelayMenuSelection()); // So that FadeCanvasGroupsWave can turn the elements on
        }
        else
        {
            CursorManager.Instance.SetAimCursor();
            InputController.SetJoystickMouseControl(false);
            Debug.Log("Joystick turned off from BuildingSystem");

            // Unlock camera from player
            cameraController.ZoomAndFocus(player.transform, 0, 1, 1, false, false);
            mouseLooker.lockedToPlayer = false;

            buildRangeCircle.EnableCircle(false);
            isInBuildMode = false;
            player.playerShooter.isBuilding = false;
            // Only switch guns if not repair gun
            //if (previousGunIndex != 9)
                //player.playerShooter.SwitchGuns(previousGunIndex);
            buildMenuController.OpenBuildMenu(false);
            
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

    public void UnlockStructures(List<StructureType> structures)
    {
        foreach (StructureType structure in structures)
            _UnlockedStructuresDict[structure] = true;

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
        // Check Turrets
        if (structureInHand.isTurret)
        {
            if (turretManager.currentNumTurrets >= player.stats.structureStats.maxTurrets)
            {
                MessagePanel.Instance.DelayMessage("Turret limit reached.  Use Structure "+
                                      "upgrades to increase the limit", Color.red, 0.1f);
                return;
            }
        }

        // Check Credits
        if (PlayerCredits < structureInHand.tileCost * structureCostMult)
        {
            MessagePanel.Instance.DelayMessage("Not enough Credits to build " + 
                                  structureInHand.tileName + "!", Color.red, 0.1f);
            return;
        }

        // Add Tile
        if (tileManager.AddBlueprintAt(gridPos, structureInHand.blueprintTile, currentRotation))
        {
            PlayerCredits -= structureInHand.tileCost * structureCostMult;
            RemoveBuildHighlight();

            if (structureInHand.isTurret)
            {
                turretManager.currentNumTurrets++;
            }

            AddToStatCounter();
        }
        else
            MessagePanel.Instance.DelayMessage("Unable to build there.  It's blocked!", Color.red, 0.1f);
    }

    void AddToStatCounter()
    {
        StatsCounterPlayer.StructuresBuilt++;

        if (structureInHand.structureType == StructureType.OneByFourWall || 
            structureInHand.structureType == StructureType.OneByOneCorner || 
            structureInHand.structureType == StructureType.OneByOneWall || 
            structureInHand.structureType == StructureType.TwoByEightWall ||
            structureInHand.structureType == StructureType.TwoByTwoCorner || 
            structureInHand.structureType == StructureType.TwoByTwoWall)
        {
            StatsCounterPlayer.WallsBuilt++;
        }
        else if (structureInHand.structureType == StructureType.Door || 
                 structureInHand.structureType == StructureType.BlastDoor)
        {
            StatsCounterPlayer.DoorsBuilt++;
        }
        else if (structureInHand.structureType == StructureType.LaserTurret ||
                 structureInHand.structureType == StructureType.GunTurret)
        {
            StatsCounterPlayer.TurretsBuilt++;
        }
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
            MessagePanel.PulseMessage("Tile not clear for removal", Color.yellow);
            Debug.Log("Tile removal unsuccesful");
        }
    }

    #endregion

    #region Highlight / Preview

    private void HighlightTile()
    {
        Vector3Int mouseGridPos = GetMouseToGridPos();
        mouseGridPos.z = 0;
        ActionType currentAction = structureInHand.actionType;
        
        // Replace the highlight position for build or destroy
        RemoveBuildHighlight();
        if (currentAction != ActionType.Build && tileManager.ContainsTileKey(mouseGridPos))
            highlightedTilePos = tileManager.GridToRootPos(mouseGridPos);
        else 
            highlightedTilePos = mouseGridPos;

        // Find player grid position
        //playerGridPos = structureTilemap.WorldToCell(player.transform.position);
        Vector2 mouseWorldPos = CursorManager.Instance.GetCustomCursorWorldPos();

        // Highlight if in range and conditions met.
        //if (InRange(playerGridPos, mouseGridPos, (Vector3Int) structureInHand.actionRange))
        if (!InRange(player.transform.position, mouseWorldPos, 6f))
        {
            inRange = false;
            allHighlighted = false;
            return;
        }

        inRange = true;
        if (currentAction == ActionType.Destroy)
        {
            HighlightForDestroy(highlightedTilePos);
            
            if (tileManager.ContainsTileKey(highlightedTilePos))
            {
                Vector3Int rootPos = tileManager.GridToRootPos(highlightedTilePos);

                Matrix4x4 matrix = animatedTilemap.GetTransformMatrix(highlightedTilePos);
                int rotation = StructureRotator.GetRotationFromMatrix(matrix);
                TileBase tile = animatedTilemap.GetTile(highlightedTilePos);
                    
                SetPreviewImageDestroy(rootPos, tile, new Color(1, 0, 0, 0.5f), -rotation);
            }
            
            return;
        }

        if (currentAction == ActionType.Destroy && !CheckHighlightConditions(
            structureTilemap.GetTile<RuleTileStructure>(highlightedTilePos), structureInHand))
        { 
            allHighlighted = false;
            return;
        }

        // Set preview build Image
        if (currentAction == ActionType.Build)
        {
            if (structureInHand.ruleTileStructure.buildUiTile != null)
            {
                SetPreviewImageBuild(highlightedTilePos, 
                                    structureInHand.ruleTileStructure.buildUiTile, 
                                    new Color(1, 1, 1, 0.8f));
            }
            else
                SetPreviewImageBuild(highlightedTilePos, 
                                    structureInHand.ruleTileStructure.damagedTiles[0], 
                                    new Color(1, 1, 1, 0.8f));
        }

        // Highlight the tiles for building
        allHighlighted = true;
        if (currentAction == ActionType.Build)
        {
            foreach (Vector3Int pos in structureInHand.cellPositions)
            {                
                if (tileManager.CheckGridIsClear(highlightedTilePos + pos, layersForBuildClearCheck, true))
                {
                    Vector3Int newPos = new Vector3Int(pos.x, pos.y, -1);
                    highlightTilemap.SetTile(highlightedTilePos + newPos, highlightedTileAsset);
                    highlightTilemap.SetTileFlags(highlightedTilePos + newPos, TileFlags.None);
                    highlightTilemap.SetColor(highlightedTilePos + newPos, new Color(1, 1, 1, 0.5f));
                }
                else
                {
                    // Show X where grid is not clear
                    Vector3Int newPos = new Vector3Int(pos.x, pos.y, -1);
                    highlightTilemap.SetTile(highlightedTilePos + newPos, blockedTileAsset);
                    allHighlighted = false;
                }   
            }
        }             
        else if (currentAction == ActionType.Select ||
                 currentAction == ActionType.Interact)
        {
            // Do stuff
        } 
    }

    void RemoveBuildHighlight()
    {
        // Remove preview image
        Vector3Int previewImagePos = new Vector3Int(highlightedTilePos.x, 
                                        highlightedTilePos.y, highlightedTilePos.z);
        previewTilemap.SetTile(previewImagePos, null);
        StructureRotator.ResetTileMatrix(previewTilemap, previewImagePos);

        RemoveDestroyHighlight();

        // Remove build highlight
        foreach (Vector3Int pos in structureInHand.cellPositions)
        {
            Vector3Int newPos = new Vector3Int(pos.x, pos.y, -1);
            highlightTilemap.SetTile(highlightedTilePos + newPos, null);               
        }
        
        allHighlighted = false;
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
            destroyPositions = new List<Vector3Int>(tileManager.GetStructurePositions(tileManager.blueprintTilemap, gridPos));
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
        StructureRotator.RotateTileAt(previewTilemap, imageTilePos, currentRotation);
        previewTilemap.SetTileFlags(imageTilePos, TileFlags.None);
        previewTilemap.SetColor(imageTilePos, color);
    }

    void SetPreviewImageDestroy(Vector3Int gridPos, TileBase tile, Color color, int rotation)
    {
        // PROBLEMS HERE
        Vector3Int imageTilePos = new Vector3Int(gridPos.x, gridPos.y, gridPos.z);
        //Debug.Log($"Setting destroy preview image at {imageTilePos} with rotation {rotation}");
        
        previewTilemap.SetTile(imageTilePos, tile);
        StructureRotator.RotateTileAt(previewTilemap, imageTilePos, rotation);
        previewTilemap.SetTileFlags(imageTilePos, TileFlags.None);
        previewTilemap.SetColor(imageTilePos, color);  
    }

    #endregion

    #region Checks

    private Vector3Int GetMouseToGridPos()
    {
        //Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mousePos = CursorManager.Instance.GetCustomCursorWorldPos();
        Vector3Int mouseCellPos = structureTilemap.WorldToCell(mousePos);
        mouseCellPos.z = 0;

        return mouseCellPos;
    }

    private bool InRange(Vector2 positionA, Vector2 positionB, float radius)
    {
        // Calculate the squared distance between positionA and positionB
        float distanceSquared = (positionA.x - positionB.x) * (positionA.x - positionB.x) +
                                (positionA.y - positionB.y) * (positionA.y - positionB.y);
    
    // Compare the squared distance with the squared radius
    return distanceSquared <= radius * radius;
    }

    private bool CheckHighlightConditions(RuleTileStructure mousedTile, StructureSO structureInHand)
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

