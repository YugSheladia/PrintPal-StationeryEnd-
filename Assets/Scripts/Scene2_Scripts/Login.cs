using UnityEngine;
using TMPro;

public class LoginInfo
{
    public string PhoneNumber;
    public string Password;
}

public class Login : MonoBehaviour
{
    [SerializeField] Scene2Manager Scene2ManagerScript;
    [SerializeField] Scene2Backend Scene2BackendScript;

    [SerializeField] TMP_InputField PhoneNumberInput;
    [SerializeField] TMP_InputField PasswordInput;








    public void ClickedSubmitbutton()
    {
        if(CheckLoginInfo())
        {
            Scene2ManagerScript.EnableInvisiblePanel();

            LoginInfo LoginInfoObject = new LoginInfo();

            LoginInfoObject.PhoneNumber = PhoneNumberInput.text;
            LoginInfoObject.Password = PasswordInput.text;

            Scene2BackendScript.VerifyLoginInfo(LoginInfoObject);
        }
        else
        {
            Scene2ManagerScript.EnableMessagePanel();

            Scene2ManagerScript.DisableAllPanels();
            Scene2ManagerScript.EnableLoginPanel();
        }
            
    }

    public void ClickedCreateNewAccount()
    {
        Scene2ManagerScript.DisableAllPanels();
        Scene2ManagerScript.EnableSetPhoneNumberPanel();
    }










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
