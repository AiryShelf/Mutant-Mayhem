using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public static float PlayerCredits;
    [SerializeField] float playerStartingCredits;
    public StructureSO structureInHand;
    public LayerMask layersForBuildClearCheck;
    public LayerMask layersForRemoveClearCheck;
    [SerializeField] TileBase highlightedTileAsset;
    [SerializeField] TileBase destroyTileAsset;
    [SerializeField] Tilemap structureTilemap;
    [SerializeField] Tilemap animatedTilemap;
    [SerializeField] Tilemap highlightTilemap;
    [SerializeField] Tilemap previewTilemap;
    [SerializeField] TileManager tileManager;
    [SerializeField] UIBuildMenuController buildMenuController;
    public List<StructureSO> AllStructureSOs;
    [SerializeField] int numStructuresAvailAtStart;

    [SerializeField] QCubeController qCubeController;

    public bool inBuildMode;
    public int currentRotation;
    
    // needs to actually be created
    public static Dictionary<StructureType, bool> _StructsAvailDict = 
                                        new Dictionary<StructureType, bool>();

    Vector3Int playerGridPos;
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
   
        _StructsAvailDict.Clear();   
    }

    void FixedUpdate()
    {
        // Highlight a tile
        if (structureInHand != null)
        {
            HighlightTile();
        }
    }

    #region Controls

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

        // Normalize the rotation to be within the range [0, 360)
        currentRotation = (currentRotation % 360 + 360) % 360;
        SwitchTools(structureInHand.structureType);
    }

    void OnBuild(InputAction.CallbackContext context)
    {
        if (allHighlited)
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
        else if (inRange)
        {
            messagePanel.ShowMessage("Tile not clear for building", Color.yellow);
        }
        else
        {
            messagePanel.ShowMessage("Too far away to build", Color.yellow);
        }
    }

    public void ToggleBuildMenu(bool on)
    {
        //Debug.Log("ToggleBuildMenu Called");

        if (on)
        {
            inBuildMode = true;
            player.playerShooter.isBuilding = true;
            buildMenuController.TriggerFadeGroups(true);
            qCubeController.CloseUpgradeWindow();
            //Debug.Log("Opened Build Panel");
        }
        else
        {
            inBuildMode = false;
            player.playerShooter.isBuilding = false;
            buildMenuController.TriggerFadeGroups(false);
            
            float time = buildMenuController.fadeCanvasGroups.fadeOutAllTime;

            if (clearSelection != null)
                StopCoroutine(clearSelection);
            clearSelection = StartCoroutine(ClearSelection(time));

            RemoveBuildHighlight();
            structureInHand = AllStructureSOs[(int)StructureType.SelectTool];
            
            //Debug.Log("Closed Build Panel");
        }
    }

    IEnumerator ClearSelection(float time)
    {
        yield return new WaitForSeconds(time);

        RemoveBuildHighlight();
        structureInHand = AllStructureSOs[(int)StructureType.SelectTool];
    }

    void OnToolbarUsed(InputAction.CallbackContext context)
    {
        if (inBuildMode)
        {
            //Debug.Log("OnToolbar called");
            if (Input.GetKeyDown("c"))
            {
                //Repair Tool
                SwitchTools(StructureType.RepairTool);
            }
            else if (Input.GetKeyDown("x"))
            {
                Debug.Log("switch to destroy tool");
                //Destroy Tool
                SwitchTools(StructureType.DestroyTool);
            }
            else
            {
                RemoveBuildHighlight();
                structureInHand = AllStructureSOs[(int)StructureType.SelectTool];
            }
        }
    }

    public void SwitchTools(StructureType structure)
    {
        // if the bool in the dict is true, its avialable to use
        if (_StructsAvailDict[structure])
        {
            RemoveBuildHighlight();

            StructureSO sourceSO = AllStructureSOs[(int)structure];  

            // Apply current Rotation to tiles, not tools
            if ((int) structure > 2)
            {
                structureInHand = StructureRotator.RotateStructure(sourceSO, currentRotation);
            }
            else
            {
                structureInHand = sourceSO;
            }


            //Debug.Log("Switched to tool index: " + (int)structure);     
        }
    }

    public void OnCheatCodeCredits(InputAction.CallbackContext context)
    {
        PlayerCredits += 10000;
    }

    #endregion

    #region Build and Remove

    void Build(Vector3Int gridPos)
    {
        if (PlayerCredits >= structureInHand.tileCost)
        {
            if (tileManager.AddTileAt(gridPos, structureInHand, currentRotation))
            {
                PlayerCredits -= structureInHand.tileCost;
                RemoveBuildHighlight();
            }
        }
        else
        {
            messagePanel.ShowMessage("Not enough Credits to build " + 
                                     structureInHand.tileName + "!", Color.red);
        }
    }

    void RemoveTile(Vector3Int gridPos)
    {
        RemoveBuildHighlight();
        if (tileManager.CheckGridIsClear(gridPos, structureInHand, layersForRemoveClearCheck, false))
        {
            
            tileManager.RefundTileAt(gridPos);
            tileManager.DestroyTileAt(gridPos);
        }
        else
        {
            messagePanel.ShowMessage("Tile not clear for removal", Color.yellow);
            Debug.Log("Tile removal blocked");
        }
    }

    void BuildStructsAvailDict()
    {
        int available = numStructuresAvailAtStart;
        foreach(StructureSO structure in AllStructureSOs)
        {           
            if (available != 0)
            {
                _StructsAvailDict.Add(structure.structureType, true);
                available--;
            }
            else
                _StructsAvailDict.Add(structure.structureType, false);
        }
    }

    #endregion

    #region Highlights and Preview Image

    private void HighlightTile()
    {
        Vector3Int mouseGridPos = GetMouseToGridPos();
        ActionType currentAction = structureInHand.actionType;
        
        // Replace the highlight
        RemoveBuildHighlight();
        if (currentAction != ActionType.Build && tileManager.ContainsTileDictKey(mouseGridPos))
            highlightedTilePos = tileManager.GetRootPos(mouseGridPos);
        else 
            highlightedTilePos = mouseGridPos;

        // Find player grid position
        playerGridPos = structureTilemap.WorldToCell(player.transform.position);

        // Highlight if in range and conditions met.
        if (InRange(playerGridPos, mouseGridPos, (Vector3Int) structureInHand.actionRange))
        {
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

            if (currentAction != ActionType.Destroy && CheckHighlightConditions(
                structureTilemap.GetTile<RuleTileStructure>(highlightedTilePos), structureInHand))
            { 
                // Set preview build Image
                if (currentAction == ActionType.Build)
                {
                    SetPreviewImageBuild(highlightedTilePos, 
                                         structureInHand.ruleTileStructure.damagedTiles[0], 
                                         new Color(1, 1, 1, 0.5f));
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
                            highlightTilemap.SetColor(highlightedTilePos + newPos, new Color(1, 1, 1, 1f));
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
            else
            {
                allHighlited = false;
            }
        }
        else
        {
            inRange = false;
            allHighlited = false;
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

    private bool InRange(Vector3Int positionA, Vector3Int positionB, Vector3Int range)
    {
        Vector3Int distance = positionA - positionB;

        // Thsi code allows for different x and y ranges.
        if (Mathf.Abs(distance.x) >= range.x || Mathf.Abs(distance.y) >= range.y)
        {
            return false;
        }

        return true;
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

