using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualityManager : MonoBehaviour
{
    void Awake()
    {
        if (PlayerPrefs.HasKey("Graphics_Quality"))
        {
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Graphics_Quality"));
        }
        else
        {
            DetermineGraphicsQuality();
        }
    }

    
    void DetermineGraphicsQuality()
    {
        string gpuName = SystemInfo.graphicsDeviceName;
        int vram = SystemInfo.graphicsMemorySize; // In MB
        int cpuCores = SystemInfo.processorCount;
        int ram = SystemInfo.systemMemorySize;
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        Debug.Log($"GPU: {gpuName}, VRAM: {vram}MB, CPU Cores: {cpuCores}, RAM: {ram}MB, Resolution: {screenWidth}x{screenHeight}");

        int qualityLevel = 3; // Default to High

        // Assign quality level based on hardware
        if (vram >= 8000 && cpuCores >= 10 && ram >= 32000)
        {
            qualityLevel = 5; // Ultra
        }
        else if (vram >= 6000 && cpuCores >= 8 && ram >= 16000)
        {
            qualityLevel = 4; // Very High
        }
        else if (vram >= 4000 && cpuCores >= 6 && ram >= 12000)
        {
            qualityLevel = 3; // High
        }
        else if (vram >= 2000 && cpuCores >= 4 && ram >= 8000)
        {
            qualityLevel = 2; // Medium
        }
        else if (vram >= 1000 && cpuCores >= 2 && ram >= 4000)
        {
            qualityLevel = 1; // Low
        }
        else
        {
            qualityLevel = 0; // Very Low
        }

        QualitySettings.SetQualityLevel(qualityLevel, true);
        PlayerPrefs.SetInt("Graphics_Quality", qualityLevel);
        PlayerPrefs.Save();

        Debug.Log($"Graphics Quality Auto-Set to: {QualitySettings.names[qualityLevel]} (Level {qualityLevel})");
    }
}
