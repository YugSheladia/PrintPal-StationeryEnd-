//namespaces = 
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
                        //Debug.Log($"Status  : {printerInfo.status}");
                        //Debug.Log($"Default : {printerInfo.isDefault}");
                        Debug.Log("------------------------------");

                        Printer printer = new Printer()
                        {
                            Name = printerInfo.name,

                            IsSelected = false,

                            //IsOnline = true as this status is not decided by the server, but instead by the printer's actual activity.
                            IsOnline = true,

                            //IsBusy = false as this status is not decided byt the server, but instead by the printer's actual activity.
                            IsBusy = false,

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
                Scene2ManagerScript.DisableInvisiblePanel();

                if (!Success)
                {
                    Scene2ManagerScript.MessageText.text = "Failed to save approved printers\nRestart the app and try again later.";

                    Scene2ManagerScript.EnableMessagePanel();

                    return;
                }

                Script1.SetPrinters(SelectedPrinters);

                Script1.PrintAllPrinterDetails();

                SceneManager.LoadScene("Scene3_HomePage");
            }
        );
    }
}