using UnityEngine;
using TMPro;
using System.Collections;

public class ViewProfile : MonoBehaviour
{
    [SerializeField] Scene4Manager Scene4ManagerScript;
    [SerializeField] Scene4Backend Scene4BackendScript;

    [SerializeField] UpdateProfile UpdateProfileScript;

    [SerializeField] TMP_Text StationeryNameText;
    [SerializeField] TMP_Text StationeryIDText;
    [SerializeField] TMP_Text PhoneNumberText;
    [SerializeField] TMP_Text EmailText;




    public IEnumerator UpdateProfileInfo()
    {
        bool Success = false;

        yield return (StartCoroutine(Scene4BackendScript.GetStationeryInfoFromServer(
                                                                                        Result =>
                                                                                        {
                                                                                            Success = Result;
                                                                                        }
                                                                                    )
                                                                                 ));

        if(!Success)
        {
            Scene4ManagerScript.DisableAllPanels();

            Scene4ManagerScript.MessageText.text = "Something went wrong, try again later.";
            Scene4ManagerScript.EnableMessagePanel();

            Scene4ManagerScript.EnableProfileMenuPanel();

            yield break;
        }

        UpdateUI();

        Scene4ManagerScript.DisableAllPanels();
        Scene4ManagerScript.EnableViewProfileInfoPanel();
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
        Scene4ManagerScript.EnableInvisiblePanel();

        Scene4ManagerScript.EnableUpdateProfilePanel();

        StartCoroutine(UpdateProfileScript.UpdateProfileInfo());
    }

    public void ClickedPreviousPageButton()
    {
        Scene4ManagerScript.DisableAllPanels();
        Scene4ManagerScript.EnableProfileMenuPanel();
    }
}
