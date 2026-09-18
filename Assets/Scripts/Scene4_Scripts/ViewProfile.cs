//namespaces =
using UnityEngine;
using TMPro;
using System.Collections;

public class ViewProfile : MonoBehaviour
{
    //Script references =
    [SerializeField] Scene4Manager Scene4ManagerScript;
    [SerializeField] Scene4Backend Scene4BackendScript;

    [SerializeField] UpdateProfile UpdateProfileScript;



    //UI texts =
    [SerializeField] TMP_Text StationeryNameText;
    [SerializeField] TMP_Text StationeryLocationText;
    [SerializeField] TMP_Text StationeryIDText;
    [SerializeField] TMP_Text PhoneNumberText;
    [SerializeField] TMP_Text EmailText;




    //This is called by ProfileMenu page when a user clicks on "View Profile Info" button.
    public IEnumerator UpdateProfileInfo()
    {
        bool Success = false;

        //This is used to retrieve all Stationery info from server, to display in ViewProfile page.
        yield return (StartCoroutine(Scene4BackendScript.GetStationeryInfoFromServer(
                                                                                        Result =>
                                                                                        {
                                                                                            Success = Result;
                                                                                        }
                                                                                    )
                                                                                 ));

        if(!Success)
        {
            Scene4ManagerScript.DisableAllPanels();

            Scene4ManagerScript.MessageText.text = "Something went wrong, try again later.";
            Scene4ManagerScript.EnableMessagePanel();

            Scene4ManagerScript.EnableProfileMenuPanel();

            yield break;
        }

        UpdateUI();

        Scene4ManagerScript.DisableAllPanels();
        Scene4ManagerScript.EnableViewProfileInfoPanel();
    }

    //This is called by UpdateProfileInfo to Update the ViewProfile page's UI.
    public void UpdateUI()
    {
        StationeryNameText.text     = Script1.StationeryInfoObject.StationeryName;
        StationeryLocationText.text = Script1.StationeryInfoObject.StationeryLocation;

        StationeryIDText.text   = Script1.StationeryInfoObject.StationeryID.ToString();
        PhoneNumberText.text    = Script1.StationeryInfoObject.PhoneNumber;
        EmailText.text          = Script1.StationeryInfoObject.Email;
    }









    //==========================
    //Button functions =
    //==========================


    //This is called when a user clicks on "Edit" button.
    public void ClickedEditButton()
    {
        Scene4ManagerScript.EnableInvisiblePanel();

        Scene4ManagerScript.EnableUpdateProfilePanel();

        //
        StartCoroutine(UpdateProfileScript.UpdateProfileInfo());
    }





    //This is to go from ViewProfile page -> ProfileMenu page.
    public void ClickedPreviousPageButton()
    {
        Scene4ManagerScript.DisableAllPanels();
        Scene4ManagerScript.EnableProfileMenuPanel();
    }
}
