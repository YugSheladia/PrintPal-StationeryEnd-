using UnityEngine;

public class SearchedOrderStatusManager : MonoBehaviour
{
    [SerializeField] Scene3Manager      Scene3ManagerScript;
    [SerializeField] Scene3Backend      Scene3BackendScript;


    [Header("Order Status")]
    [SerializeField] GameObject OrderStatusPrefab;
    [SerializeField] GameObject SearchedOrderStatusListParent;


    [SerializeField] public GameObject SearchedOrdersStatusPanel;



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
        foreach (SearchedOrderStatus Panel in SearchedOrderStatusListParent.GetComponentsInChildren<SearchedOrderStatus>())
        {
            if (Panel.Order.OrderID == Order.OrderID) { return; }
        }

        GameObject OrderStatusObject = Instantiate(
                                                    OrderStatusPrefab,
                                                    SearchedOrderStatusListParent.transform
                                                  );

        SearchedOrderStatus SearchedOrderStatusScript = OrderStatusObject.GetComponent<SearchedOrderStatus>();

        SearchedOrderStatusScript.SetRequiredScripts(Scene3ManagerScript, Scene3BackendScript, this);

        SearchedOrderStatusScript.SetOrder(Order);

        Scene3BackendScript.GetUsernameFromServer(Order.UserID, SearchedOrderStatusScript);
    }


    

    public void ClickedPreviousPageButton()
    {
        DeleteAllSearchedOrderPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableHomePagePanel();
    }
    private void DeleteAllSearchedOrderPanels()
    {
        foreach (Transform Child in SearchedOrderStatusListParent.transform)
        {
            Destroy(Child.gameObject);
        }
    }
}
