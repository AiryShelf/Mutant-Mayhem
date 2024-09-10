using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public List<StructureSO> AllStructureSOs;
    [SerializeField] List<bool> unlockedStructuresStart;
    public StructureSO structureInHand;
    public static float PlayerCredits;
    [SerializeField] float playerStartingCredits;
    public BuildRangeCircle buildRangeCircle;
    [SerializeField] LayerMask layersForBuildClearCheck;
    [SerializeField] LayerMask layersForRemoveClearCheck;
    [SerializeField] TurretManager turretManager;
    [SerializeField] UIBuildMenuController buildMenuController;
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

    public static Dictionary<Structure, bool> _UnlockedStructuresDict = 
                                        new Dictionary<Structure, bool>();
    public bool inBuildMode;
    public int currentRotation;
    private Vector3Int highlightedTilePos;
    public bool allHighlited;
    bool inRange;
    Player player;
    InputActionMap playerActionMap;
    InputAction toolbarAction;
    InputAction rotateStructureAction;
    InputAction buildAction;
    InputAction cheatCodeCreditsAction;
    List<Vector3Int> destroyPositions = new List<Vector3Int>();
    MessagePanel messagePanel;

    Coroutine clearSelection;
    GameObject lastSelectedObject;

    void Awake()
    {
        BuildStructsAvailDict();    

        player = FindObjectOfType<Player>();
        PlayerCredits = playerStartingCredits;
        messagePanel = FindObjectOfType<MessagePanel>();
    }

    void OnEnable()
    {
        playerActionMap = player.inputAsset.FindActionMap("Player");
        toolbarAction = playerActionMap.FindAction("Toolbar");
        buildAction = playerActionMap.FindAction("BuildStructure");
        rotateStructureAction = playerActionMap.FindAction("RotateStructure");
        cheatCodeCreditsAction = playerActionMap.FindAction("CheatCodeCredits");

        rotateStructureAction.started += OnRotate;
        toolbarAction.started += OnToolbarUsed;
        buildAction.started += OnBuild;
        cheatCodeCreditsAction.started += OnCheatCodeCredits;

    }

    void OnDisable()
    {
        toolbarAction.started -= OnToolbarUsed;
        rotateStructureAction.started -= OnRotate;
        buildAction.started -= OnBuild;
        cheatCodeCreditsAction.started -= OnCheatCodeCredits;
   
        _UnlockedStructuresDict.Clear();   
    }

    void FixedUpdate()
    {
        // Highlight a tile
        if (structureInHand != null)
        {
            HighlightTile();
        }
    }

    void Update()
    {
        // Keep track of currently selected object
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            lastSelectedObject = EventSystem.current.currentSelectedGameObject;
        }        
    }

    #region Inputs

    void OnBuild(InputAction.CallbackContext context)
    {
        if (!inBuildMode)
        {
            return;
        }

        // Prevent mouse input from deselecting UI element **THIS DOESNT APPEAR TO WORK**
        if (Input.GetMouseButtonDown(0))
        {
            // Check if the pointer is over a UI element
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (lastSelectedObject != null)
                {
                    StartCoroutine(DelayUIReselect());
                }
            }
        }

        // Build
        if (allHighlited)
        {   
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (structureInHand.actionType == ActionType.Build)
                {
                    Build(highlightedTilePos);
                }
                else if (structureInHand.actionType == ActionType.Destroy)
                {
                    RemoveTile(destroyPositions[0]);
                }
            }
        }
        // Failed to build or destroy messages
        else if (inRange)
        {
            if (structureInHand.actionType == ActionType.Build)
                messagePanel.ShowMessage("Area not clear for building", Color.yellow);
            if (structureInHand.actionType == ActionType.Destroy)
                messagePanel.ShowMessage("Unable to destroy", Color.yellow);
        }
        else
        {
            if (structureInHand.actionType == ActionType.Build)
                messagePanel.ShowMessage("Too far away to build", Color.yellow);
            if (structureInHand.actionType == ActionType.Destroy)
                messagePanel.ShowMessage("Too far away to destroy", Color.yellow);
        }
    }

    IEnumerator DelayUIReselect()
    {
        yield return new WaitForFixedUpdate();
        EventSystem.current.SetSelectedGameObject(lastSelectedObject);
    }

    void OnRotate(InputAction.CallbackContext context)
    {
        if (context.control.name == "q")
        {
            currentRotation += 90;
        }   
        else if (context.control.name == "e")
        {
            currentRotation -= 90;
        }
        // Normalize the rotation to be within the range (0, 360)
        currentRotation = (currentRotation % 360 + 360) % 360;
        Rotate(structureInHand.ruleTileStructure.structureSO);
    }

    void Rotate(StructureSO structure)
    {
        RemoveBuildHighlight();
        // ensure use of original cell positions for rotation
        structure = AllStructureSOs[AllStructureSOs.IndexOf(structure)];
        structureInHand = StructureRotator.RotateStructure(structure, currentRotation);
    }

    void OnToolbarUsed(InputAction.CallbackContext context)
    {
        if (inBuildMode)
        {
            //Debug.Log("OnToolbar called");
            if (Input.GetKeyDown("c"))
            {
                //Repair Tool
                ChangeStructureInHand(AllStructureSOs[1]);
            }
            else if (Input.GetKeyDown("x"))
            {
                Debug.Log("Switched to destroy tool");
                //Destroy Tool
                ChangeStructureInHand(AllStructureSOs[2]);
            }
            else
            {
                //structureInHand = AllStructureSOs[(int)Structure.SelectTool];
                //RemoveBuildHighlight();
            }
        }
    }

    #endregion

    #region Build Menu and Unlock

    public void ToggleBuildMenu(bool on)
    {
        //Debug.Log("ToggleBuildMenu Called");

        if (on)
        {
            CursorManager.Instance.SetBuildCursor();

            // Lock camera to player
            cameraController.ZoomAndFocus(player.transform, 0, 1, 0.5f, true, false);
            mouseLooker.lockedToPlayer = true;

            //currentRotation = 0;
            buildRangeCircle.EnableBuildCircle(true);
            inBuildMode = true;
            //previousGunIndex = player.playerShooter.currentGunIndex;
            player.playerShooter.isBuilding = true;
            //player.playerShooter.SwitchGuns(9);
            buildMenuController.TriggerFadeGroups(true);
            qCubeController.CloseUpgradeWindow();
            //Debug.Log("Opened Build Panel");
        }
        else
        {
            // If not holding repair gun, set aim cursor
            if (player.playerShooter.currentGunIndex != 9)
                CursorManager.Instance.SetAimCursor();

            // Unlock camera from player
            cameraController.ZoomAndFocus(player.transform, 0, 1, 1, false, false);
            mouseLooker.lockedToPlayer = false;

            buildRangeCircle.EnableBuildCircle(false);
            inBuildMode = false;
            player.playerShooter.isBuilding = false;
            // Only switch guns if not repair gun
            //if (previousGunIndex != 9)
                //player.playerShooter.SwitchGuns(previousGunIndex);
            buildMenuController.TriggerFadeGroups(false);
            
            float time = buildMenuController.fadeCanvasGroups.fadeOutAllTime;

            if (clearSelection != null)
                StopCoroutine(clearSelection);
            clearSelection = StartCoroutine(ClearSelection(time));

            RemoveBuildHighlight();
            structureInHand = AllStructureSOs[(int)Structure.SelectTool];
            //Debug.Log("Closed Build Panel");
        }
    }

    IEnumerator ClearSelection(float time)
    {
        yield return new WaitForSeconds(time);

        RemoveBuildHighlight();
        structureInHand = AllStructureSOs[(int)Structure.SelectTool];
    }

    public void ChangeStructureInHand(StructureSO structure)
    {
        Rotate(structure);
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

    public void UnlockStructure(Structure structure)
    {
        _UnlockedStructuresDict[structure] = true;
    }

    public void OnCheatCodeCredits(InputAction.CallbackContext context)
    {
        PlayerCredits += 10000;
    }

    #endregion

    #region Build and Remove

    void Build(Vector3Int gridPos)
    {
        // Check Turrets
        if (structureInHand.isTurret)
        {
            if (turretManager.numTurrets >= player.stats.structureStats.maxTurrets)
            {
                messagePanel.ShowMessage("Turret limit reached.  Use upgrades to increase the limit", Color.red);
                return;
            }
        }

        // Check Credits
        if (PlayerCredits < structureInHand.tileCost)
        {
            messagePanel.ShowMessage("Not enough Credits to build " + 
                                     structureInHand.tileName + "!", Color.red);
            return;
        }

        // Add Tile
        if (tileManager.AddTileAt(gridPos, structureInHand, currentRotation))
        {
            PlayerCredits -= structureInHand.tileCost;
            RemoveBuildHighlight();

            if (structureInHand.isTurret)
            {
                turretManager.AddTurret(gridPos);
            }
        }   
    }

    void RemoveTile(Vector3Int gridPos)
    {
        RemoveBuildHighlight();
        if (tileManager.CheckGridIsClear(gridPos, structureInHand, layersForRemoveClearCheck, false))
        {
            
            tileManager.RefundTileAt(gridPos);
            tileManager.RemoveTileAt(gridPos);
        }
        else
        {
            messagePanel.ShowMessage("Tile not clear for removal", Color.yellow);
            Debug.Log("Tile removal blocked");
        }
    }

    #endregion

    #region Highlight and Preview

    private void HighlightTile()
    {
        Vector3Int mouseGridPos = GetMouseToGridPos();
        mouseGridPos.z = 0;
        ActionType currentAction = structureInHand.actionType;
        
        // Replace the highlight
        RemoveBuildHighlight();
        if (currentAction != ActionType.Build && tileManager.ContainsTileDictKey(mouseGridPos))
            highlightedTilePos = tileManager.GetRootPos(mouseGridPos);
        else 
            highlightedTilePos = mouseGridPos;

        // Find player grid position
        //playerGridPos = structureTilemap.WorldToCell(player.transform.position);
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Highlight if in range and conditions met.
        //if (InRange(playerGridPos, mouseGridPos, (Vector3Int) structureInHand.actionRange))
        if (!InRange(player.transform.position, mouseWorldPos, 6f))
        {
            inRange = false;
            allHighlited = false;
            return;
        }

        inRange = true;
        if (currentAction == ActionType.Destroy)
        {
            HighlightForDestroy(highlightedTilePos);
            
            if (tileManager.ContainsTileDictKey(highlightedTilePos))
            {
                Vector3Int rootPos = tileManager.GetRootPos(highlightedTilePos);

                // Probably not finding the tile
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
            allHighlited = false;
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
        allHighlited = true;
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
                    highlightTilemap.SetTile(highlightedTilePos + newPos, destroyTileAsset);
                    allHighlited = false;
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
        
        allHighlited = false;
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
        allHighlited = false;
    }

    void HighlightForDestroy(Vector3Int gridPos)
    {
        // Find cells for destroying
        destroyPositions = new List<Vector3Int>(tileManager.GetStructurePositions(animatedTilemap, gridPos));
        if (destroyPositions.Count > 0)
        {
            foreach (Vector3Int pos in destroyPositions)
            {
                Vector3Int newPos = new Vector3Int(pos.x, pos.y, -1);
                highlightTilemap.SetTile(newPos, destroyTileAsset);
            }
            allHighlited = true;
        }
        else
        {
            allHighlited = false;
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
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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

