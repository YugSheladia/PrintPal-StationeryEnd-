using UnityEngine;
using UnityEngine.SceneManagement;

public class ProfileMenu : MonoBehaviour
{
    [SerializeField] Scene4Manager Scene4ManagerScript;
    [SerializeField] ViewProfile ViewProfileScript;


    public void ClickedHomeButton()
    {
        SceneManager.LoadScene("Scene3_HomePage");
    }
    public void ClickedSettingsButton()
    {
        Debug.Log("Settings");
    }


    public void ClickedViewProfileInfoButton()
    {
        ViewProfileScript.UpdateProfileInfo();

        Scene4ManagerScript.DisableAllPanels();
        Scene4ManagerScript.EnableViewProfileInfoPanel();
    }

    public void ClickedLogoutButton()
    {
        Script1.SetStationeryInfo(null);
        SceneManager.LoadScene("Scene1_Loading");
    }

    public void ClickedPreviousPageButton()
    {
        SceneManager.LoadScene("Scene3_HomePage");
    }
}
