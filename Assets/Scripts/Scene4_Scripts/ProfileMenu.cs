using UnityEngine;
using UnityEngine.SceneManagement;

public class ProfileMenu : MonoBehaviour
{
    [SerializeField] Scene4Manager Scene4ManagerScript;
    [SerializeField] ViewProfile ViewProfileScript;



    public void ClickedViewProfileInfoButton()
    {
        Scene4ManagerScript.EnableInvisiblePanel();

        Scene4ManagerScript.EnableViewProfileInfoPanel();

        StartCoroutine(ViewProfileScript.UpdateProfileInfo());

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
