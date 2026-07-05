using UnityEngine;

public class SearchedOrderStatusInfo : MonoBehaviour
{
    Scene3Manager Scene3ManagerScript;

    GameObject SearchedOrdersStatusPanel;

    public void SetScene3ManagerScript(Scene3Manager Scene3ManagerScript)
    {
        this.Scene3ManagerScript = Scene3ManagerScript; 
    }

    public void SetSearchedOrdersStatusPanel(GameObject SearchedOrdersStatusPanel)
    {
        this.SearchedOrdersStatusPanel = SearchedOrdersStatusPanel;
    }

    public void ClickedPreviousPageButton()
    {
        Destroy(gameObject);

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableSearchedOrdersPage();

        if (SearchedOrdersStatusPanel != null)
        {
            SearchedOrdersStatusPanel.SetActive(true);
        }
    }
}
