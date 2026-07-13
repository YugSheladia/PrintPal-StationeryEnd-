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
        if(CheckProfileInfo())
        {
            Scene2ManagerScript.EnableInvisiblePanel();

            Scene2BackendScript.StationeryInfoWrapperObject.StationeryInfoObject.StationeryName = StationeryNameInput.text;
            Scene2BackendScript.StationeryInfoWrapperObject.StationeryInfoObject.UPINumber = UPINumberInput.text;
            Scene2BackendScript.StationeryInfoWrapperObject.StationeryInfoObject.Email = EmailInput.text;

            Scene2BackendScript.VerifyStationeryInfo();
        }
        else
        {
            Scene2ManagerScript.EnableMessagePanel();

            Scene2ManagerScript.DisableAllPanels();
            Scene2ManagerScript.EnableSetProfileInfoPanel();
        }
    }




    public void ClickedPreviousPage()
    {
        Scene2ManagerScript.DisableAllPanels();
        Scene2ManagerScript.EnableSetPasswordPanel();
    }




    public bool CheckProfileInfo()
    {
        if(!CheckNameLength())
        {
            Scene2ManagerScript.MessageText.text = "Name field can't be empty.";

            return false;
        }
        if (!CheckUPINumberLength())
        {
            return false;
        }
        if (!CheckEmailLegnth())
        {
            Scene2ManagerScript.MessageText.text = "Email field can't be empty.";

            return false;
        }
        else
        {
            return true;
        }
    }
    public bool CheckNameLength()
    {
        if (StationeryNameInput.text.Length <= 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public bool CheckUPINumberLength()
    {
        if(UPINumberInput.text.Length <= 0)
        {
            Scene2ManagerScript.MessageText.text = "UPI number can't be empty.";

            return false;
        }
        if (UPINumberInput.text.Length != 10)
        {
            Scene2ManagerScript.MessageText.text = "UPI number has to be of 10 digits.";

            return false;
        }
        else
        {
            return true;
        }
    }
    public bool CheckEmailLegnth()
    {
        if (EmailInput.text.Length <= 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
