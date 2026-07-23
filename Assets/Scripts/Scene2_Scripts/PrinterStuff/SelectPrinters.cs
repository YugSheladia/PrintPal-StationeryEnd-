using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Scene2Backend;

public class SelectPrinters : MonoBehaviour
{
    [SerializeField] Scene2Manager Scene2ManagerScript;
    [SerializeField] Scene2Backend Scene2BackendScript;

    [SerializeField] GameObject PrinterPanelPrefab;
    [SerializeField] GameObject PrinterPanelsParent;

    public List<PrinterPanel> PrinterList = new();



    public void GetConnectedPrinters(System.Action<bool> OnComplete)
    {
        foreach (Transform child in PrinterPanelsParent.transform)
        {
            Destroy(child.gameObject);
        }

        PrinterList.Clear();

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
                        Debug.Log($"Status  : {printerInfo.status}");
                        Debug.Log($"Default : {printerInfo.isDefault}");
                        Debug.Log("------------------------------");

                        Printer printer = new Printer()
                        {
                            Name = printerInfo.name,

                            IsSelected = false,

                            IsOnline =
                                printerInfo.status != "Offline",

                            IsBusy =
                                printerInfo.status == "Busy"
                                || printerInfo.status == "Printing",

                            AutomaticPrinting = false
                        };

                        CreatePrinterPanel(printer);
                    }

                    OnComplete?.Invoke(true);
                }
            )
        );
    }



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



    public void ClickedProceed()
    {
        Scene2ManagerScript.EnableInvisiblePanel();

        List<Printer> SelectedPrinters = new();

        foreach (PrinterPanel Panel in PrinterList)
        {
            if (Panel.Printer != null && Panel.Printer.IsSelected)
            {
                SelectedPrinters.Add(Panel.Printer);
            }
        }

        Scene2BackendScript.SaveApprovedPrinters
        (
            SelectedPrinters,
            Success =>
            {
                Scene2ManagerScript.DisableInvisiblePanel();

                if (!Success)
                {
                    Scene2ManagerScript.MessageText.text = "Failed to save approved printers.";

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