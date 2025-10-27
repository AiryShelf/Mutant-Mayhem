// NEW: InputActionTapper.cs  (attach next to PlayerInput for debugging)
// Logs device states and action callbacks in the first few seconds of play.
using UnityEngine;
using UnityEngine.InputSystem;

public class InputActionTapper : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;        // HIGHLIGHT: assign in Inspector
    [SerializeField] private string fire = "Fire";           // your action names here
    [SerializeField] private string grenade = "Grenade";
    [SerializeField] private string melee = "Melee";
    [SerializeField] private string sprint = "Sprint";
    [SerializeField] private string interact = "Interact";   // your "Tab/Cube" action

    private InputAction aFire, aGrenade, aMelee, aSprint, aInteract;
    private float tEnd;

    void Awake()
    {
        if (!playerInput) playerInput = FindObjectOfType<PlayerInput>();
        aFire     = playerInput.actions.FindAction(fire);
        aGrenade  = playerInput.actions.FindAction(grenade);
        aMelee    = playerInput.actions.FindAction(melee);
        aSprint   = playerInput.actions.FindAction(sprint);
        aInteract = playerInput.actions.FindAction(interact);
        tEnd = Time.unscaledTime + 6f; // watch the first 6 seconds
    }

    void OnEnable()
    {
        Hook(aFire,     "Fire");
        Hook(aGrenade,  "Grenade");
        Hook(aMelee,    "Melee");
        Hook(aSprint,   "Sprint");
        Hook(aInteract, "Interact");
    }
    void OnDisable()
    {
        Unhook(aFire); Unhook(aGrenade); Unhook(aMelee); Unhook(aSprint); Unhook(aInteract);
    }

    private void Hook(InputAction a, string name)
    {
        if (a == null) return;
        a.started  += _ => Debug.Log($"[Tapper] {name}.started");
        a.performed+= _ => Debug.Log($"[Tapper] {name}.performed");
        a.canceled += _ => Debug.Log($"[Tapper] {name}.canceled");
    }
    private void Unhook(InputAction a)
    {
        if (a == null) return;
        a.started  -= null; a.performed -= null; a.canceled -= null;
    }

    void Update()
    {
        if (Time.unscaledTime > tEnd) return;

        // HIGHLIGHT: raw device states the same frame (proves whether devices see presses)
        var kb = Keyboard.current; var ms = Mouse.current;
        if (kb != null && ms != null)
        {
            Debug.Log($"[Device] LMB={ms.leftButton.isPressed} RMB={ms.rightButton.isPressed} " +
                      $"Space={kb.spaceKey.isPressed} Shift={kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed} " +
                      $"Tab={kb.tabKey.isPressed}");
        }
    }
}
