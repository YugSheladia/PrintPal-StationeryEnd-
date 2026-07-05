using UnityEngine;
using TMPro;

public class ViewProfile : MonoBehaviour
{
    [SerializeField] Scene4Manager Scene4ManagerScript;
    [SerializeField] Scene4Backend Scene4BackendScript;

    [SerializeField] TMP_Text StationeryNameText;
    [SerializeField] TMP_Text StationeryIDText;
    [SerializeField] TMP_Text PhoneNumberText;
    [SerializeField] TMP_Text EmailText;



    public void UpdateProfileInfo()
    {
        Scene4BackendScript.StartCoroutine_GetStationeryInfoFromServer();
    }

    public void UpdateUI()
    {
        StationeryNameText.text = Script1.StationeryInfoObject.StationeryName;
        StationeryIDText.text   = Script1.StationeryInfoObject.StationeryID.ToString();
        PhoneNumberText.text    = Script1.StationeryInfoObject.PhoneNumber;
        EmailText.text          = Script1.StationeryInfoObject.Email;
    }


    public void ClickedEditButton()
    {
        Scene4ManagerScript.DisableAllPanels();
        Scene4ManagerScript.EnableUpdateProfilePanel();
    }

    public void ClickedPreviousPageButton()
    {
        Scene4ManagerScript.DisableAllPanels();
        Scene4ManagerScript.EnableProfileMenuPanel();
    }
}
