using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PendingOrderStatus : MonoBehaviour
{
    Scene3Manager               Scene3ManagerScript;
    Scene3Backend               Scene3BackendScript;
    PendingOrderStatusManager   PendingOrderStatusManagerScript;

    [Header("Prefabs")]
    [SerializeField] GameObject OrderStatusInfoPrefab;
    [SerializeField] GameObject PrintBillPrefab;
    [SerializeField] GameObject Print_FinalBillPrefab;

    [Header("UI")]
    public string UsernameString;
    [SerializeField] TMP_Text Username;

    [SerializeField] TMP_Text OrderIDText;
    [SerializeField] TMP_Text OrderOTP;
    [SerializeField] TMP_Text CreatedAt; 

    

    GameObject PendingOrderStatusInfoObject;



    public void SetRequiredScripts(
                                    Scene3Manager Scene3ManagerScript,
                                    Scene3Backend Scene3BackendScript,
                                    PendingOrderStatusManager PendingOrderStatusManagerScript
                                  )
    {
        this.Scene3ManagerScript = Scene3ManagerScript;
        this.Scene3BackendScript = Scene3BackendScript;
        this.PendingOrderStatusManagerScript = PendingOrderStatusManagerScript;
    }



    public OrderSummary Order;
    public void SetOrder(OrderSummary Order)
    {
        Debug.Log("Inside SetOrder()");
        this.Order = Order;
    }
    public void UpdateUI()
    {
        Username.text = UsernameString;

        OrderIDText.text    = Order.OrderID.ToString();
        OrderOTP.text       = Order.OrderOTP.ToString();

        if(Order.CreatedAt == null)
        {
            Debug.Log("Created At = null");
            CreatedAt.text = "unknown";
        }
        else
        {
            CreatedAt.text = Order.CreatedAt.ToString();
        }

        print("OrderID          = " + Order.OrderID.ToString() +
              "\nOrderOTP       = " + Order.OrderOTP.ToString() +
              "\nCreated at     = " + Order.CreatedAt);
    }




    public void ClickedOrderStatusPanel()
    {
        if (Order == null)
        {
            Debug.LogError("Could not find OrderSummary Object.");
            return;
        }

        CreateOrderStatusInfo();

        PendingOrderStatusManagerScript.PendingOrdersStatusPanel.SetActive(false);
    }
    private void CreateOrderStatusInfo()
    {
        PendingOrderStatusInfoObject = Instantiate(
                                                    OrderStatusInfoPrefab,
                                                    PendingOrderStatusManagerScript.transform,
                                                    false
                                                  );

        PendingOrderStatusInfo PendingOrderStatusInfoScript = 
            PendingOrderStatusInfoObject.GetComponent<PendingOrderStatusInfo>();

        PendingOrderStatusInfoScript.SetScene3ManagerScript(Scene3ManagerScript);
        PendingOrderStatusInfoScript.SetPendingOrdersStatusPanel(PendingOrderStatusManagerScript.PendingOrdersStatusPanel);

        PendingOrderStatusInfoListParentScript Parent = 
            PendingOrderStatusInfoObject.GetComponentInChildren<PendingOrderStatusInfoListParentScript>();

        if (Parent == null)
        {
            Debug.LogError("OrderStatusInfoListParentScript not found.");
            return;
        }

        Transform BillParent = Parent.transform;

        foreach (PrintDetails Print in Order.Prints)
        {
            GameObject BillObject = Instantiate(
                                                    PrintBillPrefab,
                                                    BillParent,
                                                    false
                                               );

            PrintBill BillScript = BillObject.GetComponent<PrintBill>();

            //BillScript.SetPrintBillsManagerScript(PrintBillsManagerScript);
            BillScript.SetBillDetailsInBillScript(Print);
        }

        CreateFinalBill(BillParent);
    }
    private void CreateFinalBill(Transform BillParent)
    {
        GameObject FinalBillObject = Instantiate(Print_FinalBillPrefab, BillParent, false);

        Print_FinalBill FinalBillScript = FinalBillObject.GetComponent<Print_FinalBill>();

        FinalBillScript.SetFinalBillAmount(
                                            Order.FinalAmount,
                                            Order.OrderID,
                                            Order.OrderOTP
                                          );
    }




    public void ClickedMarkAsCompleted()
    {
        if(Order == null)
        {
            Debug.Log("Order is absent inside PendingOrderStatus script.");
            return;
        }

        Scene3ManagerScript.EnableInvisiblePanel();

        Scene3BackendScript.SetOrderAsCompleted(Order.OrderID, this.gameObject);

        //Destroy(gameObject);
    }
    public void ClickedMarkAsCollected()
    {
        if (Order == null)
        {
            Debug.Log("Order is absent inside PendingOrderStatus script.");
            return;
        }

        Scene3ManagerScript.EnableInvisiblePanel();

        Scene3BackendScript.SetOrderAsCollected(Order.OrderID, this.gameObject);

        //Destroy(gameObject);
    }
    public void ClickedArchiveOrderButton()
    {
        if (Order == null)
        {
            Debug.Log("Order is absent inside PendingOrderStatus script.");
            return;
        }

        Scene3ManagerScript.EnableInvisiblePanel();

        Scene3BackendScript.SetOrderAsArchived(Order.OrderID, this.gameObject);

        //Destroy(gameObject);
    }



    public void DownloadAllPrintsFromOrder()
    {
        Scene3BackendScript.DownloadOrderPrintsFromServer(Order);
    }
}
