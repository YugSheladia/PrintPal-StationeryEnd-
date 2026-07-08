using UnityEngine;

public class ArchivedOrderStatusInfo : MonoBehaviour
{
    Scene3Manager Scene3ManagerScript;

    GameObject ArchivedOrdersStatusPanel;

    public void SetScene3ManagerScript(Scene3Manager Scene3ManagerScript)
    {
        this.Scene3ManagerScript = Scene3ManagerScript; 
    }

    public void SetArchivedOrdersStatusPanel(GameObject ArchivedOrdersStatusPanel)
    {
        this.ArchivedOrdersStatusPanel = ArchivedOrdersStatusPanel;
    }

    public void ClickedPreviousPageButton()
    {
        Destroy(gameObject);

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableArchivedOrdersPage();

        if (ArchivedOrdersStatusPanel != null)
        {
            ArchivedOrdersStatusPanel.SetActive(true);
        }
    }
}
