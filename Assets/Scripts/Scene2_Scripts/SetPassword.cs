using UnityEngine;
using TMPro;

public class SetPassword : MonoBehaviour
{
    [SerializeField] Scene2Manager Scene2ManagerScript;
    [SerializeField] Scene2Backend Scene2BackendScript;

    [SerializeField] TMP_InputField PasswordInput;







    public void ClickedSubmitButton()
    {
        if(CheckPasswordLength())
        {
            Scene2BackendScript.StationeryInfoWrapperObject.StationeryInfoObject.Password = PasswordInput.text;

            Scene2ManagerScript.DisableAllPanels();
            Scene2ManagerScript.EnableSetProfileInfoPanel();
        }
        else
        {
            Scene2ManagerScript.EnableMessagePanel();

            Scene2ManagerScript.DisableAllPanels();
            Scene2ManagerScript.EnableSetPasswordPanel();
        }
    }


    public void ClickedPreviousPage()
    {
        Scene2ManagerScript.DisableAllPanels();
        Scene2ManagerScript.EnableSetPhoneNumberPanel();
    }









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
}
