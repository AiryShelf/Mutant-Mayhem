using UnityEngine;
using UnityEngine.InputSystem;

public class InputProbe : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;               // HIGHLIGHT: assign in Inspector
    [SerializeField] private string[] watchActions =                // HIGHLIGHT: set to your action names
        { "Move", "Fire", "Grenade", "Melee", "Sprint", "Interact", "Tab" };

    private int frames;
    void Awake()
    {
        if (!playerInput) playerInput = FindObjectOfType<PlayerInput>();
        Debug.Log("[Probe] Awake");
    }
    void Update()
    {
        if (!playerInput || frames++ > 120) return; // ~2 seconds
        var map = playerInput.currentActionMap;
        var scheme = playerInput.currentControlScheme ?? "(null)";
        string rows = "";
        foreach (var name in watchActions)
        {
            var a = playerInput.actions?.FindAction(name);
            if (a == null) continue;
            rows += $"{name}: enabled={a.enabled}  phase={a.phase}  boundControls={a.controls.Count}\n";
        }
        Debug.Log($"[Probe f{frames}] Scheme={scheme}  Map={(map != null ? map.name : "(null)")}  " +
                  $"actionsEnabled={playerInput.actions?.enabled}\n{rows}");
    }
}
