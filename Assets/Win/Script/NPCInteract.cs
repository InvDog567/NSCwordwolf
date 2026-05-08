using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    public GameObject chatUI;

    bool playerNearby;

    void Update()
    {
        if(playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            chatUI.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if(chatUI.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            chatUI.SetActive(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}