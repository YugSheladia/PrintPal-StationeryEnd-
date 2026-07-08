using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchedOrderStatus : MonoBehaviour
{
    Scene3Manager           Scene3ManagerScript;
    Scene3Backend           Scene3BackendScript;
    SearchedOrderStatusManager   SearchedOrderStatusManagerScript;

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

    [SerializeField] GameObject ArchivePanel;
    [SerializeField] GameObject PendingPanel;
    [SerializeField] GameObject CompletedPanel;
    [SerializeField] GameObject CollectedPanel;

    [SerializeField] Image ViewDetailsColor;

    [SerializeField] Image StatusIndicator;


    GameObject SearchedOrderStatusInfoObject;


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


    public void SetRequiredScripts(
                                    Scene3Manager Scene3ManagerScript,
                                    Scene3Backend Scene3BackendScript,
                                    SearchedOrderStatusManager SearchedOrderStatusManagerScript
                                  )
    {
        this.Scene3ManagerScript = Scene3ManagerScript;
        this.Scene3BackendScript = Scene3BackendScript;
        this.SearchedOrderStatusManagerScript = SearchedOrderStatusManagerScript;
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
        archiveStatusEnum = (ArchiveStatusEnum)Order.ArchivedByStationery;

        switch(orderStatusEnum)
        {
            case OrderStatusEnum.Pending:
                DisableAllPanels();
                EnablePendingPanel();
                
                StatusTimeStampText.text = "Order Created at " + Order.CreatedAt;
                StatusTimeStampText.color = new Color32(246, 59, 57, 200);

                ViewDetailsColor.color = new Color32(246, 59, 57, 200);

                StatusIndicator.color = new Color32(246, 59, 57, 200);
                
                break;

            case OrderStatusEnum.Completed:
                DisableAllPanels();
                EnableCompletedPanel();

                StatusTimeStampText.text = "Order Completed at " + Order.CompletedAt;
                StatusTimeStampText.color = new Color32(245, 207, 73, 230);

                ViewDetailsColor.color = new Color32(245, 207, 73, 230);

                StatusIndicator.color = new Color32(245, 207, 73, 230);

                break;

            case OrderStatusEnum.Collected:
                DisableAllPanels();
                EnableCollectedPanel();

                StatusTimeStampText.text = "Order Collected at " + Order.CollectedAt;
                StatusTimeStampText.color = new Color32(2, 141, 26, 220);

                ViewDetailsColor.color = new Color32(2, 141, 26, 220);

                StatusIndicator.color = new Color32(2, 141, 26, 220);

                break;

            default:
                DisableAllPanels();
           
                Debug.Log("Invalid Order Status");
                StatusTimeStampText.text = "?";

                break;
        }

        switch (archiveStatusEnum)
        {
            case ArchiveStatusEnum.Unarchived:
                break;

            case ArchiveStatusEnum.Archived:
                DisableAllPanels();
                EnableArchivePanel();

                StatusTimeStampText.color = new Color32(36, 36, 36, 150);

                ViewDetailsColor.color = new Color32(36, 36, 36, 150);

                StatusIndicator.color = new Color32(36, 36, 36, 150);
                break;
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

        SearchedOrderStatusManagerScript.SearchedOrdersStatusPanel.SetActive(false);
    }
    private void CreateOrderStatusInfo()
    {
        SearchedOrderStatusInfoObject = Instantiate(
                                                    OrderStatusInfoPrefab,
                                                    SearchedOrderStatusManagerScript.transform,
                                                    false
                                                  );

        SearchedOrderStatusInfo SearchedOrderStatusInfoScript = 
            SearchedOrderStatusInfoObject.GetComponent<SearchedOrderStatusInfo>();

        SearchedOrderStatusInfoScript.SetScene3ManagerScript(Scene3ManagerScript);
        SearchedOrderStatusInfoScript.SetSearchedOrdersStatusPanel(SearchedOrderStatusManagerScript.SearchedOrdersStatusPanel);

        SearchedOrderStatusInfoListParentScript Parent = 
            SearchedOrderStatusInfoObject.GetComponentInChildren<SearchedOrderStatusInfoListParentScript>();

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

        if (archiveStatusEnum == ArchiveStatusEnum.Archived)
        {
            Scene3BackendScript.SetOrderAsUnarchived(Order.OrderID);
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

        if (archiveStatusEnum == ArchiveStatusEnum.Archived)
        {
            Scene3BackendScript.SetOrderAsUnarchived(Order.OrderID);
        }

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

        if (archiveStatusEnum == ArchiveStatusEnum.Archived)
        {
            Scene3BackendScript.SetOrderAsUnarchived(Order.OrderID);
        }

        Scene3BackendScript.SetOrderAsCollected(Order.OrderID);

        Destroy(gameObject);
    }
    public void ClickedArchiveOrderButton()
    {
        if (Order == null)
        {
            Debug.Log("Order is absent inside SearchedOrderStatus script.");
            return;
        }

        Scene3BackendScript.SetOrderAsArchived(Order.OrderID);

        Destroy(gameObject);
    }




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
}
