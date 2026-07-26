using UnityEditor.Animations;
using UnityEngine;
using static Scene3Backend;

public class RevenuePageManager : MonoBehaviour
{
    [SerializeField] private Scene3Manager Scene3ManagerScript;
    [SerializeField] private Scene3Backend Scene3BackendScript;

    [SerializeField] private GameObject RevenuePanelPrefab;
    [SerializeField] private GameObject RevenuePanelsParent;







    public void LoadRevenue(int StationeryID)
    {
        Scene3BackendScript.GetRevenueFromServer
        (
            StationeryID,
            RevenueRows =>
            {
                if (RevenueRows == null)
                {                    
                    Scene3ManagerScript.DisableAllPanels();
                    Scene3ManagerScript.EnableHomePagePanel();

                    Scene3ManagerScript.EnableMessagePanel();

                    return;
                }

                foreach (RevenueDetails Row in RevenueRows)
                {
                    Debug.Log
                    (
                        $"{Row.RevenueDate} | " +
                        $"Revenue = {Row.Revenue} | " +
                        $"Orders = {Row.TotalOrders}"
                    );

                    CreateRevenuePanel(Row);
                }

                Scene3ManagerScript.DisableInvisiblePanel();
            }
        );
    }
    private void CreateRevenuePanel(RevenueDetails Revenue)
    {
        GameObject RevenuePanelObject = Instantiate
        (
            RevenuePanelPrefab,
            RevenuePanelsParent.transform
        );

        RevenuePanel RevenuePanelScript = RevenuePanelObject.GetComponent<RevenuePanel>();

        RevenuePanelScript.SetRevenuePanelInfo
        (
            Revenue.RevenueDate, 
            Revenue.Revenue, 
            Revenue.TotalOrders,
            Revenue.CompletedOrders,
            Revenue.TotalPagesPrinted
        );
    }




    public void ClickedPreviousPageButton()
    {
        DestroyAllRevenuePanels();

        Scene3ManagerScript.DisableAllPanels();
        Scene3ManagerScript.EnableHomePagePanel();
    }
    public void DestroyAllRevenuePanels()
    {
        foreach (Transform Child in RevenuePanelsParent.transform)
        {
            Destroy(Child.gameObject);
        }
    }
}