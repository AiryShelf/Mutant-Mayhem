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
    [SerializeField] TileBase highlightedTileAsset;
    [SerializeField] TileBase destroyTileAsset;
    [SerializeField] Tilemap structureTilemap;
    [SerializeField] Tilemap tempTilemap;
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
    Player player;
    InputActionMap playerActionMap;
    InputAction toolbarAction;
    InputAction rotateStructureAction;
    InputAction buildAction;
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

        rotateStructureAction.started += OnRotate;
        toolbarAction.started += OnToolbarUsed;
        buildAction.started += OnBuild;
    }

    void OnDisable()
    {
        toolbarAction.started -= OnToolbarUsed;
        rotateStructureAction.started -= OnRotate;
        buildAction.started -= OnBuild;
   
        _StructsAvailDict.Clear();   
    }

    void FixedUpdate()
    {
        // Highlight a tile
        if (structureInHand != null)
        {
            HighlightTile(structureInHand);
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
                DestroyTile(destroyPositions[0]);
            }
        }
    }

    public void ToggleBuildMenu(bool on)
    {
        //Debug.Log("ToggleBuildMenu Called");

        if (on)
        {
            inBuildMode = true;
            buildMenuController.TriggerFadeGroups(true);
            qCubeController.CloseUpgradeWindow();
            //Debug.Log("Opened Build Panel");
        }
        else
        {
            inBuildMode = false;
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


            Debug.Log("Switched to tool index: " + (int)structure);     
        }
    }

    #endregion

    #region Build and Destroy

    void Build(Vector3Int gridPos)
    {
        if (PlayerCredits >= structureInHand.tileCost)
        {
            if (tileManager.AddTileAt(gridPos, structureInHand, currentRotation))
            {
                RemoveBuildHighlight();
                PlayerCredits -= structureInHand.tileCost;
            }
        }
        else
        {
            messagePanel.ShowMessage("Not enough Credits to build " + 
                                     structureInHand.tileName + "!", Color.red);
        }
    }

    void DestroyTile(Vector3Int gridPos)
    {
        RemoveBuildHighlight();
        tileManager.DestroyTileAt(gridPos);
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

    void RemoveBuildHighlight()
    {
        // Remove preview image
        Vector3Int previewImagePos = new Vector3Int(highlightedTilePos.x, 
                                        highlightedTilePos.y, highlightedTilePos.z + 1);
        tempTilemap.SetTile(previewImagePos, null);
        StructureRotator.ResetTileMatrix(tempTilemap, previewImagePos);

        RemoveDestroyHighlight();

        // Remove build highlight
        foreach (Vector3Int pos in structureInHand.cellPositions)
        {
            tempTilemap.SetTile(highlightedTilePos + pos, null);               
        }
        
        allHighlited = false;
    }

    void RemoveDestroyHighlight()
    {
        if (destroyPositions.Count > 0)
        {
            foreach (Vector3Int pos in destroyPositions)
            {
                tempTilemap.SetTile(pos, null);
            }
            destroyPositions.Clear();
        }
        allHighlited = false;
    }

    private void HighlightTile(StructureSO structureInHand)
    {
        Vector3Int mouseGridPos = GetMouseToGridPos();
        ActionType currentAction = structureInHand.actionType;
        
        // Replace the highlight
        RemoveBuildHighlight();
        if (currentAction != ActionType.Build && tileManager.ContainsTileDictKey(mouseGridPos))
            highlightedTilePos = tileManager.GetRootPos(mouseGridPos);
        else highlightedTilePos = mouseGridPos;

        // Find player grid position
        playerGridPos = structureTilemap.WorldToCell(player.transform.position);

        // Highlight if in range and conditions met.
        if (InRange(playerGridPos, mouseGridPos, (Vector3Int) structureInHand.actionRange))
        {
            if (currentAction == ActionType.Destroy)
            {
                HighlightForDestroy(highlightedTilePos);
                
                if (tileManager.ContainsTileDictKey(highlightedTilePos))
                {
                    Vector3Int rootPos = tileManager.GetRootPos(highlightedTilePos);
                    AnimatedTile rootAnimTile = tileManager.GetRootAnimTile(rootPos);
                    SetPreviewImage(rootPos, rootAnimTile, new Color(1, 0, 0, 0.5f));
                }
                
                return;
            }

            if (currentAction != ActionType.Destroy && CheckHighlightConditions(
                structureTilemap.GetTile<RuleTileStructure>(highlightedTilePos), structureInHand))
            { 
                // Set preview build Image
                if (currentAction == ActionType.Build)
                {
                    SetPreviewImage(highlightedTilePos, 
                        structureInHand.ruleTileStructure.damagedTiles[0], new Color(1, 1, 1, 0.5f));
                }
                
                // Highlight the tiles for building
                allHighlited = true;
                if (currentAction == ActionType.Build)
                {
                    foreach (Vector3Int pos in structureInHand.cellPositions)
                    {                
                        if (tileManager.CheckGridIsClear(highlightedTilePos + pos))
                        {
                            tempTilemap.SetTile(highlightedTilePos + pos, highlightedTileAsset);
                            tempTilemap.SetTileFlags(highlightedTilePos + pos, TileFlags.None);
                            tempTilemap.SetColor(highlightedTilePos + pos, new Color(1, 1, 1, 0.5f));
                        }
                        else
                        {
                            tempTilemap.SetTile(highlightedTilePos + pos, destroyTileAsset);
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
            allHighlited = false;
        }    
    }

    void HighlightForDestroy(Vector3Int gridPos)
    {
        // Find cells for destroying
        destroyPositions = new List<Vector3Int>(tileManager.GetStructurePositions(gridPos));
        if (destroyPositions.Count > 0)
        {
            foreach (Vector3Int pos in destroyPositions)
            {
                tempTilemap.SetTile(pos, destroyTileAsset);
            }
            allHighlited = true;
        }
        else
        {
            allHighlited = false;
        }
    }

    void SetPreviewImage(Vector3Int gridPos, AnimatedTile tile, Color color)
    {
        Vector3Int imageTilePos = new Vector3Int(gridPos.x, gridPos.y, gridPos.z + 1);
        
        tempTilemap.SetTile(imageTilePos, tile);
        StructureRotator.RotateTile(tempTilemap, imageTilePos, currentRotation);
        tempTilemap.SetTileFlags(imageTilePos, TileFlags.None);
        tempTilemap.SetColor(imageTilePos, color);
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

