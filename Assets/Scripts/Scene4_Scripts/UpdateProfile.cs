using UnityEngine;
using TMPro;
using System.Collections;

public class UpdatedStationeryInfo
{
    public string PhoneNumber;
    public string StationeryName;
    public string UPINumber;
    public string Email;
}

public class UpdateProfile : MonoBehaviour
{
    [SerializeField] Scene4Manager Scene4ManagerScript;
    [SerializeField] Scene4Backend Scene4BackendScript;

    [SerializeField] ViewProfile ViewProfileScript;

    [SerializeField] TMP_InputField StationeryNameInput;
    [SerializeField] TMP_InputField UPINumberInput;
    [SerializeField] TMP_InputField EmailInput;




    UpdatedStationeryInfo UpdatedStationeryInfoObject = new UpdatedStationeryInfo();




    public IEnumerator UpdateProfileInfo()
    {
        bool Success = false;

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


    public void UpdateUI()
    {
        StationeryNameInput.text    = Script1.StationeryInfoObject.StationeryName;
        UPINumberInput.text         = Script1.StationeryInfoObject.UPINumber;
        EmailInput.text             = Script1.StationeryInfoObject.Email;
    }


    public void ClickedSubmitButton()
    {
        Scene4ManagerScript.EnableInvisiblePanel();

        UpdatedStationeryInfoObject.PhoneNumber     = Script1.StationeryInfoObject.PhoneNumber;

        UpdatedStationeryInfoObject.StationeryName  = StationeryNameInput.text;
        UpdatedStationeryInfoObject.UPINumber       = UPINumberInput.text;
        UpdatedStationeryInfoObject.Email           = EmailInput.text;

        StartCoroutine(UpdateInfoInServer());
    }


    public IEnumerator UpdateInfoInServer()
    {
        bool Success = false;

        yield return StartCoroutine(
                                    Scene4BackendScript.UpdateStationeryInfoInServer(
                                    UpdatedStationeryInfoObject, Result =>
                                    {
                                    Success = Result;
                                    }));

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


    public void ClickedPreviousPageButton()
    {
        Scene4ManagerScript.DisableAllPanels();
        Scene4ManagerScript.EnableViewProfileInfoPanel();
    }
}
