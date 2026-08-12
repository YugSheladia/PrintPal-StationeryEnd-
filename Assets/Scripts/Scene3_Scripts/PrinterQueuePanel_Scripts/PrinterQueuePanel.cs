using UnityEngine;
using TMPro;

public class PrinterQueuePanel : MonoBehaviour
{
    PrintManager PrintManagerScript;


    [SerializeField] public GameObject QueuedOrderPrefab;
    [SerializeField] public GameObject QueuedOrdersParent;

    [SerializeField] public TMP_Text PrinterName;
    [SerializeField] public TMP_Text PrinterNumber;



    PrinterWorker Worker;




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
    }
}
