using UnityEngine;
using TMPro;

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

    [SerializeField] TMP_InputField StationeryNameInput;
    [SerializeField] TMP_InputField UPINumberInput;
    [SerializeField] TMP_InputField EmailInput;

    public void ClickedSubmitButton()
    {
        UpdatedStationeryInfo UpdatedStationeryInfoObject = new UpdatedStationeryInfo();

        UpdatedStationeryInfoObject.PhoneNumber     = Script1.StationeryInfoObject.PhoneNumber;

        UpdatedStationeryInfoObject.StationeryName  = StationeryNameInput.text;
        UpdatedStationeryInfoObject.UPINumber       = UPINumberInput.text;
        UpdatedStationeryInfoObject.Email           = EmailInput.text;

        Scene4BackendScript.StartCoroutine_UpdateStationeryInfoInServer(UpdatedStationeryInfoObject);
    }


    public void ClickedPreviousPageButton()
    {
        Scene4ManagerScript.DisableAllPanels();
        Scene4ManagerScript.EnableViewProfileInfoPanel();
    }
}
