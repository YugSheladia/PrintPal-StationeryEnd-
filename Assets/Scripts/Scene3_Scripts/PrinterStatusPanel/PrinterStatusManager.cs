using Unity.VisualScripting;
using UnityEngine;

public class PrinterStatusManager : MonoBehaviour
{
    [SerializeField] GameObject PrinterStatusPanelPrefab;
    [SerializeField] GameObject PrinterStatusPanelsParent;


    public void CreatePrinterStatusPanels()
    {
        foreach (Printer printer in Script1.SelectedPrinters)
        {
            CreatePrinterStatusPanel(printer);
        }
    }

    public void CreatePrinterStatusPanel(Printer printer)
    {
        GameObject PrinterStatusPanel = Instantiate(PrinterStatusPanelPrefab, PrinterStatusPanelsParent.transform);
        PrinterStatusPanel PrinterStatusPanelScript = PrinterStatusPanel.GetComponent<PrinterStatusPanel>();

        PrinterStatusPanelScript.PrinterName.text = printer.Name;

        if(printer.IsOnline)
        {
            PrinterStatusPanelScript.PrinterStatus.text = "Working";

            PrinterStatusPanelScript.PrinterNotWorking.SetActive(false);
            PrinterStatusPanelScript.PrinterWorking.SetActive(true);
        }
        else
        {
            PrinterStatusPanelScript.PrinterStatus.text = "Not Working";

            PrinterStatusPanelScript.PrinterWorking.SetActive(false);
            PrinterStatusPanelScript.PrinterNotWorking.SetActive(true);
        }     
    }

    public void DestroyAllPrinterStatusPanels()
    {
        foreach (PrinterStatusPanel Panel in PrinterStatusPanelsParent.GetComponentsInChildren<PrinterStatusPanel>())
        {
            Destroy(Panel.gameObject);
        }
    }
}
