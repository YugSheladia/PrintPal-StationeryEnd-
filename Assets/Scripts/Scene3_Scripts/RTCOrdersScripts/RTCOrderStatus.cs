using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RTCOrderStatus : MonoBehaviour
{
    Scene3Manager           Scene3ManagerScript;
    Scene3Backend           Scene3BackendScript;
    RTCOrderStatusManager   RTCOrderStatusManagerScript;

    [Header("Prefabs")]
    [SerializeField] GameObject OrderStatusInfoPrefab;
    [SerializeField] GameObject PrintBillPrefab;
    [SerializeField] GameObject Print_FinalBillPrefab;

    [Header("UI")]
    public string UsernameString;
    [SerializeField] TMP_Text Username;

    [SerializeField] TMP_Text OrderIDText;
    [SerializeField] TMP_Text OrderOTP;
    [SerializeField] TMP_Text CompletedAtText; 

    

    GameObject RTCOrderStatusInfoObject;



    public void SetRequiredScripts(
                                    Scene3Manager Scene3ManagerScript,
                                    Scene3Backend Scene3BackendScript,
                                    RTCOrderStatusManager RTCOrderStatusManagerScript
                                  )
    {
        this.Scene3ManagerScript = Scene3ManagerScript;
        this.Scene3BackendScript = Scene3BackendScript;
        this.RTCOrderStatusManagerScript = RTCOrderStatusManagerScript;
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

        if (Order.CompletedAt == null)
        {
            Debug.Log("CompletedAt = null");
            CompletedAtText.text = "unknown";
        }
        else
        {
            CompletedAtText.text = Order.CompletedAt.ToString();
        }

        print("OrderID          = " + Order.OrderID.ToString()      +
              "\nOrderOTP       = " + Order.OrderOTP.ToString()     +
              "\nCompleted at   = " + Order.CompletedAt);
    }


    

    public void ClickedOrderStatusPanel()
    {
        if (Order == null)
        {
            Debug.LogError("Could not find OrderSummary Object.");
            return;
        }

        CreateOrderStatusInfo();

        RTCOrderStatusManagerScript.RTCOrdersStatusPanel.SetActive(false);
    }
    private void CreateOrderStatusInfo()
    {
        RTCOrderStatusInfoObject = Instantiate(
                                                    OrderStatusInfoPrefab,
                                                    RTCOrderStatusManagerScript.transform,
                                                    false
                                                  );

        RTCOrderStatusInfo RTCOrderStatusInfoScript = 
            RTCOrderStatusInfoObject.GetComponent<RTCOrderStatusInfo>();

        RTCOrderStatusInfoScript.SetScene3ManagerScript(Scene3ManagerScript);
        RTCOrderStatusInfoScript.SetRTCOrdersStatusPanel(RTCOrderStatusManagerScript.RTCOrdersStatusPanel);

        RTCOrderStatusInfoListParentScript Parent = 
            RTCOrderStatusInfoObject.GetComponentInChildren<RTCOrderStatusInfoListParentScript>();

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
            Debug.Log("Order is absent inside RTCOrderStatus script.");
            return;
        }

        Scene3BackendScript.SetOrderAsPending(Order.OrderID);

        Destroy(gameObject);
    }
    public void ClickedMarkAsCollected()
    {
        if (Order == null)
        {
            Debug.Log("Order is absent inside RTCOrderStatus script.");
            return;
        }

        Scene3BackendScript.SetOrderAsCollected(Order.OrderID);

        Destroy(gameObject);
    }
    public void ClickedArchiveOrderButton()
    {
        if (Order == null)
        {
            Debug.Log("Order is absent inside RTCOrderStatus script.");
            return;
        }

        Scene3BackendScript.SetOrderAsArchived(Order.OrderID);

        Destroy(gameObject);
    }
}
