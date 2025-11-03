using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractController : MonoBehaviour
{
    public static InteractController Instance { get; private set; }

    [SerializeField] float pollInterval = 0.05f;
    [SerializeField] float radius = 0.9f;
    [SerializeField] LayerMask interactMask;
    
    Player player;

    // ▸ Reused buffer to avoid GC each poll.
    private readonly Collider2D[] _cols = new Collider2D[32];

    Vector3 _lastPlayerPos;
    PanelInteract highlightedPanel;
    PanelInteract openedPanel;
    PanelInteract _lastHighlightedPanel;
        
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (player == null) player = FindObjectOfType<Player>();
        StartCoroutine(DelaySearch());
    }

    IEnumerator DelaySearch()
    {
        var wait = new WaitForSecondsRealtime(pollInterval);
        while (true)
        {
            if (player != null)
            {
                // ▸ Skip if player barely moved (cheap early-out).
                if ((player.transform.position - _lastPlayerPos).sqrMagnitude > 0.001f)
                {
                    ScanAndHighlight((Vector2)player.transform.position);
                    _lastPlayerPos = player.transform.position;
                }
            }

            yield return wait;
        }
    }

    void ScanAndHighlight(Vector2 playerPos)
    {
        var tileManager = TileManager.Instance;
        PanelInteract bestPanel = null;
        if (tileManager != null)
        {
            var tuple = tileManager.GetClosestObjectAndPanelUnderCircle(playerPos, radius);
            bestPanel = tuple.Item2;
        }

        // Apply/remove glow
        if (bestPanel != null && bestPanel != _lastHighlightedPanel)
        {
            bestPanel.interactHighlighter.EnableGlow();
        }
        if (_lastHighlightedPanel != null && _lastHighlightedPanel != bestPanel)
        {
            _lastHighlightedPanel.interactHighlighter.DisableGlow();
            _lastHighlightedPanel = null;
        }
        _lastHighlightedPanel = bestPanel;
        highlightedPanel = bestPanel;
    }
    
    public bool OpenHighlightedPanel(Player player)
    {
        if (highlightedPanel != null)
        {
            if (highlightedPanel is PanelInteract_DroneHangar dronePanel)
            {
                DroneContainer droneContainer = dronePanel.droneContainer;
                if (droneContainer != null)
                {
                    player.stats.structureStats.currentDroneContainer = droneContainer;
                    dronePanel.OpenPanel(player);
                    openedPanel = highlightedPanel;
                    return true;
                }
                else
                {
                    Debug.LogError("InteractController: Highlighted Drone Hangar panel has no DroneContainer!");
                    return false;
                }
            }

            highlightedPanel.OpenPanel(player);
            openedPanel = highlightedPanel;
            return true;
        }
        return false;
    }

    public void CloseOpenedPanel()
    {
        if (openedPanel != null)
        {
            openedPanel.ClosePanel();
            openedPanel = null;
        }
    }

    
}