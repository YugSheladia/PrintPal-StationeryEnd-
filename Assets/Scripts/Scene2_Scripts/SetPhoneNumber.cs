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
        StationeryPhoneNumber StationeryPhoneNumberObject = new StationeryPhoneNumber();
        StationeryPhoneNumberObject.PhoneNumber = PhoneNumberInput.text;

        Scene2BackendScript.CheckPhoneNumberAvailability(StationeryPhoneNumberObject);
    }
}
