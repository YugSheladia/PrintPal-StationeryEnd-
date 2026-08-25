using UnityEngine;
using TMPro;

public class PrinterQueuePanel : MonoBehaviour
{
    PrintManager PrintManagerScript;


    [SerializeField] public GameObject QueuedOrderPrefab;
    [SerializeField] public GameObject QueuedOrdersParent;

    [SerializeField] public TMP_Text PrinterName;
    [SerializeField] public TMP_Text PrinterNumber;

    [SerializeField] public GameObject Operational;
    [SerializeField] public GameObject NonOperational;

    PrinterWorker Worker;

    [SerializeField] public GameObject ColorTMP;
    [SerializeField] public GameObject DoubleSidedTMP;
    [SerializeField] public GameObject Color_DoubleSidedTMP;






    public void SetPrintManagerScript(PrintManager PrintManagerScript)
    {
        this.PrintManagerScript = PrintManagerScript;
    }






    public void CreateQueuedOrders(PrinterWorker Worker)
    {
        this.Worker = Worker;

        foreach(PrintJob Job in Worker.Queue.Jobs)
        {
            CreateQueuedOrder(Job);
        }
    }
    public void CreateQueuedOrder(PrintJob Job)
    {
        GameObject QueuedOrder = Instantiate(QueuedOrderPrefab, QueuedOrdersParent.transform);
        QueuedOrder QueuedOrderScript = QueuedOrder.GetComponent<QueuedOrder>();

        QueuedOrderScript.OrderID.text = "OrderID: " + Job.OrderID;
    }






    public void ClickedJumpStartButton()
    {
        PrintManagerScript.JumpStartPrinter(PrinterName.text);

        PrinterIs_Operational();
    }
    public void ClickedDisablePrinterButton()
    {
        PrintManagerScript.DisablePrinter(PrinterName.text);

        PrinterIs_NonOperational();
    }





    public void PrinterIs_Operational()
    {
        NonOperational.SetActive(false);
        Operational.SetActive(true);
    }
    public void PrinterIs_NonOperational()
    {
        Operational.SetActive(false);
        NonOperational.SetActive(true);
    }
}
