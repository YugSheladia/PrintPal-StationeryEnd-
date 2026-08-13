using UnityEngine;

public class QueueManager : MonoBehaviour
{
    [SerializeField] Scene3Manager Scene3ManagerScript;

    [SerializeField] PrintManager PrintManagerScript;

    [SerializeField] GameObject PrinterQueuePanelPrefab;
    [SerializeField] GameObject PrinterQueuePanelsParent;




    public void CreatePrinterQueuePanels()
    {
        int PrinterNumber = 0;

        foreach (PrinterWorker Worker in PrintManagerScript.Workers)
        {
            PrinterNumber++;
            CreatePrinterQueuePanel(Worker, PrinterNumber);
        }
    }
    private void CreatePrinterQueuePanel(PrinterWorker Worker, int PrinterNumber)
    {
        GameObject PrinterQueuePanel = Instantiate(PrinterQueuePanelPrefab, PrinterQueuePanelsParent.transform);
        PrinterQueuePanel PrinterQueuePanelScript = PrinterQueuePanel.GetComponent <PrinterQueuePanel>();

        if(Worker.WorkedLastTime)
        {
            PrinterQueuePanelScript.PrinterIs_Operational();
        }
        else
        {
            PrinterQueuePanelScript.PrinterIs_NonOperational();
        }

        PrinterQueuePanelScript.SetPrintManagerScript(PrintManagerScript);

        PrinterQueuePanelScript.PrinterName.text = Worker.Printer.Name;
        PrinterQueuePanelScript.PrinterNumber.text = PrinterNumber.ToString();

        PrinterQueuePanelScript.CreateQueuedOrders(Worker);
    }






    public void RefreshQueuedOrders()
    {
        DestroyAllPrinterQueuePanels();

        CreatePrinterQueuePanels();

        if(Scene3ManagerScript.CheckQueuePage_ActiveStatus())
        {
            Scene3ManagerScript.DisableQueuePage();
            Scene3ManagerScript.EnableQueuePage();
        }
    }










    private void ClickedPreviousPageButton()
    {
        DestroyAllPrinterQueuePanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableHomePagePanel();
    }
    private void DestroyAllPrinterQueuePanels()
    {
        foreach (Transform Child in PrinterQueuePanelsParent.transform)
        {
            Destroy(Child.gameObject);
        }
    }
}
