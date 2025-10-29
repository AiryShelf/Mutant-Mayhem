using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradePanelManager : MonoBehaviour
{
    public static UpgradePanelManager Instance;
    public UiUpgradePanel[] panels;
    public bool isOpen { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void OpenPanel(StructureType structureType, PanelInteract panelInteract)
    {
        foreach (UiUpgradePanel panel in panels)
        {
            if (panel.structureToBuildForUnlock == structureType)
            {
                panel.OpenPanel(panelInteract);
                isOpen = true;
            }
            else
            {
                panel.ClosePanel();
            }
        }

        if (!isOpen)
            Debug.LogError("UpgradePanelManager: Tried to open a panel for a structure that has no associated panel!");
    }

    public void ClosePanel(StructureType structureType)
    {
        foreach (UiUpgradePanel panel in panels)
        {
            if (panel.structureToBuildForUnlock == structureType)
            {
                panel.ClosePanel();
                isOpen = false;
                return;
            }
        }

        Debug.LogError("UpgradePanelManager: Tried to close a panel for a structure that has no associated panel!");
    }

    public void CloseAllPanels()
    {
        isOpen = false;

        foreach (UiUpgradePanel panel in panels)
        {
            panel.ClosePanel();
        }
    }

    public void PowerOnUpgradePanel(StructureSO structure, bool playEffect)
    {
        foreach (UiUpgradePanel panel in panels)
        {
            if (panel.structureToBuildForUnlock == structure.structureType)
            {
                panel.OnPowerOn(playEffect);
                return;
            }
        }
    }

    public void PowerOffUpgradePanel(StructureSO structure, bool playEffect)
    {
        foreach (UiUpgradePanel panel in panels)
        {
            if (panel.structureToBuildForUnlock == structure.structureType)
            {
                panel.OnPowerOff(playEffect);
                return;
            }
        }
    }
}
