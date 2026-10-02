//namespaces =
using UnityEngine;




public class PrinterStatusManager : MonoBehaviour
{
    //This is the prefab using which PrinterStatusPanel is instantiated.
    [SerializeField] GameObject PrinterStatusPanelPrefab;

    //This is the parent which contains PrinterStatusPanels as its children.
    [SerializeField] GameObject PrinterStatusPanelsParent;







    //This is the function which is called by HomePage script -> ClickedPrintersStatusDropdown().
    //It is the function which is responsible for instantiation of PrinterStatusPanels.
    public void CreatePrinterStatusPanels()
    {
        foreach (Printer printer in Script1.SelectedPrinters)
        {
            CreatePrinterStatusPanel(printer);
        }
    }


    //This function is called by CreatePrinterStatusPanels().
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




    //This is called when PrinterStatusManager panel is to be turned off. 
    //This function deletes all exisitng PrinterStatusPanels.
    public void DestroyAllPrinterStatusPanels()
    {
        foreach (PrinterStatusPanel Panel in PrinterStatusPanelsParent.GetComponentsInChildren<PrinterStatusPanel>())
        {
            Destroy(Panel.gameObject);
        }
    }
}
