using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] Scene5Manager Scene5ManagerScript;
    [SerializeField] Scene5Backend Scene5BackendScript;

    [SerializeField] ViewPrinters ViewPrintersScript;
    [SerializeField] UpdatePrinters UpdatePrintersScript;

    [SerializeField] GameObject ErrorPanel;


    [SerializeField] private GameObject ToggleOffImage;
    [SerializeField] private GameObject ToggleOnImage;

    //bool Toogle = false;





    //Removed the ability to turn AutomaticPrinting On / Off through Settings.
    /*
    private void Start()
    {
        GetAutomaticPrintingStatus();
    }

    public void GetAutomaticPrintingStatus()
    {
        StartCoroutine
        (
            Scene5BackendScript.GetAutomaticPrinting
            (
                Script1.StationeryInfoObject.StationeryID,
                AutomaticPrinting =>
                {
                    if (AutomaticPrinting == null)
                    {
                        Debug.LogError("Failed to retrieve automatic printing setting.");

                        Scene5ManagerScript.MessageText.text = "Failed to get automatic printing settings from server\n" +
                                                                "Restart the app and try again.";
                        Scene5ManagerScript.EnableMessagePanel();

                        ErrorPanel.SetActive(true);

                        return;
                    }

                    ErrorPanel.SetActive(false);

                    Toogle = AutomaticPrinting.Value;
                    Script1.SetAutomaticPrintingValue(Toogle);

                    UpdateUI();
                }
            )
        );
    }
    
    public void ClickedAutomaticPrintingToggleButton()
    {
        Scene5ManagerScript.EnableInvisiblePanel();

        Toogle = !Toogle;

        StartCoroutine
        (
            Scene5BackendScript.SetAutomaticPrinting
            (
                Script1.StationeryInfoObject.StationeryID,
                Toogle,
                Success =>
                {
                    if (!Success)
                    {
                        Toogle = !Toogle;
                        Scene5ManagerScript.MessageText.text = "Something went wrong, try again later.";
                        Scene5ManagerScript.EnableMessagePanel();

                        Scene5ManagerScript.DisableInvisiblePanel();
                        return;
                    }

                    UpdateUI();

                    if(Toogle)
                    {
                        Scene5ManagerScript.MessageText.text = "Automatic printing has been turned on.";
                    }
                    else
                    {
                        Scene5ManagerScript.MessageText.text = "Automatic printing has been turned off.";
                    }
                    Scene5ManagerScript.EnableMessagePanel();

                    Script1.SetAutomaticPrintingValue(Toogle);

                    Scene5ManagerScript.DisableInvisiblePanel();
                }
            )
        );
    }
    public void UpdateUI()
    {
        ToggleOnImage.SetActive(Toogle);
        ToggleOffImage.SetActive(!Toogle);
    }
    */



    public void ClickedViewSelectedPrinters()
    {
        Scene5ManagerScript.EnableInvisiblePanel();

        StartCoroutine
        (
            Scene5BackendScript.GetApprovedPrintersFromServer
            (
                Script1.StationeryInfoObject.StationeryID,
                Result =>
                {
                    Scene5ManagerScript.DisableInvisiblePanel();

                    if (!Result)
                    {
                        Scene5ManagerScript.DisableAllPanels();
                        Scene5ManagerScript.EnableMessagePanel();

                        return;
                    }

                    if (Script1.SelectedPrinters.Count <= 0)
                    {
                        Scene5ManagerScript.MessageText.text = "No printers have been selected\n Click on Select new printers in Settings.";

                        Scene5ManagerScript.DisableAllPanels();

                        Scene5ManagerScript.EnableSettingsPanel();
                        Scene5ManagerScript.EnableMessagePanel();

                        return;
                    }

                    Scene5ManagerScript.MessageText.text = "Total printers currently in use : " + Script1.SelectedPrinters.Count;

                    ViewPrintersScript.CreatePrinterPanels();

                    Scene5ManagerScript.DisableAllPanels();

                    Scene5ManagerScript.EnableMessagePanel();
                    Scene5ManagerScript.EnableViewPrintersPanel();
                }
            )
        );
    }
    public void ClickedUpdatePrinters()
    {
        Scene5ManagerScript.EnableInvisiblePanel();
        Scene5ManagerScript.EnableUpdatePrintersPanel();

        GetPrinters();
    }
    public void GetPrinters()
    {
        UpdatePrintersScript.GetConnectedPrinters
        (
            Result =>
            {
                if (Result)
                {
                    Scene5ManagerScript.DisableAllPanels();

                    Scene5ManagerScript.EnableUpdatePrintersPanel();
                    Scene5ManagerScript.EnableMessagePanel();
                }
                else
                {
                    Scene5ManagerScript.DisableAllPanels();

                    Scene5ManagerScript.EnableSettingsPanel();
                    Scene5ManagerScript.EnableMessagePanel();
                }
            }
        );
    }




    public void ClickedPreviousPage()
    {
        SceneManager.LoadScene("Scene3_HomePage");
    }
}
