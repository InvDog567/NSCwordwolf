using UnityEngine;

public class GlobalCloseButton : MonoBehaviour
{
    public void CloseCurrentPanel()
    {
        if (NPCPanelInteraction.currentOpenPanel != null)
        {
            NPCPanelInteraction.currentOpenPanel.ClosePanel();
        }
    }
}