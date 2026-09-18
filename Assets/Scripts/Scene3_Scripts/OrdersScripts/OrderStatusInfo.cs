//namespaces =
using UnityEngine;



public class OrderStatusInfo : MonoBehaviour
{
    //Script references =
    Scene3Manager Scene3ManagerScript;



    //Object references =
    GameObject OrdersStatusPanel;




    //Providing neccessary references =
    public void SetScene3ManagerScript(Scene3Manager Scene3ManagerScript)
    {
        this.Scene3ManagerScript = Scene3ManagerScript; 
    }

    public void SetOrdersStatusPanel(GameObject OrdersStatusPanel)
    {
        this.OrdersStatusPanel = OrdersStatusPanel;
    }




    //To go from OrderStatusInfo page -> OrderStatusManager page.
    public void ClickedPreviousPageButton()
    {
        Destroy(gameObject);

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableOrdersPage();

        if (OrdersStatusPanel != null)
        {
            OrdersStatusPanel.SetActive(true);
        }
    }
}
