using UnityEngine;
using UnityEngine.UI;
using static Scene2Backend;

public class SelectPrinters : MonoBehaviour
{
    [SerializeField] Scene2Manager Scene2ManagerScript;
    [SerializeField] Scene2Backend Scene2BackendScript;

    //[SerializeField] PrinterPanel PrinterPanelScript;

    [SerializeField] GameObject PrinterPanelPrefab;
    [SerializeField] GameObject PrinterPanelsParent;
     
    


    


    public void GetConnectedPrinters()
    {
        StartCoroutine
        (
            Scene2BackendScript.GetPrinters
            (
                (Printers) =>
                {
                    if (Printers == null)
                    {
                        Debug.LogError("Failed to retrieve printers.");
                        return;
                    }

                    foreach (PrinterInfo Printer in Printers)
                    {
                        Debug.Log($"Name           : {Printer.name}");
                        //Debug.Log($"Online         : {Printer.online}");
                        Debug.Log($"Status         : {Printer.status}");
                        //Debug.Log($"Supports Color : {Printer.supportsColor}");
                        Debug.Log($"Default        : {Printer.isDefault}");
                        Debug.Log("------------------------------");

                        CreatePrinterPanel(Printer.name, Printer.status);
                    }
                    Scene2ManagerScript.DisableAllPanels();
                    Scene2ManagerScript.EnableSelectPrintersPanel();
                }
            )
        );
    }
    public void CreatePrinterPanel(string PrinterName, string PrinterStatus)
    {
        GameObject PrintPanelObject = Instantiate(PrinterPanelPrefab, PrinterPanelsParent.transform);

        PrinterPanel PrinterPanelScript = PrintPanelObject.GetComponent<PrinterPanel>();

        PrinterPanelScript.SetPrinterDetails(PrinterName, PrinterStatus);

        PrinterPanelScript.UpdateUI();
    }
}
