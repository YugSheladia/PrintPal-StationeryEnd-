//namespaces =
using UnityEngine;
using TMPro;









public class PrinterQueuePanel : MonoBehaviour
{
    //Script references =
    PrintManager PrintManagerScript;


    //This is the prefab used to instantiate a QueuedOrder object inside a PrinterQueue panel.
    [SerializeField] public GameObject QueuedOrderPrefab;

    //This is the parent of all QueuedOrder objects in a PrinterQueue panel.
    [SerializeField] public GameObject QueuedOrdersParent;


    //UI stuff =
    [SerializeField] public TMP_Text PrinterName;
    [SerializeField] public TMP_Text PrinterNumber;


    //Seperate panels to indicate weather the printer is operational or not.
    [SerializeField] public GameObject Operational;
    [SerializeField] public GameObject NonOperational;


    //Reference to Worker, i.e. printer, whose queue is represented by this specific PrinterQueue panel.
    PrinterWorker Worker;


    //Panels to indicate if the printer supports DoubleSided, Color, etc.

    [SerializeField] private GameObject Color_SingleSidedTMP;
    [SerializeField] private GameObject Color_DoubleSidedTMP;

    [SerializeField] private GameObject Monochrome_SingleSidedTMP;
    [SerializeField] private GameObject Monochrome_DoubleSidedTMP;





    //Page Capacity stuff =
    [SerializeField] private TMP_Text PagesRemaining;











    //This function provides PrinterQueuePanel script a reference of PrintManager script.
    public void SetPrintManagerScript(PrintManager PrintManagerScript)
    {
        this.PrintManagerScript = PrintManagerScript;
    }





    //This function is called by QueueManager -> CreatePrinterQueuePanel().
    public void CreateQueuedOrders(PrinterWorker Worker)
    {
        this.Worker = Worker;

        //For each Job the workers has in it's queue, a QueueOrder object is instantiated.
        foreach(PrintJob Job in Worker.Queue.Jobs)
        {
            CreateQueuedOrder(Job);
        }
    }

    //This function is called by CreateQueuedOrders().
    public void CreateQueuedOrder(PrintJob Job)
    {
        GameObject QueuedOrder = Instantiate(QueuedOrderPrefab, QueuedOrdersParent.transform);
        QueuedOrder QueuedOrderScript = QueuedOrder.GetComponent<QueuedOrder>();

        QueuedOrderScript.OrderID.text = "OrderID: " + Job.OrderID;
    }

    //QueueManager script -> CreatePrinterQueuePanel() -> UpdateUI()
    public void UpdateUI(PrinterWorker Worker, int WorkerNumber)
    {
        //This is to mark the printer as Operational / Non Operational.
        if (Worker.WorkedLastTime)
        {
            PrinterIs_Operational();
        }
        else
        {
            PrinterIs_NonOperational();
        }


        PrinterName.text = Worker.Printer.Name;
        PrinterNumber.text = WorkerNumber.ToString();


        //This tags the Printer with the type of print it supports.
        if(Worker.Printer.SupportsColorPrinting)
        {
            if(Worker.Printer.SupportsDoubleSidedPrinting)
            {
                Color_DoubleSidedTMP.SetActive(true);
            }
            else
            {
                Color_SingleSidedTMP.SetActive(true);
            }
        }
        else
        {
            if (Worker.Printer.SupportsDoubleSidedPrinting)
            {
                Monochrome_DoubleSidedTMP.SetActive(true);
            }
            else
            {
                Monochrome_SingleSidedTMP.SetActive(true);
            }
        }

        //Have to call this function after posting total pages available, to make sure there is synchronicity.

        //This function displays the total page capacity of a printer tray, as well as the pages currently left ( both approx ).
        PagesRemaining.text =   "Pages remaining : " + 
                                Worker.Printer.PagesLeft.ToString() + 
                                " / " + 
                                Worker.Printer.PageCapacity.ToString();
    }





    //This function is called when a user clicks on 'Activate' button.
    public void ClickedJumpStartButton()
    {
        PrintManagerScript.JumpStartPrinter(PrinterName.text);

        PrinterIs_Operational();
    }

    //This function is called when a user clicks on 'Deactivate' button.
    public void ClickedDisablePrinterButton()
    {
        PrintManagerScript.DisablePrinter(PrinterName.text);

        PrinterIs_NonOperational();
    }





    //This function is called by =
    //QueueManager script -> CreatePrinterQueuePanel()
    //PrinterQueuePanel script -> ClickedJumpStartButton()
    public void PrinterIs_Operational()
    {
        NonOperational.SetActive(false);
        Operational.SetActive(true);
    }


    //This function is called by =
    //QueueManager script -> CreatePrinterQueuePanel()
    //PrinterQueuePanel script -> ClickedDisablePrinterButton()
    public void PrinterIs_NonOperational()
    {
        Operational.SetActive(false);
        NonOperational.SetActive(true);
    }
}
