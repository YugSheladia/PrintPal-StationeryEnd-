//namespaces =
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProfileMenu : MonoBehaviour
{
    //Script references =
    [SerializeField] Scene4Manager Scene4ManagerScript;
    [SerializeField] ViewProfile ViewProfileScript;








    //===========================
    //Button functions =
    //===========================

    //To view Stationery info.
    public void ClickedViewProfileInfoButton()
    {
        Scene4ManagerScript.EnableInvisiblePanel();

        Scene4ManagerScript.EnableViewProfileInfoPanel();

        //This coroutine retrieves all stationery info from the server.
        StartCoroutine(ViewProfileScript.UpdateProfileInfo());

    }



    //To logout.
    public void ClickedLogoutButton()
    {
        //This sets all Stationery info in Script1 to null.
        Script1.SetStationeryInfo(null);
        Script1.SetPrinters(null);

        SceneManager.LoadScene("Scene1_Loading");
    }



    //To go from Profile menu page -> HomePage.
    public void ClickedPreviousPageButton()
    {
        SceneManager.LoadScene("Scene3_HomePage");
    }
}
