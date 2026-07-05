using UnityEngine;

public class PendingOrderStatusManager : MonoBehaviour
{
    [SerializeField] Scene3Manager      Scene3ManagerScript;
    [SerializeField] Scene3Backend      Scene3BackendScript;


    [Header("Order Status")]
    [SerializeField] GameObject OrderStatusPrefab;
    [SerializeField] GameObject PendingOrderStatusListParent;

    [SerializeField] public GameObject PendingOrdersStatusPanel;



    public void CreateOrderStatusPanels()
    {
        foreach (OrderSummary Order in Scene3BackendScript.ConfirmedOrders.Orders)
        {
            Debug.Log("OrderID = " + Order.OrderID);

            CreateOrderStatusPanel(Order);
        }
    }
    public void CreateOrderStatusPanel(OrderSummary Order)
    {
        foreach (PendingOrderStatus Panel in PendingOrderStatusListParent.GetComponentsInChildren<PendingOrderStatus>())
        {
            if (Panel.Order.OrderID == Order.OrderID) { return; }
        }

        GameObject OrderStatusObject = Instantiate(
                                                    OrderStatusPrefab,
                                                    PendingOrderStatusListParent.transform
                                                  );

        PendingOrderStatus PendingOrderStatusScript = OrderStatusObject.GetComponent<PendingOrderStatus>();

        PendingOrderStatusScript.SetRequiredScripts(
                                                        Scene3ManagerScript, 
                                                        Scene3BackendScript,
                                                        this
                                                   );

        PendingOrderStatusScript.SetOrder(Order);

        Scene3BackendScript.GetUsernameFromServer(Order.UserID, PendingOrderStatusScript);
    }


    

    public void ClickedPreviousPageButton()
    {
        DeleteAllPendingOrderPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableHomePagePanel();
    }
    private void DeleteAllPendingOrderPanels()
    {
        foreach (Transform Child in PendingOrderStatusListParent.transform)
        {
            Destroy(Child.gameObject);
        }
    }
}
