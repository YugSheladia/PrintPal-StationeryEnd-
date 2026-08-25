using System.Collections.Generic;
using UnityEngine;

using static Scene2Backend;

public class UpdatePrinters : MonoBehaviour
{
    [SerializeField] Scene5Manager Scene5ManagerScript;
    [SerializeField] Scene5Backend Scene5BackendScript;

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
            Scene5BackendScript.GetPrinters
            (
                (printers) =>
                {
                    if (printers == null)
                    {
                        Debug.LogError("Failed to retrieve printers.");

                        Scene5ManagerScript.MessageText.text =
                            "No connected printers were found\nMake sure all printers are connected ( via USB or Bluetooth )";

                        OnComplete?.Invoke(false);
                        return;
                    }

                    foreach (PrinterInfo printerInfo in printers)
                    {
                        Printer printer = new Printer()
                        {
                            Name = printerInfo.name,

                            IsSelected = false,

                            IsOnline = true,
                            IsBusy = false,

                            AutomaticPrinting = false,

                            SupportsDoubleSidedPrinting = false,
                            SupportsColorPrinting = false
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

        panel.DisableViewPrinterPanel_Stuff();

        PrinterList.Add(panel);
    }




    public void ClickedProceed()
    {
        Scene5ManagerScript.EnableInvisiblePanel();

        List<Printer> SelectedPrinters = new();

        foreach (PrinterPanel Panel in PrinterList)
        {
            if (Panel.Printer != null && Panel.Printer.IsSelected)
            {
                SelectedPrinters.Add(Panel.Printer);
            }
        }

        Scene5BackendScript.SaveApprovedPrinters
        (
            SelectedPrinters,
            Success =>
            {
                Scene5ManagerScript.DisableInvisiblePanel();

                if (!Success)
                {
                    Scene5ManagerScript.DisableInvisiblePanel();

                    Scene5ManagerScript.MessageText.text = "Failed to save approved printers.";
                    Scene5ManagerScript.EnableMessagePanel();

                    return;
                }

                Script1.SetPrinters(SelectedPrinters);

                Script1.PrintAllPrinterDetails();

                Scene5ManagerScript.DisableInvisiblePanel();

                Scene5ManagerScript.MessageText.text = "Printer selection has been updated.";
                Scene5ManagerScript.EnableMessagePanel();
            }
        );
    }
    public void ClickedPreviousPage()
    {
        Scene5ManagerScript.DisableAllPanels();
        Scene5ManagerScript.EnableSettingsPanel();
    }
}