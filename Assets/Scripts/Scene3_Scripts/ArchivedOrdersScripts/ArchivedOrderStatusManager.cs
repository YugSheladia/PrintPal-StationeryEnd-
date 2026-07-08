using UnityEngine;

public class ArchivedOrderStatusManager : MonoBehaviour
{
    [SerializeField] Scene3Manager      Scene3ManagerScript;
    [SerializeField] Scene3Backend      Scene3BackendScript;


    [Header("Order Status")]
    [SerializeField] GameObject OrderStatusPrefab;
    [SerializeField] GameObject ArchivedOrderStatusListParent;


    [SerializeField] public GameObject ArchivedOrdersStatusPanel;



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
        foreach (ArchivedOrderStatus Panel in ArchivedOrderStatusListParent.GetComponentsInChildren<ArchivedOrderStatus>())
        {
            if (Panel.Order.OrderID == Order.OrderID) { return; }
        }

        GameObject OrderStatusObject = Instantiate(
                                                    OrderStatusPrefab,
                                                    ArchivedOrderStatusListParent.transform
                                                  );

        ArchivedOrderStatus ArchivedOrderStatusScript = OrderStatusObject.GetComponent<ArchivedOrderStatus>();

        ArchivedOrderStatusScript.SetRequiredScripts(Scene3ManagerScript, Scene3BackendScript, this);

        ArchivedOrderStatusScript.SetOrder(Order);

        Scene3BackendScript.GetUsernameFromServer(Order.UserID, ArchivedOrderStatusScript);
    }


    

    public void ClickedPreviousPageButton()
    {
        DeleteAllArchivedOrderPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableHomePagePanel();
    }
    private void DeleteAllArchivedOrderPanels()
    {
        foreach (Transform Child in ArchivedOrderStatusListParent.transform)
        {
            Destroy(Child.gameObject);
        }
    }
}
