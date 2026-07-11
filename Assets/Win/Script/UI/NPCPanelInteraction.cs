using UnityEngine;

public class NPCPanelInteraction : MonoBehaviour
{
    public GameObject panelToShow;
    public float interactDistance = 100f;

    private Transform player;
    private bool panelOpen = false;

    public static NPCPanelInteraction currentOpenPanel;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (panelToShow != null)
            panelToShow.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        if (Vector3.Distance(transform.position, player.position) <= interactDistance &&
            Input.GetKeyDown(KeyCode.F))
        {
            if (panelOpen)
            {
                ClosePanel();
            }
            else
            {
                if (currentOpenPanel != null)
                    currentOpenPanel.ClosePanel();

                OpenPanel();
            }
        }
    }

    void OpenPanel()
    {
        panelOpen = true;
        panelToShow.SetActive(true);
        currentOpenPanel = this;
    }

    public void ClosePanel()
    {
        panelOpen = false;
        panelToShow.SetActive(false);

        if (currentOpenPanel == this)
            currentOpenPanel = null;
    }
}