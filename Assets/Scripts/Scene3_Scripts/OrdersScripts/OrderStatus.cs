//namespaces =
using TMPro;
using UnityEngine;
using UnityEngine.UI;





public class OrderStatus : MonoBehaviour
{
    //Script references =
    Scene3Manager           Scene3ManagerScript;
    Scene3Backend           Scene3BackendScript;
    OrderStatusManager      OrderStatusManagerScript;




    //=============================
    [Header("Prefabs")]
    //=============================

    //Prefabs for =
    //1. PrintBlls ( Bills of each Print of an Order ).
    //2. Print_FinalBill 
    //3. OrderStatusInfo ( contains the PrintBills & Print_FinalBill ).
    [SerializeField] GameObject OrderStatusInfoPrefab;
    [SerializeField] GameObject PrintBillPrefab;
    [SerializeField] GameObject Print_FinalBillPrefab;




    //=================
    [Header("UI")]
    //=================

    //Name of the Customer.
    public string UsernameString;
    [SerializeField] TMP_Text Username;

    //Time at which the order was recieved.
    [SerializeField] TMP_Text StatusTimeStampText;

    //OrderID & OTP.
    [SerializeField] TMP_Text OrderIDText;
    [SerializeField] TMP_Text OrderOTP;


    //Depending upon the status of the Order, one of these panels will be activated.
    //This is done as the positioning of buttons, and which buttons are available to use, change on the basis of the status of the order.
    [SerializeField] GameObject ArchivePanel;
    [SerializeField] GameObject PendingPanel;
    [SerializeField] GameObject CompletedPanel;
    [SerializeField] GameObject CollectedPanel;



    //This changes based on the status of the order as well.
    //e.g. Red for Pending, Yellow for Printed, etc.
    [SerializeField] Image StatusBoxColor;
    [SerializeField] TMP_Text StatusBoxText;


    //This is a reference to the OrderStatusInfo object itself, which contains everything mentioned above.
    GameObject OrderStatusInfoObject;




    //============
    //Enums
    //============

    enum OrderStatusEnum
    {
        Pending,
        Completed,
        Collected
    }
    OrderStatusEnum orderStatusEnum;

    enum ArchiveStatusEnum
    {
        Unarchived,
        Archived
    }
    ArchiveStatusEnum archiveStatusEnum;












    //Setting the required scripts after instantiating OrderStatus object.
    public void SetRequiredScripts(
                                    Scene3Manager Scene3ManagerScript,
                                    Scene3Backend Scene3BackendScript,
                                    OrderStatusManager OrderStatusManagerScript
                                  )
    {
        this.Scene3ManagerScript = Scene3ManagerScript;
        this.Scene3BackendScript = Scene3BackendScript;
        this.OrderStatusManagerScript = OrderStatusManagerScript;
    }







    //OrderSummary is The object which contains all information about an order.
    public OrderSummary Order;
    public void SetOrder(OrderSummary Order)
    {
        this.Order = Order;
    }
    public void UpdateUI()
    {
        orderStatusEnum = (OrderStatusEnum)Order.Status;
        archiveStatusEnum = (ArchiveStatusEnum)Order.ArchivedByStationery;


        //Changing OrderStatus panel on the basis of it's status =
        switch(orderStatusEnum)
        {
            case OrderStatusEnum.Pending:

                DisableAllPanels();
                EnablePendingPanel();

                StatusBoxText.text = "Pending";
                StatusBoxColor.color = new Color32(246, 59, 57, 200);
                
                StatusTimeStampText.text = "Order Created at " + Order.CreatedAt;
                
                break;

            case OrderStatusEnum.Completed:

                DisableAllPanels();
                EnableCompletedPanel();

                StatusBoxText.text = "Printed";
                StatusBoxColor.color = new Color32(245, 207, 73, 230);

                StatusTimeStampText.text = "Order Completed at " + Order.CompletedAt;

                break;

            case OrderStatusEnum.Collected:

                DisableAllPanels();
                EnableCollectedPanel();

                StatusBoxText.text = "Collected";
                StatusBoxColor.color = new Color32(2, 141, 26, 220);

                StatusTimeStampText.text = "Order Collected at " + Order.CollectedAt;
 
                break;

            default:

                DisableAllPanels();
           
                Debug.Log("Invalid Order Status");
                StatusTimeStampText.text = "?";

                break;
        }


        //Changing OrderStatus panel on the basis of it's archival status =
        switch (archiveStatusEnum)
        {
            case ArchiveStatusEnum.Unarchived:
                break;

            case ArchiveStatusEnum.Archived:
                DisableAllPanels();
                EnableArchivePanel();

                StatusBoxText.text = "Archived";
                StatusBoxColor.color = new Color32(255, 255, 255, 255);

                break;
        }


        //Other vital information =
        Username.text = UsernameString;

        OrderIDText.text                = Order.OrderID.ToString();
        OrderOTP.text                   = Order.OrderOTP.ToString();
    }


    



    //This is the function which is called when user clicks on 'View Details' inside OrderStatus panel.
    public void ClickedOrderStatusPanel()
    {
        if (Order == null)
        {
            Debug.LogError("Could not find OrderSummary Object.");
            return;
        }

        CreateOrderStatusInfo();

        //This is to turn off OrderStatusManager panel.
        OrderStatusManagerScript.OrdersStatusManagerPanel.SetActive(false);
    }

