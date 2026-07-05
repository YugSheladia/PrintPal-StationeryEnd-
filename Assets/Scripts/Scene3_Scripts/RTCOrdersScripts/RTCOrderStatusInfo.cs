using UnityEngine;

public class RTCOrderStatusInfo : MonoBehaviour
{
    Scene3Manager Scene3ManagerScript;

    GameObject RTCOrdersStatusPanel;

    public void SetScene3ManagerScript(Scene3Manager Scene3ManagerScript)
    {
        this.Scene3ManagerScript = Scene3ManagerScript; 
    }

    public void SetRTCOrdersStatusPanel(GameObject RTCOrdersStatusPanel)
    {
        this.RTCOrdersStatusPanel = RTCOrdersStatusPanel;
    }

    public void ClickedPreviousPageButton()
    {
        Destroy(gameObject);

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableCompletedOrdersPage();

        if (RTCOrdersStatusPanel != null)
        {
            RTCOrdersStatusPanel.SetActive(true);
        }
    }
}
