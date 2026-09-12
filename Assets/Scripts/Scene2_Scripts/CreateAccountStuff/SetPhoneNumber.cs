using UnityEngine;
using TMPro;



//Class used to store Stationery phone number.
public class StationeryPhoneNumber
{
    public string PhoneNumber;
}





public class SetPhoneNumber : MonoBehaviour
{
    //Script references =
    [SerializeField] Scene2Manager Scene2ManagerScript;
    [SerializeField] Scene2Backend Scene2BackendScript;



    //InputFields = 
    [SerializeField] TMP_InputField PhoneNumberInput;






    //Input button functions =
    public void ClickedSubmitButton()
    {
        //CheckPhoneNumberLength() = returns true if Phone number is of correct length.
        if (CheckPhoneNumberLength())
        {
            //To make sure any other input button is not clicked by the user while server checks if the inputed number is
            //already taken or not.
            Scene2ManagerScript.EnableInvisiblePanel();


            //Phone number status validation process = 
            StationeryPhoneNumber StationeryPhoneNumberObject = new StationeryPhoneNumber();

            StationeryPhoneNumberObject.PhoneNumber = PhoneNumberInput.text;

            StartCoroutine
            (
                Scene2BackendScript.SubmitPhoneNumberToServer
                (
                    StationeryPhoneNumberObject,

                    (Success, ErrorMessage) =>
                    {
                        if (Success)
                        {
                            Scene2ManagerScript.DisableAllPanels();
                            Scene2ManagerScript.EnableSetPasswordPanel();
                        }
                        else
                        {
                            Scene2ManagerScript.DisableAllPanels();
                            Scene2ManagerScript.EnableSetPhoneNumberPanel();

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
            Scene2ManagerScript.EnableSetPhoneNumberPanel();

            Scene2ManagerScript.EnableMessagePanel();
        }
    }

    //To go from SetPhoneNumber panel -> Login panel
    public void ClickedPreviousBUtton()
    {
        Scene2ManagerScript.DisableAllPanels();
        Scene2ManagerScript.EnableLoginPanel();
    }






    //CheckPhoneNumberLength() = To check if the Phone number is of 10 digits.
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
