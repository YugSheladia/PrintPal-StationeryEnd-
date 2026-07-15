using UnityEngine;

public class OrderStatusInfo : MonoBehaviour
{
    Scene3Manager Scene3ManagerScript;

    GameObject OrdersStatusPanel;

    public void SetScene3ManagerScript(Scene3Manager Scene3ManagerScript)
    {
        this.Scene3ManagerScript = Scene3ManagerScript; 
    }

    public void SetOrdersStatusPanel(GameObject OrdersStatusPanel)
    {
        this.OrdersStatusPanel = OrdersStatusPanel;
    }

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
