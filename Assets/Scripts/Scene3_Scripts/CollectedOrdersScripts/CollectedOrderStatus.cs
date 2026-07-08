using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CollectedOrderStatus : MonoBehaviour
{
    Scene3Manager                   Scene3ManagerScript;
    Scene3Backend                   Scene3BackendScript;
    CollectedOrderStatusManager     CollectedOrderStatusManagerScript;

    [Header("Prefabs")]
    [SerializeField] GameObject OrderStatusInfoPrefab;
    [SerializeField] GameObject PrintBillPrefab;
    [SerializeField] GameObject Print_FinalBillPrefab;

    [Header("UI")]
    public string UsernameString;
    [SerializeField] TMP_Text Username;

    [SerializeField] TMP_Text OrderIDText;
    [SerializeField] TMP_Text OrderOTP;
    [SerializeField] TMP_Text CollectedAtText; 

    

    GameObject CollectedOrderStatusInfoObject;


    public void SetRequiredScripts(
                                   Scene3Manager Scene3ManagerScript,
                                   Scene3Backend Scene3BackendScript,
                                   CollectedOrderStatusManager CollectedOrderStatusManagerScript
                                 )
    {
        this.Scene3ManagerScript = Scene3ManagerScript;
        this.Scene3BackendScript = Scene3BackendScript;
        this.CollectedOrderStatusManagerScript = CollectedOrderStatusManagerScript;
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

        OrderIDText.text        = Order.OrderID.ToString();
        OrderOTP.text           = Order.OrderOTP.ToString();

        if(Order.CollectedAt == null)
        {
            Debug.Log("CollectedAt = null");
            CollectedAtText.text = "unknown";
        }
        else
        {
            CollectedAtText.text = Order.CollectedAt.ToString();
        }

        print("OrderID          = " + Order.OrderID.ToString()      +
              "\nOrderOTP       = " + Order.OrderOTP.ToString()     +
              "\nCollected at   = " + Order.CollectedAt);
    }


    

    public void ClickedOrderStatusPanel()
    {
        if (Order == null)
        {
            Debug.LogError("Could not find OrderSummary Object.");
            return;
        }

        CreateOrderStatusInfo();

        CollectedOrderStatusManagerScript.CollectedOrdersStatusPanel.SetActive(false);
    }
    private void CreateOrderStatusInfo()
    {
        CollectedOrderStatusInfoObject = Instantiate(
                                                        OrderStatusInfoPrefab,
                                                        CollectedOrderStatusManagerScript.transform,
                                                        false
                                                    );

        CollectedOrderStatusInfo CollectedOrderStatusInfoScript = 
            CollectedOrderStatusInfoObject.GetComponent<CollectedOrderStatusInfo>();

        CollectedOrderStatusInfoScript.SetScene3ManagerScript(Scene3ManagerScript);
        CollectedOrderStatusInfoScript.SetCollectedOrdersStatusPanel(CollectedOrderStatusManagerScript.CollectedOrdersStatusPanel);

        CollectedOrderStatusInfoListParentScript Parent = 
            CollectedOrderStatusInfoObject.GetComponentInChildren<CollectedOrderStatusInfoListParentScript>();

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



    public void ClickedMarkAsPending()
    {
        if (Order == null)
        {
            Debug.Log("Order is absent inside CollectedOrderStatus script.");
            return;
        }

        Scene3BackendScript.SetOrderAsPending(Order.OrderID);

        Destroy(gameObject);
    }
    public void ClickedMarkAsCompleted()
    {
        if (Order == null)
        {
            Debug.Log("Order is absent inside CollectedOrderStatus script.");
            return;
        }

        Scene3BackendScript.SetOrderAsCompleted(Order.OrderID);

        Destroy(gameObject);
    }
    public void ClickedArchiveOrderButton()
    {
        if (Order == null)
        {
            Debug.Log("Order is absent inside CollectedOrderStatus script.");
            return;
        }

        Scene3BackendScript.SetOrderAsArchived(Order.OrderID);

        Destroy(gameObject);
    }
}
