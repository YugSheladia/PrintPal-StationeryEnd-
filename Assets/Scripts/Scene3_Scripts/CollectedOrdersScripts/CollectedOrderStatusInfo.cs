using UnityEngine;

public class CollectedOrderStatusInfo : MonoBehaviour
{
    Scene3Manager Scene3ManagerScript;

    GameObject CollectedOrdersStatusPanel;

    public void SetScene3ManagerScript(Scene3Manager Scene3ManagerScript)
    {
        this.Scene3ManagerScript = Scene3ManagerScript; 
    }

    public void SetCollectedOrdersStatusPanel(GameObject CollectedOrdersStatusPanel)
    {
        this.CollectedOrdersStatusPanel = CollectedOrdersStatusPanel;
    }

    public void ClickedPreviousPageButton()
    {
        Destroy(gameObject);

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableCollectedOrdersPage();

        if (CollectedOrdersStatusPanel != null)
        {
            CollectedOrdersStatusPanel.SetActive(true);
        }
    }
}
