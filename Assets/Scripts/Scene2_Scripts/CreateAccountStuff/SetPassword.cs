//namespaces = 
using UnityEngine;
using TMPro;

public class SetPassword : MonoBehaviour
{
    //Script references =
    [SerializeField] Scene2Manager Scene2ManagerScript;
    [SerializeField] Scene2Backend Scene2BackendScript;




    //Inputfield references =
    [SerializeField] TMP_InputField PasswordInput;
    [SerializeField] TMP_InputField Password2Input;






    //Buttons in PasswordPanel =
    public void ClickedSubmitButton()
    {
        //CheckPasswordLength() returns true if Password is greater than 9 characters.
        //&
        //CheckPasswordSameness() returns true if CreatePassword field and ConfirmPassword field is the same.
        if(CheckPasswordLength() && CheckPasswordSameness())
        {
            Scene2BackendScript.StationeryInfoWrapperObject.StationeryInfoObject.Password = PasswordInput.text;

            Scene2ManagerScript.DisableAllPanels();
            Scene2ManagerScript.EnableSetProfileInfoPanel();

            return;
        }

        Scene2ManagerScript.DisableAllPanels();

        Scene2ManagerScript.EnableMessagePanel();
        Scene2ManagerScript.EnableSetPasswordPanel();
    }

    //To go from Password panel -> PhoneNumber panel.
    public void ClickedPreviousPage()
    {
        Scene2ManagerScript.DisableAllPanels();
        Scene2ManagerScript.EnableSetPhoneNumberPanel();
    }








    //CheckPasswordLength() = Checks if Password length is greater than 9 or not.
    public bool CheckPasswordLength()
    {
        if(PasswordInput.text.Length < 9)
        {
            Scene2ManagerScript.MessageText.text = "Password has to be of atleast 9 characters.";
            return false;
        }
        else
        {
            return true;
        }
    }

    //CheckPasswordSamenesss() = Checks if both the Create Password & Confirm Password is same or not.
    public bool CheckPasswordSameness()
    {
        if (Password2Input.text != PasswordInput.text)
        {
            Scene2ManagerScript.MessageText.text = "Passwords don't match.";

            return false;
        }
        else
        {
            return true;
        }
    }
}
