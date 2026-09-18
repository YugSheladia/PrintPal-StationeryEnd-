//namespaces = 
using UnityEngine;
using TMPro;
using System.Collections;






//This class is used to store all the info the user has inputted in UpdateProfile page's Text Input fields.
public class UpdatedStationeryInfo
{
    public string PhoneNumber;

    public string StationeryName;
    public string StationeryLocation;

    public string UPINumber;
    public string Email;
}








public class UpdateProfile : MonoBehaviour
{
    //Script references =
    [SerializeField] Scene4Manager Scene4ManagerScript;
    [SerializeField] Scene4Backend Scene4BackendScript;

    [SerializeField] ViewProfile ViewProfileScript;




    //UpdateProfile's UI Text fields =
    [SerializeField] TMP_InputField StationeryNameInput;
    [SerializeField] TMP_InputField StationeryLocationInput;
    [SerializeField] TMP_InputField UPINumberInput;
    [SerializeField] TMP_InputField EmailInput;




    //Instantiation of UpdateStationeryInfo object.
    UpdatedStationeryInfo UpdatedStationeryInfoObject = new UpdatedStationeryInfo();








    //This function is called by ViewProfile when a user clicks on Edit button.
    public IEnumerator UpdateProfileInfo()
    {
        bool Success = false;

        //Used to recieve all Stationery info from the server, to display it in UpdateProfile page.
        yield return (StartCoroutine(Scene4BackendScript.GetStationeryInfoFromServer(
                                                                                        Result =>
                                                                                        {
                                                                                            Success = Result;
                                                                                        }
                                                                                    )
                                                                                 ));

        if (!Success)
        {
            Scene4ManagerScript.DisableAllPanels();

            Scene4ManagerScript.MessageText.text = "Something went wrong, try again later.";
            Scene4ManagerScript.EnableMessagePanel();

            Scene4ManagerScript.EnableProfileMenuPanel();

            yield break;
        }

        UpdateUI();

        Scene4ManagerScript.DisableAllPanels();
        Scene4ManagerScript.EnableUpdateProfilePanel();
    }
    
    //This functions is called by UpdateProfileInfo() to Update UI Textfields of UpdateProfile page.
    public void UpdateUI()
    {
        StationeryNameInput.text        = Script1.StationeryInfoObject.StationeryName;
        StationeryLocationInput.text    = Script1.StationeryInfoObject.StationeryLocation;

        UPINumberInput.text             = Script1.StationeryInfoObject.UPINumber;
        EmailInput.text                 = Script1.StationeryInfoObject.Email;
    }









    //============================
    //Button functions =
    //============================


    //This function is called when a user clicks on Submit button.
    public void ClickedSubmitButton()
    {
        Scene4ManagerScript.EnableInvisiblePanel();

        UpdatedStationeryInfoObject.PhoneNumber         = Script1.StationeryInfoObject.PhoneNumber;

        UpdatedStationeryInfoObject.StationeryName      = StationeryNameInput.text;
        UpdatedStationeryInfoObject.StationeryLocation  = StationeryLocationInput.text;
        UpdatedStationeryInfoObject.UPINumber           = UPINumberInput.text;
        UpdatedStationeryInfoObject.Email               = EmailInput.text;

        //This is to actually update the Stationery info in the server.
        StartCoroutine(UpdateInfoInServer());
    }
    public IEnumerator UpdateInfoInServer()
    {
        bool Success = false;

        yield return StartCoroutine
                     (
                        Scene4BackendScript.UpdateStationeryInfoInServer
                        (
                            UpdatedStationeryInfoObject, Result =>
                            {
                                Success = Result;
                            }
                        )
                     );

        if (!Success)
        {
            Scene4ManagerScript.DisableAllPanels();

            Scene4ManagerScript.MessageText.text = "Something went wrong, try again later.";
            Scene4ManagerScript.EnableMessagePanel();

            Scene4ManagerScript.EnableUpdateProfilePanel();

            yield break;
        }

        ViewProfileScript.UpdateUI();

        Scene4ManagerScript.DisableAllPanels();

        Scene4ManagerScript.MessageText.text = "Profile info updated successfully.";
        Scene4ManagerScript.EnableMessagePanel();

        Scene4ManagerScript.EnableViewProfileInfoPanel();
    }




    //IT is used to go from UpdateProfile page -> ViewProfile page.
    public void ClickedPreviousPageButton()
    {
        Scene4ManagerScript.DisableAllPanels();
        Scene4ManagerScript.EnableViewProfileInfoPanel();
    }
}
