//namespaces =
using System.Collections.Generic;
using UnityEngine;





public class QueueManager : MonoBehaviour
{
    //Script references =
    [SerializeField] Scene3Manager  Scene3ManagerScript;
    [SerializeField] Scene3Backend  Scene3BackendScript;
    [SerializeField] PrintManager   PrintManagerScript;



    //This is the prefab used for instantiating PrinterQueuePanel.
    [SerializeField] GameObject PrinterQueuePanelPrefab;

    //This object acts as a parent to all PrinterQueuePanels.
    [SerializeField] GameObject PrinterQueuePanelsParent;


    //This is a list of PrinterQueuePanels.
    public List<PrinterQueuePanel> PrinterQueuePanel_List = new List<PrinterQueuePanel>();









    //=====================================================================================================================
    //flow = RefreshQueuedOrders -> CreatePrinterQueuePanels() -> CreatePrinterQueuePanel() + Update_PagesLeftIndicator
    //=====================================================================================================================

    //This function is called to refresh all Queues in QueueManager page every time an order is Queued, Unqueued,
    //or when a user clicks on Queue button in HomePage, etc
   
    public void RefreshQueuedOrders()
    {
        //This makes it so that the PrinterQueueManager page is disabled & enabled automatically only if it was enabled in the first place. 
        if (Scene3ManagerScript.CheckQueueManagerPage_ActiveStatus())
        {
            //This is to clean, i.e. delete all PrinterQueue panels in QueueManager.
            DestroyAllPrinterQueuePanels();


            //After deleting, we again instantiate PrinterQueue panels based on the most recent information we have.
            CreatePrinterQueuePanels();


            Scene3ManagerScript.DisableQueuePage();
            Scene3ManagerScript.EnableQueuePage();
        }
    }


    //This is to create PrinterQueue panels in PrinterQueueManager.
    public void CreatePrinterQueuePanels()
    {
        PrinterQueuePanel_List.Clear();

        int PrinterNumber = 0;

        foreach (PrinterWorker Worker in PrintManagerScript.Workers)
        {
            PrinterNumber++;
            CreatePrinterQueuePanel(Worker, PrinterNumber);
        }
    }


    //This function is called by CreatePrinterQueuePanels().
    private void CreatePrinterQueuePanel(PrinterWorker Worker, int PrinterNumber)
    {
        GameObject PrinterQueuePanel = Instantiate(PrinterQueuePanelPrefab, PrinterQueuePanelsParent.transform);
        PrinterQueuePanel PrinterQueuePanelScript = PrinterQueuePanel.GetComponent <PrinterQueuePanel>();

        PrinterQueuePanel_List.Add(PrinterQueuePanelScript);

        PrinterQueuePanelScript.SetRequiredScripts(Scene3BackendScript, PrintManagerScript);
        PrinterQueuePanelScript.SetPrintWorker(Worker, PrinterNumber);

        PrinterQueuePanelScript.UpdateUI();

        //This is to create the QueuedOrder panels inside each PrinterQueue panel.
        PrinterQueuePanelScript.CreateQueuedOrders(Worker);

        Update_PagesLeftIndicator(PrinterQueuePanelScript);
    }


    private void Update_PagesLeftIndicator(PrinterQueuePanel PrinterQueuePanelScript)
    {
        float PagesLeft = PrinterQueuePanelScript.Worker.Printer.PagesLeft;
        float PageCapacity = PrinterQueuePanelScript.Worker.Printer.PageCapacity;

        float PagesLeft_Percentage = (PagesLeft / PageCapacity) * 100;

        Debug.Log("PagesLeft    = " + PagesLeft);
        Debug.Log("PageCapacity = " + PageCapacity);
        Debug.Log("PagesLeftPercentage = " + PagesLeft_Percentage);

        PrinterQueuePanelScript.DisableAll_PagesLeftIndicatorPanels();

        if (PagesLeft_Percentage < 10)
        {
            PrinterQueuePanelScript.PagesLeftIndicator_Caution.SetActive(true);
            return;
        }
        if (PagesLeft_Percentage < 33)
        {
            PrinterQueuePanelScript.PagesLeftIndicator_Low.SetActive(true);
            return;
        }
        if (PagesLeft_Percentage < 66)
        {
            PrinterQueuePanelScript.PagesLeftIndicator_Medium.SetActive(true);
            return;
        }

        PrinterQueuePanelScript.PagesLeftIndicator_High.SetActive(true);
    }
















    //This function is used to go from QueueManager page -> HomePage.
    private void ClickedPreviousPageButton()
    {
        DestroyAllPrinterQueuePanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableHomePagePanel();
    }

    //This function is called by ClickedPreviousPageButton() & RefreshQueuedOrders().
    //It is used to delete all PrinterQueuePanels before leaving PrinterQueueManager page.
    private void DestroyAllPrinterQueuePanels()
    {
        foreach (Transform Child in PrinterQueuePanelsParent.transform)
        {
            Destroy(Child.gameObject);
        }
    }
}
