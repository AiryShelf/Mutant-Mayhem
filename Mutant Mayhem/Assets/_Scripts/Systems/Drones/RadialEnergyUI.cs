using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Radial energy indicator for small drones using a pre-sliced sprite sheet displayed via a UI Image.
/// 
/// • Provide frames in order (either Empty→Full or Full→Empty) via the inspector.
/// • Call SetEnergy01(value) ONLY when energy changes. No Update() polling.
/// • Uses an Image component and swaps sprites; keep all frames in one atlas/texture for batching.
/// </summary>
[RequireComponent(typeof(Image))]
public class RadialEnergyUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Image that displays the radial energy frames. If null, will auto-get on Awake.")]
    [SerializeField] Image targetImage;

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
        if (!targetImage)
            targetImage = GetComponent<Image>();
    }

    /// <summary>
    /// Set energy as a normalized value in [0,1]. Call this ONLY when the value changes.
    /// </summary>
    public void OnEnergyChanged(int current, int maxEnergy)
    {
        energyRatio = (float)current / maxEnergy;
        energyRatio = Mathf.Clamp01(energyRatio);
        ApplyEnergyToFrame();
    }

    void ApplyEnergyToFrame()
    {
        if (frames == null || frames.Count == 0 || targetImage == null)
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
        targetImage.sprite = frames[listIndex];
    }
}
