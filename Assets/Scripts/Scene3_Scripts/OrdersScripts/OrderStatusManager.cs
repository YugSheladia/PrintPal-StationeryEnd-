//namespaces =
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    //A list of all created OrderStatus scripts;
    public static List<OrderStatus> OrderStatus_Panels = new();







    //This helps in knowing which type of OrderPanels are being instantiated in OrderStatusManager.

    public enum CurrentStatus_OfAllOrders
    {
        Pending,
        Printed,
        Collected,
        Archived,
        Failed
    }

    public CurrentStatus_OfAllOrders CurrentStatus;






    //UI stuff =

    //This is a panel which displays how many orders have been selected.
    //It also contains buttons like = Mark as Pending / Printed... etc.
    //It is activated only when an order is selected.

    [SerializeField] GameObject SelectedOrders_Panel;

    //This is the text which displays how many orders have been selected.
    [SerializeField] TMP_Text SelectedOrder_Text;


    //This is the button used to select all orders.
    [SerializeField] GameObject SelectAll_Button;


    //This is the burron used to unselect all orders.
    [SerializeField] GameObject UnselectAll_Button;








    //Called by HomePage when a user clicks on Pending / Printed / Collected orders.
    public void CreateOrderStatusPanels()
    {
        if (Scene3BackendScript.ConfirmedOrders.Orders.Length <= 0)
        {
            //In future, if I ever want to make a "No Orders exist" wala object, it should be instantitated here.

            return;
        }

        OrderStatus_Panels.Clear();

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

        OrderStatus_Panels.Add(OrderStatusScript);

        OrderStatusScript.SetRequiredScripts(Scene3ManagerScript, Scene3BackendScript, this);

        OrderStatusScript.SetOrder(Order);
    }




    

    //To go from OrderStatus page to HomePage.
    public void ClickedPreviousPageButton()
    {
        DeleteAllOrderPanels();

        OrderStatus_Panels.Clear();

        TotalSelectedOrders = 0;
        UpdateUI();

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








    //This function is called to remove a OrderStatus panel from OrderStatusPanel list before destroying it.
    public void Remove_OrderStatusPanel(OrderStatus OrderStatusScript)
    {
        OrderStatus_Panels.Remove(OrderStatusScript);

        UpdateUI();
    }









    //================================
    //SelectedOrdersPanel stuff.
    //================================


    //This helps in keeping a track of how many orders are selected.
    public int TotalSelectedOrders = 0;



    //This function is when the TotalSelectedOrders increases / decreased.
    public void UpdateUI()
    {
        Update_TotalNumberOfSelectedOrders();

        CheckIf_SelectedAllOrders();




        if (TotalSelectedOrders <= 0)
        {
            Debug.Log("No order has been selected.");

            //This is the panel which displays how many order have been selected, along with the buttons to mark them as Pending / Printed etc.
            SelectedOrders_Panel.SetActive(false);

            //This is the button which selects all orders, and is set to active because currently, no orders have been selected.
            SelectAll_Button.SetActive(true);

            //This is the button which unselects all orders, and is set to active only when all orders are selected, which is not the case currently, therefore
            //it is set to SetActive(false).
            UnselectAll_Button.SetActive(false);

            return;
        }





        //Updating number of selected orders.

        Debug.Log("Updating SelectedOrders text.");
        
        SelectedOrder_Text.text = "Mark " + TotalSelectedOrders + " as :";
        SelectedOrders_Panel.SetActive(true);






        //Checking if all orders are selected.
        if(Has_SelectedAll)
        {
            UnselectAll_Button.SetActive(true);
            SelectAll_Button.SetActive(false);
        }
        else
        {
            SelectAll_Button.SetActive(true);
            UnselectAll_Button.SetActive(false);
        }
    }



    //This function updates the total number of selected orders everytime a user Selects / Unselects an order.
    public void Update_TotalNumberOfSelectedOrders()
    {
        TotalSelectedOrders = 0;

        foreach (OrderStatus OrderStatusScript in OrderStatus_Panels)
        {
            if (OrderStatusScript.IsSelected)
            {
                TotalSelectedOrders++;
            }
        }
    }



    //This function checks if all orders are selected or not.
    public void CheckIf_SelectedAllOrders()
    {
        Has_SelectedAll = false;

        if (TotalSelectedOrders == OrderStatus_Panels.Count)
        {
            Has_SelectedAll = true;
        }
    }









    //===================================================================================
    //Mark N orders as Pending / Printed / Collected / Archived / Unarchived button.
    //===================================================================================

    //flow = Clicked_MarkSelectedOrdersAs_...() -> Coroutine_ProcessSelectedOrders()


    public void Clicked_MarkSelectedOrdersAs_Pending()
    {
        StartCoroutine
        (
            Coroutine_ProcessSelectedOrders
            (
                Order => Order.ClickedMarkAsPending(),
                () => CurrentStatus == CurrentStatus_OfAllOrders.Pending,
                "Selected orders are already marked as Pending",
                "Pending"
            )
        );
    }

    public void Clicked_MarkSelectedOrdersAs_Printed()
    {
        StartCoroutine
        (
            Coroutine_ProcessSelectedOrders
            (
                Order => Order.ClickedMarkAsPrinted(),
                () => CurrentStatus == CurrentStatus_OfAllOrders.Printed,
                "Selected orders are already marked as Printed",
                "Printed"
            )
        );
    }

    public void Clicked_MarkSelectedOrdersAs_Collected()
    {
        StartCoroutine
        (
            Coroutine_ProcessSelectedOrders
            (
                Order => Order.ClickedMarkAsCollected(),
                () => CurrentStatus == CurrentStatus_OfAllOrders.Collected,
                "Selected orders are already marked as Collected",
                "Collected"
            )
        );
    }

    public void Clicked_MarkSelectedOrdersAs_Archived()
    {
        StartCoroutine
        (
            Coroutine_ProcessSelectedOrders
            (
                Order => Order.ClickedArchiveOrderButton(),
                () => CurrentStatus == CurrentStatus_OfAllOrders.Archived,
                "Selected orders are already marked as Archived",
                "Archived"
            )
        );
    }

    public void Clicked_MarkSelectedOrdersAs_Unarchived()
    {
        StartCoroutine
        (
            Coroutine_ProcessSelectedOrders
            (
                Order => Order.ClickedUnarchiveOrderButton(),
                () => CurrentStatus != CurrentStatus_OfAllOrders.Archived,
                "Selected orders are already marked as Unarchived",
                "Unarchived"
            )
        );
    }



    private IEnumerator Coroutine_ProcessSelectedOrders
    (
        System.Action<OrderStatus> ProcessOrder,
        System.Func<bool> IsAlreadyInStatus,
        string AlreadyMessage,
        string StatusName
    )
    {
        if (TotalSelectedOrders <= 0)
        {
            Debug.Log("0 orders have been selected.");
            yield break;
        }





        if (IsAlreadyInStatus())
        {
            Debug.Log(AlreadyMessage);

            Scene3ManagerScript.MessageText.text = AlreadyMessage;
            Scene3ManagerScript.EnableMessagePanel();
            yield break;
        }





        int SelectedCount = TotalSelectedOrders;

        // Take a snapshot because processing can remove panels.
        OrderStatus[] StatusPanelSnapshot = OrderStatus_Panels.ToArray();

        foreach (OrderStatus OrderStatusScript in StatusPanelSnapshot)
        {
            if (!OrderStatusScript || !OrderStatusScript.IsSelected)
            {
                continue;
            }




            // Wait without freezing Unity.
            yield return new WaitUntil
            (
                () => !Scene3BackendScript.CheckFor_OngoingTransaction
            );





            // Start this order's transaction.
            ProcessOrder(OrderStatusScript);





            // Wait until this transaction finishes before continuing.
            yield return new WaitUntil
            (
                () => !Scene3BackendScript.CheckFor_OngoingTransaction
            );
        }

        Debug.Log
        (
            "Finished processing selected orders for status: " + StatusName
        );




        
        Scene3ManagerScript.MessageText.text = "Processed " + SelectedCount + " selected orders for " + StatusName + ".";
        Scene3ManagerScript.EnableMessagePanel();




        TotalSelectedOrders = 0;




        UpdateUI();
    }












    //Select / Unselect all stuff =

    public bool Has_SelectedAll = false;


    //This function is called when the user clicks on SelectAll button.
    public void Clicked_SelectAll()
    {
        foreach(OrderStatus OrderStatusScript in OrderStatus_Panels)
        {
            OrderStatusScript.IsSelected = false;

            OrderStatusScript.Clicked_SelectOrder();
        }

        Has_SelectedAll = true;

        UpdateUI();
    }

    
    //This function is called when the user clicks on UnselectAll button.
    public void Clicked_UnselectAll()
    {
        foreach (OrderStatus OrderStatusScript in OrderStatus_Panels)
        {
            OrderStatusScript.IsSelected = true;

            OrderStatusScript.Clicked_SelectOrder();
        }

        Has_SelectedAll = false;

        UpdateUI();
    }
}
