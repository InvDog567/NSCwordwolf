using UnityEngine;
 
public class BackButton : MonoBehaviour
{
    public GameObject settingsMenu;
 
    public void CloseMenu()
    {
        settingsMenu.SetActive(false);
    }
}
 