    //This is called by ClickedOrderStatusPanel().
    //It creates all Bills.
    private void CreateOrderStatusInfo()
    {
        //Instantiation of OrderStatusInfo object.
        //OrderStatusInfo contains = Bills (s) + FinalBill.
        OrderStatusInfoObject = Instantiate(
                                                    OrderStatusInfoPrefab,
                                                    OrderStatusManagerScript.transform,
                                                    false
                                                  );

        OrderStatusInfo OrderStatusInfoScript = 
            OrderStatusInfoObject.GetComponent<OrderStatusInfo>();

        //Providing reference to the required scripts.
        OrderStatusInfoScript.SetScene3ManagerScript(Scene3ManagerScript);
        
        //Providing reference to this OrderStatusManager panel itself.
        OrderStatusInfoScript.SetOrdersStatusPanel(OrderStatusManagerScript.OrdersStatusManagerPanel);

        //This is to obtain the reference of the OrderStatusInfoListParent ( which is a child of OrderStatusInfo ).
        //This object will act as a parent to all Bills as well as the FinalBill.
        OrderStatusInfoListParentScript Parent = 
            OrderStatusInfoObject.GetComponentInChildren<OrderStatusInfoListParentScript>();

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

    //Called By CreateOrderStatusInfo() for final bill creation.
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






    //These are functions to mark a specific Order as Pending / Completed / Collected / Arhived.
    public void ClickedMarkAsPending()
    {
        if (Order == null)
        {
            Debug.Log("Order is absent inside CollectedOrderStatus script.");
            return;
        }

        Scene3ManagerScript.EnableInvisiblePanel();

        if (archiveStatusEnum == ArchiveStatusEnum.Archived)
        {
            Scene3BackendScript.SetOrderAsUnarchived(Order.OrderID, this.gameObject);
        }

        Scene3BackendScript.SetOrderAsPending(Order.OrderID, this.gameObject);

        //Destroy(gameObject);
    }
    public void ClickedMarkAsCompleted()
    {
        if (Order == null)
        {
            Debug.Log("Order is absent inside CollectedOrderStatus script.");
            return;
        }

        Scene3ManagerScript.EnableInvisiblePanel();

        if (archiveStatusEnum == ArchiveStatusEnum.Archived)
        {
            Scene3BackendScript.SetOrderAsUnarchived(Order.OrderID, this.gameObject);
        }

        Scene3BackendScript.SetOrderAsCompleted(Order.OrderID, this.gameObject);

        //Destroy(gameObject);
    }
    public void ClickedMarkAsCollected()
    {
        if (Order == null)
        {
            Debug.Log("Order is absent inside RTCOrderStatus script.");
            return;
        }

        Scene3ManagerScript.EnableInvisiblePanel();

        if (archiveStatusEnum == ArchiveStatusEnum.Archived)
        {
            Scene3BackendScript.SetOrderAsUnarchived(Order.OrderID, this.gameObject);
        }

        Scene3BackendScript.SetOrderAsCollected(Order.OrderID, this.gameObject);

        //Destroy(gameObject);
    }
    public void ClickedArchiveOrderButton()
    {
        if (Order == null)
        {
            Debug.Log("Order is absent inside OrderStatus script.");
            return;
        }

        Scene3ManagerScript.EnableInvisiblePanel();

        Scene3BackendScript.SetOrderAsArchived(Order.OrderID, this.gameObject);

        //Destroy(gameObject);
    }
    public void ClickedUnarchiveOrderButton()
    {
        if (Order == null)
        {
            Debug.Log("Order is absent inside OrderStatus script.");
            return;
        }

        Scene3ManagerScript.EnableInvisiblePanel();

        Scene3BackendScript.SetOrderAsUnarchived(Order.OrderID, this.gameObject);

        //Destroy(gameObject);
    }



    //==================================================================================================
    //These panels are made as the UI differs depending upon the state of the Order.
    //As in the placement of Mark as Pending / Printed / Collected buttons differ from state to state.
    //==================================================================================================

    public void DisableAllPanels()
    {
        DisableArchivePanel();
        DisablePendingPanel();
        DisableCompletedPanel();
        DisableCollectedPanel();
    }


    public void EnableArchivePanel()
    {
        ArchivePanel.SetActive(true);
    }
    public void DisableArchivePanel()
    {
        ArchivePanel.SetActive(false);
    }
    public void EnablePendingPanel()
    {
        PendingPanel.SetActive(true);
    }
    public void DisablePendingPanel()
    {
        PendingPanel.SetActive(false);
    }
    public void EnableCompletedPanel()
    {
        CompletedPanel.SetActive(true);
    }
    public void DisableCompletedPanel()
    {
        CompletedPanel.SetActive(false);
    }
    public void EnableCollectedPanel()
    {
        CollectedPanel.SetActive(true);
    }
    public void DisableCollectedPanel()
    {
        CollectedPanel.SetActive(false);
    }






    //This button is used to open PDFs of all prints of an order.
    public void ClickedOpenButton()
    {
        StartCoroutine
        (
            Scene3BackendScript.Coroutine_DownloadAllPDFs
            (
                Order,
                Success =>
                {
                    if (!Success)
                    {
                        Scene3ManagerScript.MessageText.text =
                            "PrintPal couldn't open a PDF, try again later.";

                        Scene3ManagerScript.EnableMessagePanel();
                        Scene3ManagerScript.DisableInvisiblePanel();

                        return;
                    }

                    foreach (PrintDetails Print in Order.Prints)
                    {
                        Application.OpenURL
                        (
                            "file://" + Print.LocalPDFPath
                        );
                    }
                }
            )
        );
    }



    //This is to manually print an Order.
    //This is where we begin when we restructure the printing architecture.
    public void ClickedPrintButton()
    {
        Debug.Log("Clicked Print Button.");

        //Scene3ManagerScript.EnableInvisiblePanel();

        Scene3BackendScript.StartPrintJob(Order);
    }
}
