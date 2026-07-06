using UnityEngine;
using TMPro;

public class SetProfileInfo : MonoBehaviour
{
    [SerializeField] Scene2Manager Scene2ManagerScript;
    [SerializeField] Scene2Backend Scene2BackendScript;

    [SerializeField] TMP_InputField StationeryNameInput;
    [SerializeField] TMP_InputField UPINumberInput;
    [SerializeField] TMP_InputField EmailInput;

    public void ClickedSubmitButton()
    {
        Scene2BackendScript.StationeryInfoWrapperObject.StationeryInfoObject.StationeryName = StationeryNameInput.text;
        Scene2BackendScript.StationeryInfoWrapperObject.StationeryInfoObject.UPINumber      = UPINumberInput.text;
        Scene2BackendScript.StationeryInfoWrapperObject.StationeryInfoObject.Email          = EmailInput.text;

        Scene2BackendScript.VerifyStationeryInfo();
    }



    public void ClickedPreviousPage()
    {
        Scene2ManagerScript.DisableAllPanels();
        Scene2ManagerScript.EnableSetPasswordPanel();
    }
}
