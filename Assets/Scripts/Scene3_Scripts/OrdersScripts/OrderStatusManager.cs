//namespaces =
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;




public class OrderStatusManager : MonoBehaviour
{
    //Script references =
    [SerializeField] Scene3Manager Scene3ManagerScript;
    [SerializeField] Scene3Backend Scene3BackendScript;

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
        //Starting from a clean list every time, even if there are no orders to show.
        //( Previously this happened after the "no orders" return, so old panels stayed in the list. )
        OrderStatus_Panels.Clear();

        TotalSelectedOrders = 0;
        IsBusy = false;
        UpdateUI();

        if (Scene3BackendScript.ConfirmedOrders.Orders.Length <= 0)
        {
            //In future, if I ever want to make a "No Orders exist" wala object, it should be instantitated here.

            return;
        }

        foreach (OrderSummary Order in Scene3BackendScript.ConfirmedOrders.Orders)
        {
            CreateOrderStatusPanel(Order);
        }

        UpdateUI();
    }

    //Called by CreateOrderStatusPanels() to instantiate a panel for each order in Scene3Backend.ConfirmedOrders.Orders
    public void CreateOrderStatusPanel(OrderSummary Order)
    {
        //To check if the same order already exists.
        foreach (OrderStatus Panel in OrderStatusListParent.GetComponentsInChildren<OrderStatus>())
        {
            if (Panel.Order != null && Panel.Order.OrderID == Order.OrderID) { return; }
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








    //Removes a OrderStatus panel from the list of panels ( called when its order leaves this page, e.g. it was marked as something else ).
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
        if (Has_SelectedAll)
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
        //Panels which were destroyed ( e.g. after leaving the page ) are dropped from the list first.
        OrderStatus_Panels.RemoveAll(Panel => Panel == null);

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
        Has_SelectedAll =
            OrderStatus_Panels.Count > 0
            && TotalSelectedOrders == OrderStatus_Panels.Count;
    }










    //==============================================================
    //Changing the status of orders ( one order, or many selected ).
    //==============================================================
    //
    //Why this is done one order after another:
    //Before, every selected order sent its request at the same moment, and the "Marked N as ..." message
    //was shown before the server had answered. Now the requests are sent one at a time, and the message is
    //shown once all of them are finished, saying how many really worked.

    public enum OrderAction
    {
        Pending,
        Printed,
        Collected,
        Failed,
        Archived,
        Unarchived
    }

    //True while a change is running. Select / Unselect / Mark buttons do nothing during this time.
    public bool IsBusy = false;


    //Called by OrderStatus ( single order ) and by the Clicked_MarkSelectedOrdersAs_...() functions below ( many orders ).
    public void ApplyAction(List<OrderStatus> Orders, OrderAction Action, bool ShowSummary)
    {
        if (IsBusy)
        {
            Debug.Log("Already changing orders, ignoring this click.");
            return;
        }

        if (Orders == null || Orders.Count <= 0)
        {
            return;
        }

        //Scene3Backend is used to run it, as it always stays active ( this object might be switched off in the middle ).
        Scene3BackendScript.StartCoroutine(Coroutine_ApplyAction(Orders, Action, ShowSummary));
    }

    private IEnumerator Coroutine_ApplyAction(List<OrderStatus> Orders, OrderAction Action, bool ShowSummary)
    {
        IsBusy = true;

        Scene3ManagerScript.EnableInvisiblePanel();

        int Succeeded = 0;
        int Failed = 0;

        //A copy, because panels are removed from OrderStatus_Panels while we go through the orders.
        List<OrderStatus> Snapshot = new List<OrderStatus>(Orders);

        foreach (OrderStatus Panel in Snapshot)
        {
            //The panel was destroyed in the meantime.
            if (Panel == null || Panel.Order == null)
            {
                continue;
            }

            int OrderID = Panel.Order.OrderID;

            bool Ok = true;


            //An archived order has to be unarchived first, when it is being given any other status.
            if (Panel.IsArchived && Action != OrderAction.Archived && Action != OrderAction.Unarchived)
            {
                bool Unarchived = false;

                yield return Scene3BackendScript.SetOrderAsUnarchived(OrderID, Result => Unarchived = Result);

                if (Unarchived)
                {
                    Panel.MarkedAsUnarchivedLocally();
                }
                else
                {
                    Ok = false;
                }
            }


            if (Ok)
            {
                bool Changed = false;

                switch (Action)
                {
                    case OrderAction.Pending:
                        yield return Scene3BackendScript.SetOrderAsPending(OrderID, Result => Changed = Result);
                        break;

                    case OrderAction.Printed:
                        yield return Scene3BackendScript.SetOrderAsCompleted(OrderID, Result => Changed = Result);
                        break;

                    case OrderAction.Collected:
                        yield return Scene3BackendScript.SetOrderAsCollected(OrderID, Result => Changed = Result);
                        break;

                    case OrderAction.Failed:
                        yield return Scene3BackendScript.SetOrderAsFailed(OrderID, Result => Changed = Result);
                        break;

                    case OrderAction.Archived:
                        yield return Scene3BackendScript.SetOrderAsArchived(OrderID, Result => Changed = Result);
                        break;

                    case OrderAction.Unarchived:
                        yield return Scene3BackendScript.SetOrderAsUnarchived(OrderID, Result => Changed = Result);
                        break;
                }

                Ok = Changed;
            }


            if (Ok)
            {
                Succeeded++;

                //The order doesn't belong to this page anymore, so its panel goes away.
                //Panel.Unselect() is called first so the selected counter stays right.
                OrderStatus_Panels.Remove(Panel);

                if (Panel != null)
                {
                    Destroy(Panel.gameObject);
                }
            }
            else
            {
                Failed++;

                //A failed order stays on the page, and stays selected so it can be tried again.
                Debug.LogError("Could not change Order " + OrderID + " to " + Action);
            }
        }


        IsBusy = false;

        Scene3ManagerScript.DisableInvisiblePanel();

        UpdateUI();


        //Telling the user what happened.
        if (Failed > 0)
        {
            Scene3ManagerScript.MessageText.text =
                Succeeded > 0
                ? "Marked " + Succeeded + " as " + GetActionName(Action) + ". " + Failed + " failed, try again later."
                : "Couldn't mark as " + GetActionName(Action) + ", try again later.";

            Scene3ManagerScript.EnableMessagePanel();
        }
        else if (ShowSummary && Succeeded > 0)
        {
            Scene3ManagerScript.MessageText.text = "Marked " + Succeeded + " as " + GetActionName(Action) + ".";
            Scene3ManagerScript.EnableMessagePanel();
        }
    }

    private string GetActionName(OrderAction Action)
    {
        return Action.ToString();
    }


    //Returns all orders which are currently selected.
    private List<OrderStatus> GetSelectedOrders()
    {
        List<OrderStatus> Selected = new List<OrderStatus>();

        foreach (OrderStatus OrderStatusScript in OrderStatus_Panels)
        {
            if (OrderStatusScript != null && OrderStatusScript.IsSelected)
            {
                Selected.Add(OrderStatusScript);
            }
        }

        return Selected;
    }


    //Shared by all Clicked_MarkSelectedOrdersAs_...() functions =
    //returns false ( after telling the user why ) if nothing should be done.
    private bool CanMarkSelected(bool AlreadyThatStatus, string AlreadyMessage)
    {
        if (IsBusy)
        {
            return false;
        }

        Update_TotalNumberOfSelectedOrders();

        if (TotalSelectedOrders <= 0)
        {
            Debug.Log("0 orders have been selected.");

            return false;
        }

        if (AlreadyThatStatus)
        {
            Debug.Log(AlreadyMessage);

            Scene3ManagerScript.MessageText.text = AlreadyMessage;
            Scene3ManagerScript.EnableMessagePanel();

            return false;
        }

        return true;
    }




    //Mark n orders as Pending / Printed / Collected / Failed / Archived / Unarchived button.

    public void Clicked_MarkSelectedOrdersAs_Pending()
    {
        if (!CanMarkSelected(CurrentStatus == CurrentStatus_OfAllOrders.Pending, "Selected orders are already marked as Pending"))
        {
            return;
        }

        ApplyAction(GetSelectedOrders(), OrderAction.Pending, true);
    }

    public void Clicked_MarkSelectedOrdersAs_Printed()
    {
        if (!CanMarkSelected(CurrentStatus == CurrentStatus_OfAllOrders.Printed, "Selected orders are already marked as Printed"))
        {
            return;
        }

        ApplyAction(GetSelectedOrders(), OrderAction.Printed, true);
    }

    public void Clicked_MarkSelectedOrdersAs_Collected()
    {
        if (!CanMarkSelected(CurrentStatus == CurrentStatus_OfAllOrders.Collected, "Selected orders are already marked as Collected"))
        {
            return;
        }

        ApplyAction(GetSelectedOrders(), OrderAction.Collected, true);
    }

    //NEW = there was no bulk "Failed" button before. To use it, add a button and point it to this function in the Inspector.
    public void Clicked_MarkSelectedOrdersAs_Failed()
    {
        if (!CanMarkSelected(CurrentStatus == CurrentStatus_OfAllOrders.Failed, "Selected orders are already marked as Failed"))
        {
            return;
        }

        ApplyAction(GetSelectedOrders(), OrderAction.Failed, true);
    }

    public void Clicked_MarkSelectedOrdersAs_Archived()
    {
        if (!CanMarkSelected(CurrentStatus == CurrentStatus_OfAllOrders.Archived, "Selected orders are already marked as Archived"))
        {
            return;
        }

        ApplyAction(GetSelectedOrders(), OrderAction.Archived, true);
    }

    public void Clicked_MarkSelectedOrdersAs_Unarchived()
    {
        if (!CanMarkSelected(CurrentStatus != CurrentStatus_OfAllOrders.Archived, "Selected orders are already marked as Unarchived"))
        {
            return;
        }

        ApplyAction(GetSelectedOrders(), OrderAction.Unarchived, true);
    }










    //Select / Unselect all stuff =

    public bool Has_SelectedAll = false;


    //This function is called when the user clicks on SelectAll button.
    public void Clicked_SelectAll()
    {
        if (IsBusy)
        {
            return;
        }

        foreach (OrderStatus OrderStatusScript in OrderStatus_Panels)
        {
            if (OrderStatusScript != null)
            {
                OrderStatusScript.SetSelected(true);
            }
        }

        UpdateUI();
    }


    //This function is called when the user clicks on UnselectAll button.
    public void Clicked_UnselectAll()
    {
        if (IsBusy)
        {
            return;
        }

        foreach (OrderStatus OrderStatusScript in OrderStatus_Panels)
        {
            if (OrderStatusScript != null)
            {
                OrderStatusScript.SetSelected(false);
            }
        }

        UpdateUI();
    }
}