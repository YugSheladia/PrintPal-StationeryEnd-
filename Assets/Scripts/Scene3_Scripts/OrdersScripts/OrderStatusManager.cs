using UnityEngine;

public class OrderStatusManager : MonoBehaviour
{
    [SerializeField] Scene3Manager      Scene3ManagerScript;
    [SerializeField] Scene3Backend      Scene3BackendScript;


    [Header("Order Status")]
    [SerializeField] GameObject OrderStatusPrefab;
    [SerializeField] GameObject OrderStatusListParent;


    [SerializeField] public GameObject OrdersStatusPanel;



    public void CreateOrderStatusPanels()
    {
        foreach (OrderSummary Order in Scene3BackendScript.ConfirmedOrders.Orders)
        {
            //Debug.Log("OrderID = " + Order.OrderID);

            CreateOrderStatusPanel(Order);
        }
    }
    public void CreateOrderStatusPanel(OrderSummary Order)
    {
        foreach (OrderStatus Panel in OrderStatusListParent.GetComponentsInChildren<OrderStatus>())
        {
            if (Panel.Order.OrderID == Order.OrderID) { return; }
        }

        GameObject OrderStatusObject = Instantiate(
                                                    OrderStatusPrefab,
                                                    OrderStatusListParent.transform
                                                  );

        OrderStatus OrderStatusScript = OrderStatusObject.GetComponent<OrderStatus>();

        OrderStatusScript.SetRequiredScripts(Scene3ManagerScript, Scene3BackendScript, this);

        OrderStatusScript.SetOrder(Order);

        Scene3BackendScript.GetUsernameFromServer(Order.UserID, OrderStatusScript);
    }


    

    public void ClickedPreviousPageButton()
    {
        DeleteAllOrderPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableHomePagePanel();
    }
    private void DeleteAllOrderPanels()
    {
        foreach (Transform Child in OrderStatusListParent.transform)
        {
            Destroy(Child.gameObject);
        }
    }



    public void RemoveOrderPanel(int OrderID)
    {
        foreach (OrderStatus Panel in OrderStatusListParent.GetComponentsInChildren<OrderStatus>())
        {
            if (Panel.Order.OrderID == OrderID)
            {
                Destroy(Panel.gameObject);
                return;
            }
        }
    }
}
