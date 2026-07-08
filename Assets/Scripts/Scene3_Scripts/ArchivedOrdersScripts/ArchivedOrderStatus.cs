using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ArchivedOrderStatus : MonoBehaviour
{
    Scene3Manager           Scene3ManagerScript;
    Scene3Backend           Scene3BackendScript;
    ArchivedOrderStatusManager   ArchivedOrderStatusManagerScript;

    [Header("Prefabs")]
    [SerializeField] GameObject OrderStatusInfoPrefab;
    [SerializeField] GameObject PrintBillPrefab;
    [SerializeField] GameObject Print_FinalBillPrefab;

    [Header("UI")]
    public string UsernameString;
    [SerializeField] TMP_Text Username;

    [SerializeField] TMP_Text StatusTimeStampText;

    [SerializeField] TMP_Text OrderIDText;
    [SerializeField] TMP_Text OrderOTP;


    GameObject ArchivedOrderStatusInfoObject;


    enum OrderStatusEnum
    {
        Pending,
        Completed,
        Collected
    }
    OrderStatusEnum orderStatusEnum;


    public void SetRequiredScripts(
                                    Scene3Manager Scene3ManagerScript,
                                    Scene3Backend Scene3BackendScript,
                                    ArchivedOrderStatusManager ArchivedOrderStatusManagerScript
                                  )
    {
        this.Scene3ManagerScript = Scene3ManagerScript;
        this.Scene3BackendScript = Scene3BackendScript;
        this.ArchivedOrderStatusManagerScript = ArchivedOrderStatusManagerScript;
    }




    public OrderSummary Order;
    public void SetOrder(OrderSummary Order)
    {
        Debug.Log("Inside SetOrder()");
        this.Order = Order;
    }
    public void UpdateUI()
    {
        orderStatusEnum = (OrderStatusEnum)Order.Status;

        switch(orderStatusEnum)
        {
            case OrderStatusEnum.Pending:
                StatusTimeStampText.text = "Order Created at " + Order.CreatedAt;
                
                break;

            case OrderStatusEnum.Completed:
                StatusTimeStampText.text = "Order Completed at " + Order.CompletedAt;

                break;

            case OrderStatusEnum.Collected:
                StatusTimeStampText.text = "Order Collected at " + Order.CollectedAt;

                break;

            default:
                Destroy(gameObject);
                
                Debug.Log("Invalid Order Status");

                return;
        }

        Username.text = UsernameString;

        OrderIDText.text                = Order.OrderID.ToString();
        OrderOTP.text                   = Order.OrderOTP.ToString();
        

        print("OrderID                      = " + Order.OrderID.ToString()      +
              "\nOrderOTP                   = " + Order.OrderOTP.ToString()     +
              "\nApproximate time left      = " + Order.ApproximateTimeLeft);
    }


    

    public void ClickedOrderStatusPanel()
    {
        if (Order == null)
        {
            Debug.LogError("Could not find OrderSummary Object.");
            return;
        }

        CreateOrderStatusInfo();

        ArchivedOrderStatusManagerScript.ArchivedOrdersStatusPanel.SetActive(false);
    }
    private void CreateOrderStatusInfo()
    {
        ArchivedOrderStatusInfoObject = Instantiate(
                                                    OrderStatusInfoPrefab,
                                                    ArchivedOrderStatusManagerScript.transform,
                                                    false
                                                  );

        ArchivedOrderStatusInfo ArchivedOrderStatusInfoScript = 
            ArchivedOrderStatusInfoObject.GetComponent<ArchivedOrderStatusInfo>();

        ArchivedOrderStatusInfoScript.SetScene3ManagerScript(Scene3ManagerScript);
        ArchivedOrderStatusInfoScript.SetArchivedOrdersStatusPanel(ArchivedOrderStatusManagerScript.ArchivedOrdersStatusPanel);

        ArchivedOrderStatusInfoListParentScript Parent = 
            ArchivedOrderStatusInfoObject.GetComponentInChildren<ArchivedOrderStatusInfoListParentScript>();

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

        Scene3BackendScript.SetOrderAsUnarchived(Order.OrderID);

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

        Scene3BackendScript.SetOrderAsUnarchived(Order.OrderID);

        Scene3BackendScript.SetOrderAsCompleted(Order.OrderID);

        Destroy(gameObject);
    }
    public void ClickedMarkAsCollected()
    {
        if (Order == null)
        {
            Debug.Log("Order is absent inside RTCOrderStatus script.");
            return;
        }

        Scene3BackendScript.SetOrderAsUnarchived(Order.OrderID);

        Scene3BackendScript.SetOrderAsCollected(Order.OrderID);

        Destroy(gameObject);
    }
}
