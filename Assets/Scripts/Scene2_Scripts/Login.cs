//namespaces = 
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;



//A class used to store Email & Password, which is then submitted to the server for validating login request.
public class LoginInfo
{
    public string PhoneNumber;
    public string Password;
}




public class Login : MonoBehaviour
{
    //Script references = 
    [SerializeField] Scene2Manager Scene2ManagerScript;
    [SerializeField] Scene2Backend Scene2BackendScript;

    [SerializeField] SelectPrinters SelectPrintersScript;




    //Inputfield references =
    [SerializeField] TMP_InputField PhoneNumberInput;
    [SerializeField] TMP_InputField PasswordInput;







    //Buttons in LoginPanel = 
    public void ClickedSubmitbutton()
    {
        //CheckLoginInfo() returns true only if LoginInfo ic correct.
        if (CheckLoginInfo())
        {
            //To make sure any other input button is not clicked by the user while login info is being validated.
            Scene2ManagerScript.EnableInvisiblePanel();



            //Login info validation process =
            LoginInfo LoginInfoObject = new LoginInfo();

            LoginInfoObject.PhoneNumber = PhoneNumberInput.text;
            LoginInfoObject.Password = PasswordInput.text;

            StartCoroutine
            (
                Scene2BackendScript.SubmitLoginInfoToServer
                (
                    LoginInfoObject,

                    (Success, ErrorMessage) =>
                    {
                        if (Success)
                        {
                            SceneManager.LoadScene("Scene3_HomePage");
                        }
                        else
                        {
                            Scene2ManagerScript.DisableAllPanels();
                            Scene2ManagerScript.EnableLoginPanel();

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
            Scene2ManagerScript.EnableLoginPanel();

            Scene2ManagerScript.EnableMessagePanel();
        }
    }
    public void ClickedCreateNewAccount()
    {
        Scene2ManagerScript.DisableAllPanels();
        Scene2ManagerScript.EnableSetPhoneNumberPanel();
    }







    //To check if LoginInfo is correct, CheckLoginInfo() calls
    //CheckPhoneNumberLength() = To check Phone number is of 10 digits.
    //&
    //CheckPasswordLength() = To check if Password field is not empty.
    public bool CheckLoginInfo()
    {
        if(!CheckPhoneNumberLength())
        {
            return false;
        }
        if(!CheckPasswordLength())
        {
            return false;
        }

        return true;
    }
    public bool CheckPhoneNumberLength()
    {
        if(PhoneNumberInput.text.Length <= 0)
        {
            Scene2ManagerScript.MessageText.text = "Phone number field can't be empty.";

            return false;

        }
        if(PhoneNumberInput.text.Length != 10)
        {
            Scene2ManagerScript.MessageText.text = "Phone number has to be of 10 digits.";

            return false;

        }

        return true;
    }
    public bool CheckPasswordLength()
    {
        if (PasswordInput.text.Length <= 0)
        {
            Scene2ManagerScript.MessageText.text = "Password field can't be empty.";

            return false;
        }

        return true;
    }
}
