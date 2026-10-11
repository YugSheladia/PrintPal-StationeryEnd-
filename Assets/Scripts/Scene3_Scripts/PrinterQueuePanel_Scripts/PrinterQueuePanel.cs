//namespaces =
using TMPro;
using UnityEngine;
using static Printer;









public class PrinterQueuePanel : MonoBehaviour
{
    //Script references =
    Scene3Backend Scene3BackendScript;
    PrintManager PrintManagerScript;
    QueueManager QueueManagerScript;


    //Printer Number ( to display in PrinterQueuePanel )
    int PrinterNumber;


    //Reference to Worker, i.e. printer, whose queue is represented by this specific PrinterQueue panel.
    public PrinterWorker Worker;


    //This is the prefab used to instantiate a QueuedOrder object inside a PrinterQueue panel.
    [SerializeField] public GameObject QueuedOrderPrefab;

    //This is the parent of all QueuedOrder objects in a PrinterQueue panel.
    [SerializeField] public GameObject QueuedOrdersParent;


    //UI stuff =
    [SerializeField] public TMP_Text PrinterName;
    [SerializeField] public TMP_Text PrinterNumberText;


    //Seperate panels to indicate weather the printer is operational or not.
    [SerializeField] public GameObject Operational;
    [SerializeField] public GameObject NonOperational;



    //Panels to indicate if the printer supports DoubleSided, Color, etc.

    [SerializeField] private GameObject Color_SingleSidedTMP;
    [SerializeField] private GameObject Color_DoubleSidedTMP;

    [SerializeField] private GameObject Monochrome_SingleSidedTMP;
    [SerializeField] private GameObject Monochrome_DoubleSidedTMP;




    //Page Capacity stuff =
    [SerializeField] private TMP_Text PagesRemaining;

    [SerializeField] public GameObject PagesLeftIndicator_Caution;
    [SerializeField] public GameObject PagesLeftIndicator_Low;
    [SerializeField] public GameObject PagesLeftIndicator_Medium;
    [SerializeField] public GameObject PagesLeftIndicator_High;








    //This function provides PrinterQueuePanel script a reference to the required scripts.
    public void SetRequiredScripts(Scene3Backend Scene3BackendScript, QueueManager QueueManagerScript, PrintManager PrintManagerScript)
    {
        this.Scene3BackendScript = Scene3BackendScript;
        this.QueueManagerScript = QueueManagerScript;
        this.PrintManagerScript = PrintManagerScript;
    }


    //This function sets the PrintWorker as well as PrintNumber in the PrinterQueuePanel script itself.
    public void SetPrintWorker(PrinterWorker Worker, int PrinterNumber)
    {
        this.Worker         = Worker;
        this.PrinterNumber  = PrinterNumber;
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
    public void UpdateUI()
    {
        PrinterName.text = Worker.Printer.Name;
        PrinterNumberText.text = PrinterNumber.ToString();






        //This is to mark the printer as Operational / Non Operational.
        if (Worker.WorkedLastTime)
        {
            PrinterIs_Operational();
        }
        else
        {
            PrinterIs_NonOperational();
        }


        



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





        //UpdateUI is called by ClickedRefillPages() -> RefillPages().
        //Hence we are updating the PagesLeftIndicator as well.
        Update_PagesLeftIndicator();
    }


    //UpdateUI() -> Update_PagesLeftIndicator().
    //This updates the PagesLeft indicator to Caution, Low, Medium, or High depending upon the PagesLeft / PageCapacity * 100.
    public void Update_PagesLeftIndicator()
    {
        //Calling Update_PagesLeftIndicator again ( after calling it in PrintWorker.UpdatePagesLocally ).
        //This is because UpdatePagesLocally is called only after printing a docuemnt, but when a user just logs in, we need to update the PagesLeftIndicator.
        Worker.Printer.Update_PagesLeftIndicator();


        //This disables all indicator panels to start fresh.
        DisableAll_PagesLeftIndicatorPanels();

        Debug.Log("Current Mode = " + Worker.Printer.PagesLeft_IndicatorEnum);

        //This checks the current PagesLeft_Indicator of the printer, and activates the appropriate PagesLeft_Indicator panel.
        if (Worker.Printer.PagesLeft_IndicatorEnum == PagesLeft_Indicator.Caution)
        {
            PagesLeftIndicator_Caution.SetActive(true);
            return;
        }
        if (Worker.Printer.PagesLeft_IndicatorEnum == PagesLeft_Indicator.Low)
        {
            PagesLeftIndicator_Low.SetActive(true);
            return;
        }
        if (Worker.Printer.PagesLeft_IndicatorEnum == PagesLeft_Indicator.Medium)
        {
            PagesLeftIndicator_Medium.SetActive(true);
            return;
        }

        PagesLeftIndicator_High.SetActive(true);
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









    //======================
    //PagesLeft stuff 
    //======================

    //====================================================================================
    //These functions are used to refill the pages of a printer to it's max capacity.
    //i.e. Printer.PagesLeft = Printer.PageCapacity.
    //====================================================================================
    public void ClickedRefilPagesButton()
    {
        RefilPages();
    }

    private void RefilPages()
    {
        StartCoroutine
        (
            Scene3BackendScript.RefillPrinterPages
            (
                Worker.Printer.StationeryPrinterID,
                Success =>
                {
                    if (Success)
                    {
                        Debug.Log("Printer refilled.");

                        Worker.Printer.RefillPagesTo_PageCapacity();

                        UpdateUI();
                    }
                    else
                    {
                        Debug.LogWarning("Failed to refill printer.");
                    }
                }
            )
        );
    }






  







    //Disable All PagesLeft_Indicator panels
    public void DisableAll_PagesLeftIndicatorPanels()
    {
        PagesLeftIndicator_Caution. SetActive(false);
        PagesLeftIndicator_Low.     SetActive(false);
        PagesLeftIndicator_Medium.  SetActive(false);
        PagesLeftIndicator_High.    SetActive(false);
    }
}
