using UnityEngine;

public class ViewPrinters : MonoBehaviour
{
    [SerializeField] Scene5Manager Scene5ManagerScript;

    [SerializeField] GameObject PrinterListParent;
    [SerializeField] GameObject PrinterPanelPrefab;



    public void CreatePrinterPanels()
    {
        foreach (Transform child in PrinterListParent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Printer printer in Script1.SelectedPrinters)
        {
            GameObject printerPanelObject =
                Instantiate(PrinterPanelPrefab, PrinterListParent.transform);

            PrinterPanel printerPanelScript =
                printerPanelObject.GetComponent<PrinterPanel>();

            printerPanelScript.SetPrinter(printer);
            printerPanelScript.DisableToggle();
        }
    }


    public void ClickedPreviousPage()
    {
        Scene5ManagerScript.DisableAllPanels();
        Scene5ManagerScript.EnableSettingsPanel();
    }
}
