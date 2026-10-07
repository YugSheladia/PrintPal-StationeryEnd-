//namespaces =
using UnityEngine;
using UnityEngine.SceneManagement;





public class Settings : MonoBehaviour
{
    //Script references =
    [SerializeField] Scene5Manager Scene5ManagerScript;
    [SerializeField] Scene5Backend Scene5BackendScript;

    [SerializeField] ViewPrinters ViewPrintersScript;
    [SerializeField] UpdatePrinters UpdatePrintersScript;





    //==============================
    //Button functions =
    //==============================
    
    //To view previously selected printers.
    public void ClickedViewSelectedPrinters()
    {
        //To prevent user from pressing any button while the below process is occuring.
        Scene5ManagerScript.EnableInvisiblePanel();

        //This coroutine gets the selected printers from the server's database, and saves it inside Script1.
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
                        Scene5ManagerScript.DisableAllPanels();
                        Scene5ManagerScript.EnableSettingsPanel();

                        Scene5ManagerScript.MessageText.text = "No printers have been selected\n Click on Select new printers in Settings.";
                        Scene5ManagerScript.EnableMessagePanel();

                        return;
                    }


                    ViewPrintersScript.CreatePrinterPanels();

                    Scene5ManagerScript.DisableAllPanels();
                    Scene5ManagerScript.EnableViewPrintersPanel();

                    Scene5ManagerScript.MessageText.text = "Total printers currently in use : " + Script1.SelectedPrinters.Count;
                    Scene5ManagerScript.EnableMessagePanel();
                }
            )
        );
    }





    //To update printer selection.
    public void ClickedUpdatePrinters()
    {
        Scene5ManagerScript.EnableInvisiblePanel();
        Scene5ManagerScript.EnableUpdatePrintersPanel();

        GetPrinters();
    }

    //ClickedUpdatePrinters() -> GetPrinters() -> UpdatePrintersScript.GetConnectedPrinters()
    public void GetPrinters()
    {
        //This retrieves all the printers which have been stored in the computer's history.
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
}
