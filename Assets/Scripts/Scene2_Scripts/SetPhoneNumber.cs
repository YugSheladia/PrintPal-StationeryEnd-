using UnityEngine;
using TMPro;

public class StationeryPhoneNumber
{
    public string PhoneNumber;
}

public class SetPhoneNumber : MonoBehaviour
{
    [SerializeField] Scene2Manager Scene2ManagerScript;
    [SerializeField] Scene2Backend Scene2BackendScript;

    [SerializeField] TMP_InputField PhoneNumberInput;






    public void ClickedSubmitButton()
    {
        if(CheckPhoneNumberLength())
        {
            Scene2ManagerScript.EnableInvisiblePanel();

            StationeryPhoneNumber StationeryPhoneNumberObject = new StationeryPhoneNumber();
            StationeryPhoneNumberObject.PhoneNumber = PhoneNumberInput.text;

            Scene2BackendScript.CheckPhoneNumberAvailability(StationeryPhoneNumberObject);
        }
        else
        {
            Scene2ManagerScript.EnableMessagePanel();

            Scene2ManagerScript.DisableAllPanels();
            Scene2ManagerScript.EnableSetPhoneNumberPanel();
        }
            
    }

    public void ClickedPreviousBUtton()
    {
        Scene2ManagerScript.DisableAllPanels();
        Scene2ManagerScript.EnableLoginPanel();
    }






    public bool CheckPhoneNumberLength()
    {
        if(PhoneNumberInput.text.Length != 10)
        {
            Scene2ManagerScript.MessageText.text = "Phone number has to be of 10 digits.";

            return false;
        }
        else
        {
            return true;
        }
    }
}
