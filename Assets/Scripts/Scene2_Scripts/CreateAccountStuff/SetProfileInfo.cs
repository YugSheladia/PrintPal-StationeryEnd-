using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SetProfileInfo : MonoBehaviour
{
    [SerializeField] Scene2Manager Scene2ManagerScript;
    [SerializeField] Scene2Backend Scene2BackendScript;

    [SerializeField] SelectPrinters SelectPrintersScript;

    [SerializeField] TMP_InputField StationeryNameInput;
    [SerializeField] TMP_InputField UPINumberInput;
    [SerializeField] TMP_InputField EmailInput;
    [SerializeField] TMP_InputField LocationInput;




    public void ClickedSubmitButton()
    {
        if (CheckProfileInfo())
        {
            Scene2ManagerScript.EnableInvisiblePanel();

            Scene2BackendScript.StationeryInfoWrapperObject.StationeryInfoObject.StationeryName =
                StationeryNameInput.text;

            Scene2BackendScript.StationeryInfoWrapperObject.StationeryInfoObject.StationeryLocation =
                LocationInput.text;

            Scene2BackendScript.StationeryInfoWrapperObject.StationeryInfoObject.UPINumber =
                UPINumberInput.text;

            Scene2BackendScript.StationeryInfoWrapperObject.StationeryInfoObject.Email =
                EmailInput.text;

            StartCoroutine
            (
                Scene2BackendScript.SubmitStationeryInfoToServer
                (
                    (Success, ErrorMessage) =>
                    {
                        if (Success)
                        {
                            Scene2ManagerScript.EnableSelectPrintersPanel();

                            Scene2ManagerScript.MessageText.text = "Account created successfully";
                            Scene2ManagerScript.EnableMessagePanel();

                            GetPrinters();
                        }
                        else
                        {
                            Scene2ManagerScript.DisableAllPanels();
                            Scene2ManagerScript.EnableSetProfileInfoPanel();

                            Scene2ManagerScript.MessageText.text = ErrorMessage;
                            Scene2ManagerScript.EnableMessagePanel();
                        }
                    }
                )
            );
        }
        else
        {
            Scene2ManagerScript.DisableAllPanels();
            Scene2ManagerScript.EnableSetProfileInfoPanel();

            Scene2ManagerScript.EnableMessagePanel();
        }
    }
    public void ClickedPreviousPage()
    {
        Scene2ManagerScript.DisableAllPanels();
        Scene2ManagerScript.EnableSetPasswordPanel();
    }
    public void GetPrinters()
    {
        Scene2ManagerScript.EnableInvisiblePanel();

        SelectPrintersScript.GetConnectedPrinters
        (
            Result =>
            {
                if(Result)
                {
                    Scene2ManagerScript.DisableAllPanels();
                    Scene2ManagerScript.EnableSelectPrintersPanel();
                }
                else
                {
                    Scene2ManagerScript.DisableAllPanels();

                    Scene2ManagerScript.EnableSelectPrintersPanel();

                    Scene2ManagerScript.MessageText.text = "Your account has been successfuly created!\nbut an error occured when we tried to retrieve printers connected to your device. Restart the app or try again later.";
                    Scene2ManagerScript.EnableMessagePanel();
                }
            }
        );
    }








    public bool CheckProfileInfo()
    {
        if(!CheckNameLength())
        {
            Scene2ManagerScript.MessageText.text = "Name field can't be empty.";

            return false;
        }
        if (!CheckLocationLength())
        {
            Scene2ManagerScript.MessageText.text = "Location field can't be empty.";

            return false;
        }
        if (!CheckUPINumberLength())
        {
            return false;
        }
        if (!CheckEmailLength())
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
    public bool CheckLocationLength()
    {
        if (LocationInput.text.Length <= 0)
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
    public bool CheckEmailLength()
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
