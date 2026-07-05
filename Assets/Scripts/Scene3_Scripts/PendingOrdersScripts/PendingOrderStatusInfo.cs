using UnityEngine;

public class PendingOrderStatusInfo : MonoBehaviour
{
    Scene3Manager Scene3ManagerScript;

    GameObject PendingOrdersStatusPanel;

    public void SetScene3ManagerScript(Scene3Manager Scene3ManagerScript)
    {
        this.Scene3ManagerScript = Scene3ManagerScript; 
    }

    public void SetPendingOrdersStatusPanel(GameObject PendingOrdersStatusPanel)
    {
        this.PendingOrdersStatusPanel = PendingOrdersStatusPanel;
    }

    public void ClickedPreviousPageButton()
    {
        Destroy(gameObject);

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnablePendingOrdersPage();

        if (PendingOrdersStatusPanel != null)
        {
            PendingOrdersStatusPanel.SetActive(true);
        }
    }
}
