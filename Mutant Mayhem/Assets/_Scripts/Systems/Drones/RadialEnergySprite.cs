using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Radial energy indicator for tiny (e.g., 32x32) sprites using a pre-sliced sprite sheet.
/// 
/// • Provide frames in order (either Empty→Full or Full→Empty) via the inspector.
/// • Call SetEnergy01(value) ONLY when energy changes. No Update() polling.
/// • Uses SpriteRenderer and swaps sprites; keep all frames in one atlas/texture for batching.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class RadialEnergySprite : MonoBehaviour
{
    [Header("References")]
    [Tooltip("SpriteRenderer that displays the radial energy frames. If null, will auto-get on Awake.")]
    [SerializeField] SpriteRenderer targetRenderer;
    [Tooltip("Drone reference")]
    [SerializeField] Drone drone;

    [Header("Frames")] 
    [Tooltip("All frames for the radial fill. Keep them in a SINGLE texture atlas for batching.\nOrder depends on FramesOrder setting.")]
    [SerializeField] List<Sprite> frames = new List<Sprite>();

    public enum FramesOrder { EmptyToFull, FullToEmpty }
    [Tooltip("How frames are ordered in the list above.")]
    [SerializeField] FramesOrder framesOrder = FramesOrder.EmptyToFull;

    // Backing state
    [SerializeField] float energyRatio = 1f; // Serialized for editor preview
    int lastAppliedIndex = -1;

    void Awake()
    {
        if (!targetRenderer)
            targetRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        // Ensure the correct frame is shown on enable
        energyRatio = (float)drone.energy / drone.energyMax;
        ApplyEnergyToFrame();
        drone.onEnergyChanged += OnEnergyChanged;
    }

    void OnDisable()
    {
        drone.onEnergyChanged -= OnEnergyChanged;
    }

    /// <summary>
    /// Set energy as a normalized value in [0,1]. Call this ONLY when the value changes.
    /// </summary>
    public void OnEnergyChanged(int value)
    {
        energyRatio = (float)value / drone.energyMax;
        energyRatio = Mathf.Clamp01(energyRatio);
        ApplyEnergyToFrame();
    }

    void ApplyEnergyToFrame()
    {
        if (frames == null || frames.Count == 0 || targetRenderer == null)
            return;

        int steps = frames.Count;
        // Map energy to discrete frame index using rounding for more intuitive thresholds
        int idx = Mathf.RoundToInt(energyRatio * (steps - 1));

        // Convert idx to the actual frames list index depending on ordering
        int listIndex;
        if (framesOrder == FramesOrder.EmptyToFull)
        {
            // 0 => empty, steps-1 => full
            listIndex = idx;
        }
        else // FullToEmpty
        {
            // 0 => full, steps-1 => empty
            listIndex = (steps - 1) - idx;
        }

        // Avoid redundant assignments to reduce minor overhead
        if (listIndex == lastAppliedIndex)
            return;

        lastAppliedIndex = listIndex;
        targetRenderer.sprite = frames[listIndex];
    }
}
