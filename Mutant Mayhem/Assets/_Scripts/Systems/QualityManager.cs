using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QualityLevel
{
    VeryLow = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    VeryHigh = 4,
    Ultra = 5,
}

public class QualityManager : MonoBehaviour
{
    public static QualityLevel CurrentQualityLevel;
    public static event Action<QualityLevel> OnQualitySettingsChanged;

    void Awake()
    {
        if (PlayerPrefs.HasKey("Graphics_Quality"))
        {
            int qualityLevel = PlayerPrefs.GetInt("Graphics_Quality");
            QualitySettings.SetQualityLevel(qualityLevel);
            CurrentQualityLevel = (QualityLevel)qualityLevel;
            OnQualitySettingsChanged?.Invoke((QualityLevel)qualityLevel);
        }
        else
        {
            DetermineGraphicsQuality();
        }

        if (PlayerPrefs.HasKey("VSync"))
        {
            QualitySettings.vSyncCount = PlayerPrefs.GetInt("VSync");
        }
    }

    public void SetVSync(bool enable)
    {
        QualitySettings.vSyncCount = enable ? 1 : 0;
        PlayerPrefs.SetInt("VSync", enable ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"VSync set to: {(enable ? "On" : "Off")}");
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

        QualityLevel qualityLevel = QualityLevel.High; // Default to High

        // Assign quality level based on hardware
        if (vram >= 8000 && cpuCores >= 10 && ram >= 32000)
        {
            qualityLevel = QualityLevel.Ultra;
        }
        else if (vram >= 6000 && cpuCores >= 8 && ram >= 16000)
        {
            qualityLevel = QualityLevel.VeryHigh;
        }
        else if (vram >= 4000 && cpuCores >= 6 && ram >= 12000)
        {
            qualityLevel = QualityLevel.High;
        }
        else if (vram >= 2000 && cpuCores >= 4 && ram >= 8000)
        {
            qualityLevel = QualityLevel.Medium;
        }
        else if (vram >= 1000 && cpuCores >= 2 && ram >= 4000)
        {
            qualityLevel = QualityLevel.Low;
        }
        else
        {
            qualityLevel = QualityLevel.VeryLow;
        }

        QualitySettings.SetQualityLevel((int)qualityLevel, true);
        PlayerPrefs.SetInt("Graphics_Quality", (int)qualityLevel);
        PlayerPrefs.Save();

        CurrentQualityLevel = qualityLevel;
        OnQualitySettingsChanged?.Invoke(qualityLevel);
        Debug.Log($"Graphics Quality Auto-Set to: {QualitySettings.names[(int)qualityLevel]} (Level {qualityLevel})");
    }

    public void SetGraphicsQuality(int index)
    {
        if (index >= QualitySettings.count)
        {
            Debug.LogError($"Failed to set graphics quality! Index: {index} is out of range");
            return;
        }

        PlayerPrefs.SetInt("Graphics_Quality", index);
        PlayerPrefs.Save();

        QualitySettings.SetQualityLevel(index, true);
        if (PlayerPrefs.HasKey("VSync"))
            SetVSync(PlayerPrefs.GetInt("VSync") > 0);

        CurrentQualityLevel = (QualityLevel)index;
        OnQualitySettingsChanged?.Invoke((QualityLevel)index);

        Debug.Log("Graphics Quality set to index: " + index + ", " + CurrentQualityLevel);
    }
}
