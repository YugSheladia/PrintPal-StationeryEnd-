using UnityEngine;

public class CollectedOrderStatusManager : MonoBehaviour
{
    [SerializeField] Scene3Manager      Scene3ManagerScript;
    [SerializeField] Scene3Backend      Scene3BackendScript;


    [Header("Order Status")]
    [SerializeField] GameObject OrderStatusPrefab;
    [SerializeField] GameObject CollectedOrderStatusListParent;

    [SerializeField] public GameObject CollectedOrdersStatusPanel;



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
        foreach (CollectedOrderStatus Panel in CollectedOrderStatusListParent.GetComponentsInChildren<CollectedOrderStatus>())
        {
            if (Panel.Order.OrderID == Order.OrderID) { return; }
        }

        GameObject OrderStatusObject = Instantiate(
                                                    OrderStatusPrefab,
                                                    CollectedOrderStatusListParent.transform
                                                  );

        CollectedOrderStatus CollectedOrderStatusScript = OrderStatusObject.GetComponent<CollectedOrderStatus>();


        CollectedOrderStatusScript.SetRequiredScripts(Scene3ManagerScript, Scene3BackendScript, this);

        CollectedOrderStatusScript.SetOrder(Order);

        Scene3BackendScript.GetUsernameFromServer(Order.UserID, CollectedOrderStatusScript);
    }


    

    public void ClickedPreviousPageButton()
    {
        DeleteAllCollectedOrderPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableHomePagePanel();
    }
    private void DeleteAllCollectedOrderPanels()
    {
        foreach (Transform Child in CollectedOrderStatusListParent.transform)
        {
            Destroy(Child.gameObject);
        }
    }
}
