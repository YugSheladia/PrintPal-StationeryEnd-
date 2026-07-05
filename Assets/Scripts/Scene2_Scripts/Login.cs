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
        LoginInfo LoginInfoObject = new LoginInfo();

        LoginInfoObject.PhoneNumber = PhoneNumberInput.text;
        LoginInfoObject.Password = PasswordInput.text;

        Scene2BackendScript.VerifyLoginInfo(LoginInfoObject);
    }

    public void ClickedCreateNewAccount()
    {
        Scene2ManagerScript.DisableAllPanels();
        Scene2ManagerScript.EnableSetPhoneNumberPanel();
    }
}
