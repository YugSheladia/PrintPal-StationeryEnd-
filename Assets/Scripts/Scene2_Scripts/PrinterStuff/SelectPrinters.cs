//namespaces = 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Scene2Backend;







public class SelectPrinters : MonoBehaviour
{
    //Reference scripts =
    [SerializeField] Scene2Manager Scene2ManagerScript;
    [SerializeField] Scene2Backend Scene2BackendScript;






    //Printer Panel prefab =
    [SerializeField] GameObject PrinterPanelPrefab;

    //A parent to hold all printer panels =
    [SerializeField] GameObject PrinterPanelsParent;

    //A list to store preffered printers.
    public List<PrinterPanel> PrinterList = new();






    //Get all connected printers and their information =
    public void GetConnectedPrinters(System.Action<bool> OnComplete)
    {
        //To empty PrinterPanelsParent before the process begins.
        foreach (Transform child in PrinterPanelsParent.transform)
        {
            Destroy(child.gameObject);
        }

        //To clear the PrinterList before the process begins.
        PrinterList.Clear();


        //To call the GetPrinters coroutine =
        StartCoroutine
        (
            Scene2BackendScript.GetPrinters
            (
                (printers) =>
                {
                    if (printers == null)
                    {
                        Debug.LogError
                        (
                            "Failed to retrieve printers."
                        );

                        OnComplete?.Invoke(false);
                        return;
                    }

                    foreach (PrinterInfo printerInfo in printers)
                    {
                        Debug.Log($"Name    : {printerInfo.name}");
                        Debug.Log("------------------------------");

                        Printer printer = new Printer()
                        {
                            Name = printerInfo.name,

                            IsSelected = false,

                            //IsOnline = true as this status is not decided by the server, but instead by the printer's actual activity.
                            IsOnline = true,

                            //IsBusy = false as this status is not decided byt the server, but instead by the printer's actual activity.
                            IsBusy = false,


                            //new =
                            SupportsColorPrinting = printerInfo.supportsColorPrinting,
                            SupportsDoubleSidedPrinting = printerInfo.supportsDoubleSidedPrinting,

                            PPM = printerInfo.PPM,

                            //This status isn't used yet, and hence it kept false by default.
                            AutomaticPrinting = false,

                            //Page Capacity is 100 by default.
                            PageCapacity = 100,
                            PagesLeft = 100
                        };

                        CreatePrinterPanel(printer);
                    }

                    OnComplete?.Invoke(true);
                }
            )
        );
    }







    //Creating Printer panel =
    void CreatePrinterPanel(Printer printer)
    {
        GameObject panelObject = Instantiate
        (
            PrinterPanelPrefab,
            PrinterPanelsParent.transform
        );

        PrinterPanel panel = panelObject.GetComponent<PrinterPanel>();

        panel.SetRequiredScripts(Scene2ManagerScript);
        panel.SetPrinter(printer);

        PrinterList.Add(panel);
    }







    //To proceed and save printer preference.
    public void ClickedProceed()
    {
        //To make sure any other input button is not clicked by the user while printer preference is being saved.
        Scene2ManagerScript.EnableInvisiblePanel();

        



        //Converting each PrinterPanel object -> Printer object =
        List<Printer> SelectedPrinters = new();

        foreach (PrinterPanel Panel in PrinterList)
        {
            if (Panel.Printer != null && Panel.Printer.IsSelected)
            {
                SelectedPrinters.Add(Panel.Printer);
            }
        }





        //Printer preference saving process =
        Scene2BackendScript.SaveApprovedPrinters
        (
            SelectedPrinters,
            Success =>
            {
                if (!Success)
                {
                    Scene2ManagerScript.DisableInvisiblePanel();

                    Scene2ManagerScript.MessageText.text = "Failed to save approved printers\nRestart the app and try again later.";
                    Scene2ManagerScript.EnableMessagePanel();

                    return;
                }

                //Calling this function to get the StationeryPrinterID of the new saved printers.
                StartCoroutine(GetPrinterInfo_AfterSavingIt());
            }
        );
    }


    //It is important to call this Coroutine as we need all printer's StationeryPrinterID after we upate the printer list in Scene5.
    IEnumerator GetPrinterInfo_AfterSavingIt()
    {
        bool Success = false;

        yield return StartCoroutine(Scene2BackendScript.GetApprovedPrintersFromServer(Script1.StationeryInfoObject.StationeryID, Result => Success = Result));

        if (!Success)
        {
            Scene2ManagerScript.DisableInvisiblePanel();

            Scene2ManagerScript.MessageText.text = "Couldn't get Printer Info after saving it.";
            Scene2ManagerScript.EnableMessagePanel();

            yield break;
        }

        Script1.PrintAllPrinterDetails();

        Scene2ManagerScript.MessageText.text = "Printer selection has been succesfully updated.";
        Scene2ManagerScript.EnableMessagePanel();

        SceneManager.LoadScene("Scene3_HomePage");
    }
}