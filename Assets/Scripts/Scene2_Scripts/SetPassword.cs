using UnityEngine;
using TMPro;

public class SetPassword : MonoBehaviour
{
    [SerializeField] Scene2Manager Scene2ManagerScript;
    [SerializeField] Scene2Backend Scene2BackendScript;

    [SerializeField] TMP_InputField PasswordInput;

    public void ClickedSubmitButton()
    {
        Scene2BackendScript.StationeryInfoWrapperObject.StationeryInfoObject.Password = PasswordInput.text;

        Scene2ManagerScript.DisableAllPanels();
        Scene2ManagerScript.EnableSetProfileInfoPanel();
    }
}
