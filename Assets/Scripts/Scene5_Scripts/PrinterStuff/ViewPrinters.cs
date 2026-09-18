//namespaces =
using UnityEngine;
using UnityEngine.UI;








public class ViewPrinters : MonoBehaviour
{
    //Script references =
    [SerializeField] Scene5Manager Scene5ManagerScript;





    //This is the prefab used to instantiate printer panels.
    [SerializeField] GameObject PrinterPanelPrefab;

    //This objects acts as a parent to all Printer panels.
    [SerializeField] GameObject PrinterListParent;

    




    //This is a reference to the Scroll which exists in ViewPrinters page.
    [SerializeField] ScrollRect scrollRect;








    //Called by ClickedViewSelectedPrinters() to make Printer panels in ViewPrinters page.
    public void CreatePrinterPanels()
    {
        //Deleting all panels in PrinterListParent ( to cleanup )
        foreach (Transform child in PrinterListParent.transform)
        {
            Destroy(child.gameObject);
        }

    

        foreach (Printer printer in Script1.SelectedPrinters)
        {
            GameObject printerPanelObject =
                Instantiate(PrinterPanelPrefab, PrinterListParent.transform);




            //=====================================
            //The below 2 lines of code didnt work
            //=====================================

            //To make it so the PrinterPanelObjects are instantiated starting from the top of the ScrollRect.
            printerPanelObject.transform.SetAsFirstSibling();

            //This is to make it so that the Scroll View is scrolled to the top.
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;





            PrinterPanel printerPanelScript =
                printerPanelObject.GetComponent<PrinterPanel>();


            printerPanelScript.SetPrinter(printer);

            //This is to set active the tick mark / cross mark, indicating whether they support Color / Double sided printing or not.
            printerPanelScript.EnableViewPrinterPanel_Stuff();
            
            //This is to deny users to change a printer's settings ( Enabling / Disabling Double Sided / Color ) through ViewPrinters page.
            printerPanelScript.DisableToggles();
        }
    }




    //Called to go from ViewPrinters -> Settings.
    public void ClickedPreviousPage()
    {
        Scene5ManagerScript.DisableAllPanels();
        Scene5ManagerScript.EnableSettingsPanel();
    }
}
