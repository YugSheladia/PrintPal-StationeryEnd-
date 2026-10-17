//namespaces =
using UnityEngine;




public class OrderStatusManager : MonoBehaviour
{
    //Script references =
    [SerializeField] Scene3Manager      Scene3ManagerScript;
    [SerializeField] Scene3Backend      Scene3BackendScript;

    [SerializeField] HomePage HomePageScript;




    [Header("Order Status")]

    //Prefab of OrderStatus panel.
    [SerializeField] GameObject OrderStatusPrefab;

    //An object which acts as a parent to OrderStatus panels.
    [SerializeField] GameObject OrderStatusListParent;

    //A reference to OrderStatus panel itself.
    [SerializeField] public GameObject OrdersStatusManagerPanel;






    //Called by HomePage when a user clicks on Pending / Printed / Collected orders.
    public void CreateOrderStatusPanels()
    {
        if (Scene3BackendScript.ConfirmedOrders.Orders.Length <= 0)

        {
            //In future, if I ever want to make a "No Orders exist" wala object, it should be instantitated here.

            return;
        }

        foreach (OrderSummary Order in Scene3BackendScript.ConfirmedOrders.Orders)
        {
            CreateOrderStatusPanel(Order);
        }
    }

    //Called by CreateOrderStatusPanels() to instantiate a panel for each order in Scene3Backend.ConfirmedOrders.Orders
    public void CreateOrderStatusPanel(OrderSummary Order)
    {
        //To check if the same order already exists.
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
    }




    

    //To go from OrderStatus page to HomePage.
    public void ClickedPreviousPageButton()
    {
        DeleteAllOrderPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableHomePagePanel();
    }

    //To delete all OrderStatus panels before leaving the OrderStatus page.
    private void DeleteAllOrderPanels()
    {
        foreach (Transform Child in OrderStatusListParent.transform)
        {
            Destroy(Child.gameObject);
        }
    }






    //Dunno what this is used for, but looks useful when one wants to delete a OrderStatus panel of a specific OrderID.
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
