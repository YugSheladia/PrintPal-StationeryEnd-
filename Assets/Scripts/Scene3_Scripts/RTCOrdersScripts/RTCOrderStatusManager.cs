using UnityEngine;

public class RTCOrderStatusManager : MonoBehaviour
{
    [SerializeField] Scene3Manager      Scene3ManagerScript;
    [SerializeField] Scene3Backend      Scene3BackendScript;


    [Header("Order Status")]
    [SerializeField] GameObject OrderStatusPrefab;
    [SerializeField] GameObject RTCOrderStatusListParent;

    [SerializeField] public GameObject RTCOrdersStatusPanel;



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
        foreach (RTCOrderStatus Panel in RTCOrderStatusListParent.GetComponentsInChildren<RTCOrderStatus>())
        {
            if (Panel.Order.OrderID == Order.OrderID) { return; }
        }

        GameObject OrderStatusObject = Instantiate(
                                                    OrderStatusPrefab,
                                                    RTCOrderStatusListParent.transform
                                                  );

        RTCOrderStatus RTCOrderStatusScript = OrderStatusObject.GetComponent<RTCOrderStatus>();

        RTCOrderStatusScript.SetRequiredScripts(Scene3ManagerScript, Scene3BackendScript, this);

        RTCOrderStatusScript.SetOrder(Order);

        Scene3BackendScript.GetUsernameFromServer(Order.UserID, RTCOrderStatusScript);
    }


    

    public void ClickedPreviousPageButton()
    {
        DeleteAllRTCOrderPanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableHomePagePanel();
    }
    private void DeleteAllRTCOrderPanels()
    {
        foreach (Transform Child in RTCOrderStatusListParent.transform)
        {
            Destroy(Child.gameObject);
        }
    }
}
